using Platform.BuildingBlocks.DateTimes;
using Platform.Domain.Common;
using Platform.Ordering.API.Domain.Errors;
using Platform.Ordering.API.Domain.Enums;

namespace Platform.Ordering.API.Domain.Entities;

public sealed class Payment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public string? PaymentLinkId { get; private set; }
    public string? CheckoutUrl { get; private set; }
    public long Amount { get; private set; }
    public string? Currency { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public PaymentStatus Status { get; private set; }

    private Payment()
    {
    }

    public Payment(Guid orderId)
    {
        OrderId = orderId;
        Status = PaymentStatus.Pending;
    }

    public static Payment Load(
        Guid id,
        Guid orderId,
        string? paymentLinkId,
        string? checkoutUrl,
        long amount,
        string? currency,
        DateTime? paidAt,
        PaymentStatus status)
    {
        return new Payment
        {
            Id = id,
            OrderId = orderId,
            PaymentLinkId = paymentLinkId,
            CheckoutUrl = checkoutUrl,
            Amount = amount,
            Currency = currency,
            PaidAt = paidAt,
            Status = status
        };
    }

    public DomainResult MarkAsPaid()
    {
        if (Status != PaymentStatus.Pending)
            return DomainResult.Failure(OrderingErrors.Payment.CannotMarkPaid);

        Status = PaymentStatus.Paid;
        PaidAt = Clock.Now;
        return DomainResult.Success();
    }

    public DomainResult MarkAsCancelled()
    {
        if (Status != PaymentStatus.Pending)
            return DomainResult.Failure(OrderingErrors.Payment.CannotMarkCancelled);

        Status = PaymentStatus.Cancelled;
        return DomainResult.Success();
    }

    public DomainResult SetCheckoutUrl(string? checkoutUrl)
    {
        if (string.IsNullOrWhiteSpace(checkoutUrl))
            return DomainResult.Failure(OrderingErrors.Payment.InvalidCheckoutUrl);

        CheckoutUrl = checkoutUrl;
        return DomainResult.Success();
    }

    public DomainResult SetCheckout(string checkoutUrl, string paymentLinkId, long amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(checkoutUrl))
            return DomainResult.Failure(OrderingErrors.Payment.InvalidCheckoutUrl);

        if (string.IsNullOrWhiteSpace(paymentLinkId))
            return DomainResult.Failure(OrderingErrors.Payment.InvalidPaymentLinkId);

        if (amount < 0)
            return DomainResult.Failure(OrderingErrors.Payment.InvalidAmount);

        if (string.IsNullOrWhiteSpace(currency))
            return DomainResult.Failure(OrderingErrors.Payment.InvalidCurrency);

        CheckoutUrl = checkoutUrl;
        PaymentLinkId = paymentLinkId;
        Amount = amount;
        Currency = currency;
        return DomainResult.Success();
    }
}
