using Platform.Domain.Common;

namespace Platform.Ordering.API.Domain.Errors;

public static class OrderingErrors
{
    public static class Cart
    {
        public static Error InvalidItemName => new(
            "Cart.InvalidItemName",
            "The cart item name is required.");

        public static Error InvalidItemPrice => new(
            "Cart.InvalidItemPrice",
            "The cart item price must be greater than or equal to zero.");

        public static Error InvalidItemQuantity => new(
            "Cart.InvalidItemQuantity",
            "The cart item quantity must be greater than zero.");

        public static Error ItemNotFound => new(
            "Cart.ItemNotFound",
            "The cart item was not found.");

        public static Error ItemDoesNotBelongToCart => new(
            "Cart.ItemDoesNotBelongToCart",
            "The cart item does not belong to this cart.");
    }

    public static class Order
    {
        public static Error InvalidItemName => new(
            "Order.InvalidItemName",
            "The order item name is required.");

        public static Error InvalidItemPrice => new(
            "Order.InvalidItemPrice",
            "The order item price must be greater than or equal to zero.");

        public static Error InvalidItemQuantity => new(
            "Order.InvalidItemQuantity",
            "The order item quantity must be greater than zero.");

        public static Error PaymentDoesNotBelongToOrder => new(
            "Order.PaymentDoesNotBelongToOrder",
            "The payment does not belong to this order.");

        public static Error CannotMarkPaid => new(
            "Order.CannotMarkPaid",
            "Only pending orders can be marked as paid.");

        public static Error CannotMarkFailed => new(
            "Order.CannotMarkFailed",
            "Only pending orders can be marked as failed.");

        public static Error CannotMarkCancelled => new(
            "Order.CannotMarkCancelled",
            "Only pending orders can be marked as cancelled.");
    }

    public static class Payment
    {
        public static Error CannotMarkPaid => new(
            "Payment.CannotMarkPaid",
            "Only pending payments can be marked as paid.");

        public static Error CannotMarkCancelled => new(
            "Payment.CannotMarkCancelled",
            "Only pending payments can be marked as cancelled.");

        public static Error InvalidCheckoutUrl => new(
            "Payment.InvalidCheckoutUrl",
            "The checkout URL is required.");

        public static Error InvalidPaymentLinkId => new(
            "Payment.InvalidPaymentLinkId",
            "The payment link id is required.");

        public static Error InvalidAmount => new(
            "Payment.InvalidAmount",
            "The payment amount must be greater than or equal to zero.");

        public static Error InvalidCurrency => new(
            "Payment.InvalidCurrency",
            "The payment currency is required.");
    }
}
