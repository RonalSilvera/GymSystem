using BusinessLogic.Ports;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IFactoryLogic _factory;

    public PaymentsController(IFactoryLogic factory)
    {
        _factory = factory;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreatePaymentModule(tenant);
        var result = await service.CreateAsync(request.ClientId, request.MembershipTypeId, request.PaymentMethodId,
            request.DiscountAmount, request.CouponCode, request.ReferenceText, DateTime.UtcNow);
        return Ok(new
        {
            paymentId = result.PaymentId,
            invoiceNumber = result.InvoiceNumber,
            totalPaid = result.TotalPaid,
            membershipId = result.MembershipId,
            startDate = result.StartDate,
            endDate = result.EndDate
        });
    }

    [HttpPost("partial")]
    public async Task<IActionResult> CreatePartial([FromBody] CreatePartialPaymentRequest request)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreatePaymentModule(tenant);
        var result = await service.CreatePartialAsync(request.ClientId, request.MembershipTypeId, request.PaymentMethodId,
            request.Amount, request.ReferenceText, DateTime.UtcNow);
        return Ok(new
        {
            paymentId = result.PaymentId,
            invoiceNumber = result.InvoiceNumber,
            totalPaid = result.TotalPaid,
            membershipId = result.MembershipId,
            startDate = result.StartDate,
            endDate = result.EndDate
        });
    }

    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] RefundPaymentRequest request)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreatePaymentModule(tenant);
        var result = await service.RefundAsync(request.PaymentId, request.Reason, DateTime.UtcNow);
        return Ok(new { refundId = result.RefundId, invoiceNumber = result.InvoiceNumber });
    }

    [HttpGet("{id}/ticket")]
    public async Task<IActionResult> Ticket(int id)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreatePaymentModule(tenant);
        var ticket = await service.GetTicketAsync(id);
        return Ok(ticket);
    }
}

public record CreatePaymentRequest(int ClientId, int MembershipTypeId, int PaymentMethodId, decimal? DiscountAmount, string? CouponCode, string? ReferenceText);
public record RefundPaymentRequest(int PaymentId, string Reason);
public record CreatePartialPaymentRequest(int ClientId, int MembershipTypeId, int PaymentMethodId, decimal Amount, string? ReferenceText);
