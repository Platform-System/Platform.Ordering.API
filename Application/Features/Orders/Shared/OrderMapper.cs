using Platform.Ordering.API.Domain.Entities;
using Platform.Ordering.API.Infrastructure.Persistence.Models;

namespace Platform.Ordering.API.Application.Features.Orders.Shared;

public static class OrderMapper
{
    public static OrderResponse ToResponse(this Order order, string? checkoutUrl = null)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderCode = order.OrderCode,
            TotalAmount = order.TotalAmount,
            ExpiredAt = order.ExpiredAt,
            Status = order.Status,
            CheckoutUrl = checkoutUrl,
            Items = order.Items.Select(item => new OrderItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity
            }).ToList()
        };
    }

    public static Order ToDomain(this OrderModel model)
    {
        var order = Order.Load(
            model.Id,
            model.UserId,
            model.OrderCode,
            model.TotalAmount,
            model.ExpiredAt,
            model.Status);

        foreach (var item in model.Items.Select(ToDomain))
        {
            var addItemResult = order.AddItem(item.ProductId, item.Name, item.Price, item.Quantity);
            if (addItemResult.IsSuccess)
            {
                var domainItem = order.Items.Last();
                domainItem.Id = item.Id;
            }
        }

        return order;
    }

    public static OrderItem ToDomain(this OrderItemModel model)
    {
        var item = new OrderItem(model.OrderId, model.ProductId, model.Name, model.Price, model.Quantity)
        {
            Id = model.Id
        };

        return item;
    }

    public static Payment ToDomain(this PaymentModel model)
    {
        return Payment.Load(
            model.Id,
            model.OrderId,
            model.PaymentLinkId,
            model.CheckoutUrl,
            model.Amount,
            model.Currency,
            model.PaidAt,
            model.Status);
    }

    public static OrderModel ToModel(this Order order)
    {
        return new OrderModel
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderCode = order.OrderCode,
            TotalAmount = order.TotalAmount,
            ExpiredAt = order.ExpiredAt,
            Status = order.Status,
            Items = order.Items.Select(item => item.ToModel()).ToList()
        };
    }

    public static OrderItemModel ToModel(this OrderItem item)
    {
        return new OrderItemModel
        {
            Id = item.Id,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            Name = item.Name,
            Price = item.Price,
            Quantity = item.Quantity
        };
    }

    public static PaymentModel ToModel(this Payment payment)
    {
        return new PaymentModel
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            PaymentLinkId = payment.PaymentLinkId,
            CheckoutUrl = payment.CheckoutUrl,
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaidAt = payment.PaidAt,
            Status = payment.Status
        };
    }

    public static void ApplyDomainState(this PaymentModel model, Payment payment)
    {
        model.OrderId = payment.OrderId;
        model.PaymentLinkId = payment.PaymentLinkId;
        model.CheckoutUrl = payment.CheckoutUrl;
        model.Amount = payment.Amount;
        model.Currency = payment.Currency;
        model.PaidAt = payment.PaidAt;
        model.Status = payment.Status;
    }
}
