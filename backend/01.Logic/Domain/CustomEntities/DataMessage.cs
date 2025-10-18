namespace Domain.CustomEntities;

public class DataMessage
{
    public string SourceIp { get; set; } = string.Empty;
    public string SourceEntity { get; set; } = string.Empty;
    public string InformationSystem { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
    public string MessageDigest { get; set; } = string.Empty;
        
}