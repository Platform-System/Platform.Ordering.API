using Platform.Ordering.API.Domain.Entities;
using Platform.Ordering.API.Infrastructure.Persistence.Models;

namespace Platform.Ordering.API.Application.Features.Orders.Shared;

public static class OrderMapper
{
    public static OrderResponse ToResponse(this Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderCode = order.OrderCode,
            TotalAmount = order.TotalAmount,
            ExpiredAt = order.ExpiredAt,
            Status = order.Status,
            Items = order.Items.Select(item => new OrderItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Type = item.Type,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity
            }).ToList()
        };
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
            Type = item.Type,
            Name = item.Name,
            Price = item.Price,
            Quantity = item.Quantity
        };
    }
}
