namespace Domain.Entity;

public class AccessLogs
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public DateTime AccessTime { get; set; }
    public int StatusId { get; set; }
    public string? DeviceId { get; set; }
    public string? Reason { get; set; }

    public Clients Client { get; set; } = null!;
    public Status Status { get; set; } = null!;
}
