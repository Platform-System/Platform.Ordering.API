using System.Text.Json;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using Platform.Ordering.API.Application.Abstractions.Payments;
using Platform.Ordering.API.Application.Common.Models.Payment;
using Platform.Ordering.API.Application.Features.Orders.Commands.Webhook;
using Platform.Ordering.API.Domain.Entities;
using Platform.Ordering.API.Domain.Enums;
using Platform.Ordering.API.Infrastructure.Configurations;

namespace Platform.Ordering.API.Infrastructure.Services;

/// <summary>
/// Service xử lý tích hợp thanh toán với cổng PayOS.
/// </summary>
public class PayOSService : IPaymentService
{
    private readonly PayOSClient _payOSClient;
    private readonly PaymentSettings _paymentSettings;

    public PayOSService(IOptions<PayOSClientOptions> payosOptions, IOptions<PaymentSettings> paymentSettings)
    {
        // Khởi tạo PayOS client từ cấu hình đã bind trong appsettings.
        // Đây là bộ key dùng để service này gọi sang PayOS API.
        var payos = payosOptions.Value;
        _payOSClient = new PayOSClient(
            payos.ClientId,
            payos.ApiKey,
            payos.ChecksumKey
        );

        _paymentSettings = paymentSettings.Value;
    }

    /// <summary>
    /// Tạo link thanh toán cho một order đang ở trạng thái Pending.
    /// </summary>
    public async Task<PaymentLinkResponse> CreatePaymentLink(Order order, CancellationToken cancellationToken = default)
    {
        if (order.Items == null || !order.Items.Any())
            throw new InvalidOperationException("Order has no items");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order is not pending");

        var items = order.Items.Select(x => new PaymentLinkItem
        {
            Name = x.Name,
            Quantity = x.Quantity,
            Price = x.Price
        }).ToList();

        var amount = items.Sum(x => x.Price * x.Quantity);

        // Tạo request gửi sang PayOS. Description nên ngắn, rõ nghĩa
        // và đủ để đối chiếu nhanh với order trong hệ thống nội bộ.
        var request = new CreatePaymentLinkRequest
        {
            OrderCode = order.OrderCode,
            Amount = amount,
            Description = $"Order {order.OrderCode}",
            ReturnUrl = _paymentSettings.ReturnUrl,
            CancelUrl = _paymentSettings.CancelUrl,
            Items = items
        };

        // Gọi PayOS để tạo payment link thực tế, kết quả trả về sẽ chứa
        // checkout url và payment link id để client điều hướng người dùng sang bước thanh toán.
        var response = await _payOSClient.PaymentRequests.CreateAsync(
            request,
            new RequestOptions<CreatePaymentLinkRequest>
            {
                CancellationToken = cancellationToken
            });

        return new PaymentLinkResponse
        {
            CheckoutUrl = response.CheckoutUrl,
            PaymentLinkId = response.PaymentLinkId,
            Amount = response.Amount,
            Currency = response.Currency,
            Status = MapPayOSStatus(response.Status)
        };
    }

    /// <summary>
    /// Xác thực payload webhook do PayOS gửi về.
    /// </summary>
    public async Task<WebhookRequest?> VerifyWebhook(string rawBody)
    {
        var webhook = JsonSerializer.Deserialize<Webhook>(rawBody);

        if (webhook == null)
            return null;

        // Verify chữ ký để chắc dữ liệu webhook thực sự đến từ PayOS.
        var verified = await _payOSClient.Webhooks.VerifyAsync(webhook);

        return new WebhookRequest
        {
            OrderCode = verified.OrderCode,
            Code = verified.Code,
            Signature = webhook.Signature,
            RawBody = rawBody
        };
    }

    /// <summary>
    /// Lấy chi tiết payment link từ PayOS để kiểm tra trạng thái hiện tại.
    /// </summary>
    public async Task<PaymentLink> GetOrderDetails(string paymentLinkId)
    {
        return await _payOSClient.PaymentRequests.GetAsync(paymentLinkId);
    }

    /// <summary>
    /// Hủy payment link trên PayOS khi order không còn tiếp tục thanh toán.
    /// </summary>
    public async Task<PaymentLink> CancelOrder(string paymentLinkId, string? reason = null)
    {
        return await _payOSClient.PaymentRequests.CancelAsync(paymentLinkId, reason);
    }

    /// <summary>
    /// Xác nhận webhook URL để PayOS biết nơi callback về hệ thống này.
    /// </summary>
    public async Task<ConfirmWebhookResponse> ConfirmWebhook(string webhookUrl)
    {
        return await _payOSClient.Webhooks.ConfirmAsync(webhookUrl);
    }

    // Map trạng thái từ SDK PayOS sang enum payment nội bộ để application
    // không phụ thuộc trực tiếp vào enum của thư viện ngoài.
    private PaymentStatus MapPayOSStatus(PaymentLinkStatus status)
    {
        return status switch
        {
            PaymentLinkStatus.Pending => PaymentStatus.Pending,
            PaymentLinkStatus.Cancelled => PaymentStatus.Cancelled,
            PaymentLinkStatus.Paid => PaymentStatus.Paid,
            _ => PaymentStatus.Pending
        };
    }
}
