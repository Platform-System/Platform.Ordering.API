using Platform.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.Ordering.API.Infrastructure.Persistence.Models;

[Table("Carts")]
public sealed class CartModel : Entity
{
    public Guid UserId { get; set; }
    public ICollection<CartItemModel> Items { get; set; } = [];
}
