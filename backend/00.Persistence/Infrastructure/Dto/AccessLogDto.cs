namespace Infrastructure.Dto;

public class AccessLogDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime AccessTime { get; set; }
    public bool Allowed { get; set; }
    public string? DeviceId { get; set; }
    public string? Reason { get; set; }
}
