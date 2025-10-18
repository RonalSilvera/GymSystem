namespace Domain.Entity;

public class Payments
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int MembershipId { get; set; }
    public int PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
    public string? ReferenceText { get; set; }
    public int? ParentPaymentId { get; set; }
    public int InvoiceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public int StatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clients Client { get; set; } = null!;
    public Memberships Membership { get; set; } = null!;
    public PaymentMethods PaymentMethod { get; set; } = null!;
    public Status Status { get; set; } = null!;
    public Payments? ParentPayment { get; set; }
    public Coupons? Coupon { get; set; }
}
