using Platform.Domain.Common;
using Platform.SharedKernel.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.Ordering.API.Infrastructure.Persistence.Models;

[Table("CartItems")]
public sealed class CartItemModel : Entity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public ProductKind Type { get; set; }
    public string Name { get; set; } = null!;
    public long Price { get; set; }
    public int Quantity { get; set; }

    [ForeignKey(nameof(CartId))]
    public CartModel Cart { get; set; } = null!;
}
