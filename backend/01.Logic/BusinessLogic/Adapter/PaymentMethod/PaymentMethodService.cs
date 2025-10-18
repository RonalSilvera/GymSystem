using System.Net;
using BusinessLogic.Ports;
using Domain.Entity;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Dto;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Adapter.PaymentMethod;

public class PaymentMethodService(AppDbContext context, IUnitOfWork unitOfWork) : IPaymentMethodService
{
    private readonly AppDbContext _context = context;
    private readonly IUnitOfWork _uow = unitOfWork;

    public async Task<IEnumerable<PaymentMethodDto>> ListAsync()
    {
        return await _context.PaymentMethods
            .Where(pm => pm.Active)
            .OrderBy(pm => pm.Name)
            .Select(pm => new PaymentMethodDto
            {
                Id = pm.Id,
                Name = pm.Name,
                Active = pm.Active
            }).ToListAsync();
    }

    public async Task<PaymentMethodDto?> DetailAsync(int id)
    {
        var pm = await _context.PaymentMethods.FindAsync(id);
        return pm == null ? null : new PaymentMethodDto
        {
            Id = pm.Id,
            Name = pm.Name,
            Active = pm.Active
        };
    }

    public async Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodDto dto)
    {
        await EnsureNameUnique(dto.Name, null);
        var entity = new PaymentMethods
        {
            Name = dto.Name,
            Active = true
        };
        _context.PaymentMethods.Add(entity);
        await _uow.Complete();
        return new PaymentMethodDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Active = entity.Active
        };
    }

    public async Task<PaymentMethodDto?> UpdateAsync(int id, UpdatePaymentMethodDto dto)
    {
        var pm = await _context.PaymentMethods.FindAsync(id);
        if (pm == null) return null;
        await EnsureNameUnique(dto.Name, id);
        pm.Name = dto.Name;
        await _uow.Complete();
        return new PaymentMethodDto
        {
            Id = pm.Id,
            Name = pm.Name,
            Active = pm.Active
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var pm = await _context.PaymentMethods.FindAsync(id);
        if (pm == null) return false;
        pm.Active = false;
        await _uow.Complete();
        return true;
    }

    private async Task EnsureNameUnique(string name, int? excludeId)
    {
        if (await _context.PaymentMethods.AnyAsync(pm => pm.Name == name && pm.Id != excludeId))
        {
            throw new BusinessException(HttpStatusCode.Conflict, "Name already exists", "Name already exists");
        }
    }
}
