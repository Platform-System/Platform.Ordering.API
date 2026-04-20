using Platform.Ordering.API.Domain.Entities;
using Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;
using Platform.Ordering.API.Infrastructure.Persistence.Models;

namespace Platform.Ordering.API.Application.Features.Carts.Shared;

public static class CartMapper
{
    public static CartResponse ToResponse(this Cart cart)
    {
        return new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(item => new CartItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Type = item.Type,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity
            }).ToList(),
            TotalAmount = cart.GetTotalAmount()
        };
    }

    public static Cart ToDomain(this CartModel model)
    {
        var cart = new Cart(model.UserId)
        {
            Id = model.Id
        };

        foreach (var item in model.Items.Select(ToDomain))
        {
            cart.AddItem(item);
        }

        return cart;
    }

    public static CartItem ToDomain(this CartItemModel model)
    {
        var itemResult = CartItem.Create(model.CartId, model.ProductId, model.Type, model.Name, model.Price, model.Quantity);
        var item = itemResult.Value;
        item.Id = model.Id;
        return item;
    }

    public static CartItemModel ToModel(this ProductCartSnapshot product, int quantity)
    {
        return new CartItemModel
        {
            ProductId = product.Id,
            Type = product.Type,
            Name = product.Title,
            Price = product.Price,
            Quantity = quantity
        };
    }
}
