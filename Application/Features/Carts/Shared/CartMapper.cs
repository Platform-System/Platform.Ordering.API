using Platform.Ordering.API.Domain.Entities;
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

    public static CartModel ToModel(this Cart cart)
    {
        return new CartModel
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(item => item.ToModel()).ToList()
        };
    }

    public static void UpdateModel(this Cart cart, CartModel model)
    {
        model.UserId = cart.UserId;

        var domainItemsById = cart.Items.ToDictionary(item => item.Id);
        var modelItemsById = model.Items.ToDictionary(item => item.Id);

        var removedItems = model.Items
            .Where(item => !domainItemsById.ContainsKey(item.Id))
            .ToList();

        foreach (var removedItem in removedItems)
        {
            model.Items.Remove(removedItem);
        }

        foreach (var domainItem in cart.Items)
        {
            if (modelItemsById.TryGetValue(domainItem.Id, out var existingModelItem))
            {
                existingModelItem.CartId = domainItem.CartId;
                existingModelItem.ProductId = domainItem.ProductId;
                existingModelItem.Type = domainItem.Type;
                existingModelItem.Name = domainItem.Name;
                existingModelItem.Price = domainItem.Price;
                existingModelItem.Quantity = domainItem.Quantity;
                continue;
            }

            model.Items.Add(domainItem.ToModel());
        }
    }

    public static CartItemModel ToModel(this CartItem item)
    {
        return new CartItemModel
        {
            Id = item.Id,
            CartId = item.CartId,
            ProductId = item.ProductId,
            Type = item.Type,
            Name = item.Name,
            Price = item.Price,
            Quantity = item.Quantity
        };
    }
}
