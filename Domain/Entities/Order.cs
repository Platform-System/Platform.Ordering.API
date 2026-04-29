using Platform.BuildingBlocks.DateTimes;
using Platform.Domain.Common;
using Platform.Ordering.API.Domain.Errors;
using Platform.Ordering.API.Domain.Enums;

namespace Platform.Ordering.API.Domain.Entities;

public sealed class Order : AggregateRoot
{
    public Guid UserId { get; private set; }
    public long OrderCode { get; private set; }
    public long TotalAmount { get; private set; }
    public DateTime ExpiredAt { get; private set; }
    public OrderStatus Status { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<Payment> _payments = [];
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private Order()
    {
    }

    public Order(Guid userId)
    {
        UserId = userId;
        OrderCode = GenerateOrderCode(Clock.Now);
        ExpiredAt = Clock.Now.AddMinutes(15);
        Status = OrderStatus.Pending;
    }

    public static Order Load(Guid id, Guid userId, long orderCode, long totalAmount, DateTime expiredAt, OrderStatus status)
    {
        return new Order
        {
            Id = id,
            UserId = userId,
            OrderCode = orderCode,
            TotalAmount = totalAmount,
            ExpiredAt = expiredAt,
            Status = status
        };
    }

    public DomainResult AddItem(Guid productId, string name, long price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainResult.Failure(OrderingErrors.Order.InvalidItemName);

        if (price < 0)
            return DomainResult.Failure(OrderingErrors.Order.InvalidItemPrice);

        if (quantity <= 0)
            return DomainResult.Failure(OrderingErrors.Order.InvalidItemQuantity);

        var existingItem = Items.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new OrderItem(Id, productId, name, price, quantity));
        }

        TotalAmount = GetTotalAmount();
        return DomainResult.Success();
    }

    public long GetTotalAmount()
    {
        return Items.Sum(x => x.Price * x.Quantity);
    }

    public DomainResult AddPayment(Payment payment)
    {
        if (payment.OrderId != Id)
            return DomainResult.Failure(OrderingErrors.Order.PaymentDoesNotBelongToOrder);

        _payments.Add(payment);
        return DomainResult.Success();
    }

    public DomainResult MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
            return DomainResult.Failure(OrderingErrors.Order.CannotMarkPaid);

        Status = OrderStatus.Paid;
        return DomainResult.Success();
    }

    public DomainResult MarkAsFailed()
    {
        if (Status != OrderStatus.Pending)
            return DomainResult.Failure(OrderingErrors.Order.CannotMarkFailed);

        Status = OrderStatus.Failed;
        return DomainResult.Success();
    }

    public DomainResult MarkAsCancelled()
    {
        if (Status != OrderStatus.Pending)
            return DomainResult.Failure(OrderingErrors.Order.CannotMarkCancelled);

        Status = OrderStatus.Cancelled;
        return DomainResult.Success();
    }

    private static long GenerateOrderCode(DateTime createdAt)
    {
        var prefix = createdAt.ToString("yyMMddHHmm");
        var randomPart = Random.Shared.Next(1000, 9999);

        return long.Parse($"{prefix}{randomPart}");
    }
}
