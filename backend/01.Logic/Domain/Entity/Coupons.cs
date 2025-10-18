namespace Domain.Entity;

public class Coupons
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? Percent { get; set; }
    public bool Active { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }

    public ICollection<Payments> Payments { get; set; } = new List<Payments>();
}
