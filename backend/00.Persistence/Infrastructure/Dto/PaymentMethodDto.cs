using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Dto;

public class PaymentMethodDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
}
