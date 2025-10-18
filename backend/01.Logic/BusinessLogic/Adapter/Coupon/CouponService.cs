using System.Net;
using BusinessLogic.Ports;
using Domain.Entity;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Dto;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Adapter.Coupon;

public class CouponService(AppDbContext context, IUnitOfWork unitOfWork) : ICouponService
{
    private readonly AppDbContext _context = context;
    private readonly IUnitOfWork _uow = unitOfWork;

    public async Task<IEnumerable<CouponDto>> ListAsync()
    {
        return await _context.Coupons
            .Where(c => c.Active)
            .OrderBy(c => c.Code)
            .Select(c => new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                DiscountAmount = c.DiscountAmount,
                Percent = c.Percent,
                Active = c.Active,
                ValidFrom = c.ValidFrom,
                ValidTo = c.ValidTo,
                MaxUses = c.MaxUses,
                UsedCount = c.UsedCount
            }).ToListAsync();
    }

    public async Task<CouponDto?> DetailAsync(int id)
    {
        var c = await _context.Coupons.FindAsync(id);
        return c == null ? null : new CouponDto
        {
            Id = c.Id,
            Code = c.Code,
            Description = c.Description,
            DiscountAmount = c.DiscountAmount,
            Percent = c.Percent,
            Active = c.Active,
            ValidFrom = c.ValidFrom,
            ValidTo = c.ValidTo,
            MaxUses = c.MaxUses,
            UsedCount = c.UsedCount
        };
    }

    public async Task<CouponDto> CreateAsync(CreateCouponDto dto)
    {
        await EnsureCodeUnique(dto.Code, null);
        var entity = new Coupons
        {
            Code = dto.Code,
            Description = dto.Description,
            DiscountAmount = dto.DiscountAmount,
            Percent = dto.Percent,
            Active = true,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            MaxUses = dto.MaxUses,
            UsedCount = 0
        };
        _context.Coupons.Add(entity);
        await _uow.Complete();
        return new CouponDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Description = entity.Description,
            DiscountAmount = entity.DiscountAmount,
            Percent = entity.Percent,
            Active = entity.Active,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            MaxUses = entity.MaxUses,
            UsedCount = entity.UsedCount
        };
    }

    public async Task<CouponDto?> UpdateAsync(int id, UpdateCouponDto dto)
    {
        var entity = await _context.Coupons.FindAsync(id);
        if (entity == null) return null;
        await EnsureCodeUnique(dto.Code, id);
        entity.Code = dto.Code;
        entity.Description = dto.Description;
        entity.DiscountAmount = dto.DiscountAmount;
        entity.Percent = dto.Percent;
        entity.ValidFrom = dto.ValidFrom;
        entity.ValidTo = dto.ValidTo;
        entity.MaxUses = dto.MaxUses;
        await _uow.Complete();
        return new CouponDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Description = entity.Description,
            DiscountAmount = entity.DiscountAmount,
            Percent = entity.Percent,
            Active = entity.Active,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            MaxUses = entity.MaxUses,
            UsedCount = entity.UsedCount
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Coupons.FindAsync(id);
        if (entity == null) return false;
        entity.Active = false;
        await _uow.Complete();
        return true;
    }

    private async Task EnsureCodeUnique(string code, int? excludeId)
    {
        if (await _context.Coupons.AnyAsync(c => c.Code == code && c.Id != excludeId))
        {
            throw new BusinessException(HttpStatusCode.Conflict, "Code already exists", "Code already exists");
        }
    }
}
