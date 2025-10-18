namespace Infrastructure.Dto;

public class CreateCouponDto
{
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? Percent { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? MaxUses { get; set; }
}
