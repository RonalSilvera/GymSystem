using System.Linq;
using BusinessLogic.Adapter.Membership;
using BusinessLogic.Ports;
using Domain.Entity;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Adapter.Payment;

public class PaymentService(AppDbContext context, IUnitOfWork unitOfWork) : IPaymentService
{
    private readonly AppDbContext _context = context;
    private readonly IUnitOfWork _uow = unitOfWork;
    private const int StatusActivo = 1;
    private const int StatusInactivo = 2;
    private const int StatusPendiente = 5;
    private const int StatusPagoParcial = 6;
    private const int StatusPagado = 7;

    public async Task<PaymentReceipt> CreateAsync(int clientId, int membershipTypeId, int paymentMethodId,
        decimal? discountAmount, string? couponCode, string? referenceText, DateTime now)
    {
        var mType = await _context.MembershipTypes.FirstAsync(t => t.Id == membershipTypeId);
        var paymentMethod = await _context.PaymentMethods.FirstAsync(pm => pm.Id == paymentMethodId);

        var amount = mType.Price;
        decimal manualDiscount = discountAmount ?? 0m;
        if (manualDiscount < 0 || manualDiscount > amount)
        {
            throw new ArgumentException("Invalid discount amount");
        }

        decimal couponDiscount = 0m;
        Coupons? coupon = null;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode);
            if (coupon == null || !coupon.Active)
            {
                throw new ArgumentException("Invalid coupon");
            }
            if (coupon.ValidFrom.HasValue && now < coupon.ValidFrom.Value)
            {
                throw new ArgumentException("Coupon not yet valid");
            }
            if (coupon.ValidTo.HasValue && now > coupon.ValidTo.Value)
            {
                throw new ArgumentException("Coupon expired");
            }
            if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses.Value)
            {
                throw new ArgumentException("Coupon exhausted");
            }
            couponDiscount = coupon.DiscountAmount ?? amount * (coupon.Percent!.Value / 100m);
        }

        if ((paymentMethod.Name == "Nequi" || paymentMethod.Name == "Bancolombia") && string.IsNullOrWhiteSpace(referenceText))
        {
            throw new ArgumentException("referenceText required for transfer methods");
        }

        var pending = await _context.Memberships
            .FirstOrDefaultAsync(m => m.ClientId == clientId && m.MembershipTypeId == membershipTypeId && m.StatusId == StatusPendiente);
        int membershipId;
        DateTime startDate;
        DateTime endDate;
        if (pending != null)
        {
            pending.StatusId = StatusActivo;
            pending.StartDate = now;
            pending.EndDate = now.AddDays(mType.DurationDays);
            pending.UpdatedAt = now;
            membershipId = pending.Id;
            startDate = pending.StartDate;
            endDate = pending.EndDate;
        }
        else
        {
            var membershipService = new MembershipService(_context, _uow);
            var activation = await membershipService.ActivateOrRenewAsync(clientId, membershipTypeId, null, now);
            membershipId = activation.MembershipId;
            startDate = activation.StartDate;
            endDate = activation.EndDate;
        }

        var nextInvoice = (await _context.Payments.MaxAsync(p => (int?)p.InvoiceNumber) ?? 0) + 1;

        var totalDiscount = manualDiscount + couponDiscount;
        if (totalDiscount > amount) totalDiscount = amount;

        if (coupon != null)
        {
            coupon.UsedCount += 1;
        }

        var payment = new Payments
        {
            ClientId = clientId,
            MembershipId = membershipId,
            PaymentMethodId = paymentMethodId,
            Amount = amount,
            DiscountAmount = totalDiscount,
            CouponCode = coupon?.Code,
            ReferenceText = referenceText,
            InvoiceNumber = nextInvoice,
            PaymentDate = now,
            StatusId = StatusActivo,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Payments.AddAsync(payment);
        await _uow.Complete();

        var totalPaid = payment.Amount - (payment.DiscountAmount ?? 0);
        return new PaymentReceipt(payment.Id, payment.InvoiceNumber, totalPaid, membershipId, startDate, endDate);
    }

    public async Task<PaymentReceipt> CreatePartialAsync(int clientId, int membershipTypeId, int paymentMethodId,
        decimal amount, string? referenceText, DateTime now)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Invalid partial amount");
        }

        var mType = await _context.MembershipTypes.FirstAsync(t => t.Id == membershipTypeId);
        if (mType.DurationDays < 30)
        {
            throw new ArgumentException("Partial payments only available for extended memberships");
        }
        var paymentMethod = await _context.PaymentMethods.FirstAsync(pm => pm.Id == paymentMethodId);
        if ((paymentMethod.Name == "Nequi" || paymentMethod.Name == "Bancolombia") && string.IsNullOrWhiteSpace(referenceText))
        {
            throw new ArgumentException("referenceText required for transfer methods");
        }

        var membership = await _context.Memberships
            .Include(m => m.MembershipType)
            .Include(m => m.Payments)
            .Where(m => m.ClientId == clientId && m.MembershipTypeId == membershipTypeId &&
                        (m.StatusId == StatusPendiente || m.StatusId == StatusPagoParcial))
            .OrderByDescending(m => m.EndDate)
            .FirstOrDefaultAsync();

        if (membership == null)
        {
            membership = new Memberships
            {
                ClientId = clientId,
                MembershipTypeId = membershipTypeId,
                StartDate = now,
                EndDate = now.AddDays(mType.DurationDays),
                StatusId = StatusPagoParcial,
                CreatedAt = now,
                UpdatedAt = now,
                MembershipType = mType
            };
            await _context.Memberships.AddAsync(membership);
        }
        else
        {
            if (membership.MembershipType == null)
            {
                membership.MembershipType = await _context.MembershipTypes.FirstAsync(t => t.Id == membershipTypeId);
            }
        }

        var price = membership.MembershipType?.Price ?? mType.Price;
        var totalPaid = membership.Payments
            .Where(p => p.StatusId == StatusActivo)
            .Sum(p => p.Amount - (p.DiscountAmount ?? 0m));
        var remaining = price - totalPaid;
        if (remaining <= 0)
        {
            throw new InvalidOperationException("Membership already paid");
        }
        if (amount > remaining)
        {
            throw new ArgumentException("Partial amount exceeds due balance");
        }

        if (membership.StatusId == StatusPendiente)
        {
            membership.StartDate = now;
            membership.EndDate = now.AddDays(membership.MembershipType?.DurationDays ?? mType.DurationDays);
        }

        var nextInvoice = (await _context.Payments.MaxAsync(p => (int?)p.InvoiceNumber) ?? 0) + 1;
        var payment = new Payments
        {
            ClientId = clientId,
            Membership = membership,
            PaymentMethodId = paymentMethodId,
            Amount = amount,
            DiscountAmount = null,
            ReferenceText = referenceText,
            InvoiceNumber = nextInvoice,
            PaymentDate = now,
            StatusId = StatusActivo,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Payments.AddAsync(payment);

        var totalPaidAfter = totalPaid + amount;
        membership.StatusId = totalPaidAfter >= price ? StatusPagado : StatusPagoParcial;
        membership.UpdatedAt = now;

        await _uow.Complete();

        return new PaymentReceipt(payment.Id, payment.InvoiceNumber, totalPaidAfter, membership.Id, membership.StartDate, membership.EndDate);
    }

    public async Task<RefundReceipt> RefundAsync(int paymentId, string reason, DateTime now)
    {
        var original = await _context.Payments.FirstAsync(p => p.Id == paymentId);
        original.StatusId = StatusInactivo;
        original.UpdatedAt = now;
        var nextInvoice = (await _context.Payments.MaxAsync(p => (int?)p.InvoiceNumber) ?? 0) + 1;

        var refund = new Payments
        {
            ClientId = original.ClientId,
            MembershipId = original.MembershipId,
            PaymentMethodId = original.PaymentMethodId,
            Amount = -original.Amount,
            DiscountAmount = original.DiscountAmount.HasValue ? -original.DiscountAmount.Value : null,
            CouponCode = original.CouponCode,
            ReferenceText = reason,
            InvoiceNumber = nextInvoice,
            PaymentDate = now,
            StatusId = StatusActivo,
            CreatedAt = now,
            UpdatedAt = now,
            ParentPaymentId = original.Id
        };

        await _context.Payments.AddAsync(refund);
        await _uow.Complete();

        return new RefundReceipt(refund.Id, refund.InvoiceNumber);
    }

    public async Task<PaymentTicket> GetTicketAsync(int paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Client)
            .Include(p => p.PaymentMethod)
            .Include(p => p.Membership)
                .ThenInclude(m => m.MembershipType)
            .FirstAsync(p => p.Id == paymentId);

        var total = payment.Amount - (payment.DiscountAmount ?? 0m);
        return new PaymentTicket(
            payment.InvoiceNumber,
            payment.PaymentDate,
            payment.Client.FullName,
            payment.Membership.MembershipType.Name,
            payment.Amount,
            payment.DiscountAmount ?? 0m,
            total,
            payment.PaymentMethod.Name,
            payment.ReferenceText
        );
    }
}
