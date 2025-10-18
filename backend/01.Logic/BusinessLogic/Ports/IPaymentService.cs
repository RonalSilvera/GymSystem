namespace BusinessLogic.Ports;

public interface IPaymentService
{
    Task<PaymentReceipt> CreateAsync(int clientId, int membershipTypeId, int paymentMethodId,
        decimal? discountAmount, string? couponCode, string? referenceText, DateTime now);
    Task<PaymentReceipt> CreatePartialAsync(int clientId, int membershipTypeId, int paymentMethodId,
        decimal amount, string? referenceText, DateTime now);
    Task<RefundReceipt> RefundAsync(int paymentId, string reason, DateTime now);
    Task<PaymentTicket> GetTicketAsync(int paymentId);
}

public record PaymentReceipt(int PaymentId, int InvoiceNumber, decimal TotalPaid, int MembershipId, DateTime StartDate, DateTime EndDate);
public record RefundReceipt(int RefundId, int InvoiceNumber);
public record PaymentTicket(int InvoiceNumber, DateTime Date, string Client, string MembershipType,
    decimal Amount, decimal Discount, decimal Total, string PaymentMethod, string? Reference);
