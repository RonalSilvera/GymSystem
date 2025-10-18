using Infrastructure.Dto;

namespace BusinessLogic.Ports;

public interface IPaymentMethodService
{
    Task<IEnumerable<PaymentMethodDto>> ListAsync();
    Task<PaymentMethodDto?> DetailAsync(int id);
    Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodDto dto);
    Task<PaymentMethodDto?> UpdateAsync(int id, UpdatePaymentMethodDto dto);
    Task<bool> DeleteAsync(int id);
}
