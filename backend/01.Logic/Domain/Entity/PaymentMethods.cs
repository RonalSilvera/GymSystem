namespace Domain.Entity;

public class PaymentMethods
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool Active { get; set; }

    public ICollection<Payments> Payments { get; set; } = new List<Payments>();
}
