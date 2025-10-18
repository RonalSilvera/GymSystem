using System.Net;
using BusinessLogic.Ports;
using Domain.Entity;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Dto;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Adapter.MembershipType;

public class MembershipTypeService(AppDbContext context, IUnitOfWork unitOfWork) : IMembershipTypeService
{
    private readonly AppDbContext _context = context;
    private readonly IUnitOfWork _uow = unitOfWork;

    public async Task<IEnumerable<MembershipTypeDto>> ListAsync()
    {
        return await _context.MembershipTypes
            .Where(mt => mt.Active)
            .OrderBy(mt => mt.Name)
            .Select(mt => new MembershipTypeDto
            {
                Id = mt.Id,
                Name = mt.Name,
                Description = mt.Description,
                DurationDays = mt.DurationDays,
                Price = mt.Price,
                Active = mt.Active
            }).ToListAsync();
    }

    public async Task<MembershipTypeDto?> DetailAsync(int id)
    {
        var mt = await _context.MembershipTypes.FindAsync(id);
        return mt == null ? null : new MembershipTypeDto
        {
            Id = mt.Id,
            Name = mt.Name,
            Description = mt.Description,
            DurationDays = mt.DurationDays,
            Price = mt.Price,
            Active = mt.Active
        };
    }

    public async Task<MembershipTypeDto> CreateAsync(CreateMembershipTypeDto dto)
    {
        await EnsureNameUnique(dto.Name, null);
        var entity = new MembershipTypes
        {
            Name = dto.Name,
            Description = dto.Description,
            DurationDays = dto.DurationDays,
            Price = dto.Price,
            Active = true
        };
        _context.MembershipTypes.Add(entity);
        await _uow.Complete();
        return new MembershipTypeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            DurationDays = entity.DurationDays,
            Price = entity.Price,
            Active = entity.Active
        };
    }

    public async Task<MembershipTypeDto?> UpdateAsync(int id, UpdateMembershipTypeDto dto)
    {
        var mt = await _context.MembershipTypes.FindAsync(id);
        if (mt == null) return null;
        await EnsureNameUnique(dto.Name, id);
        mt.Name = dto.Name;
        mt.Description = dto.Description;
        mt.DurationDays = dto.DurationDays;
        mt.Price = dto.Price;
        await _uow.Complete();
        return new MembershipTypeDto
        {
            Id = mt.Id,
            Name = mt.Name,
            Description = mt.Description,
            DurationDays = mt.DurationDays,
            Price = mt.Price,
            Active = mt.Active
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var mt = await _context.MembershipTypes.FindAsync(id);
        if (mt == null) return false;
        var hasRefs = await _context.Memberships.AnyAsync(m => m.MembershipTypeId == id);
        if (hasRefs)
        {
            mt.Active = false;
        }
        else
        {
            _context.MembershipTypes.Remove(mt);
        }
        await _uow.Complete();
        return true;
    }

    private async Task EnsureNameUnique(string name, int? excludeId)
    {
        if (await _context.MembershipTypes.AnyAsync(mt => mt.Name == name && mt.Id != excludeId))
        {
            throw new BusinessException(HttpStatusCode.Conflict, "Name already exists", "Name already exists");
        }
    }
}
