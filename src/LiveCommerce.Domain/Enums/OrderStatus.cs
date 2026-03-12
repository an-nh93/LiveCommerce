namespace LiveCommerce.Domain.Enums;

public enum OrderStatus
{
    Draft = 0,
    PendingConfirm = 1,
    Confirmed = 2,
    Packed = 3,
    Shipping = 4,
    Delivered = 5,
    Cancelled = 6,
    Returned = 7,
    CODFailed = 8
}
