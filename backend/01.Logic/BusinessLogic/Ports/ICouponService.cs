using Infrastructure.Dto;

namespace BusinessLogic.Ports;

public interface ICouponService
{
    Task<IEnumerable<CouponDto>> ListAsync();
    Task<CouponDto?> DetailAsync(int id);
    Task<CouponDto> CreateAsync(CreateCouponDto dto);
    Task<CouponDto?> UpdateAsync(int id, UpdateCouponDto dto);
    Task<bool> DeleteAsync(int id);
}
