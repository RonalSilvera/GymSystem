namespace Domain.CustomEntities;

public class ResponseMessage
{
    public Dictionary<string, string> Values { get; private set; }

    public ResponseMessage()
    {
        Values = new Dictionary<string, string>();
    }

    public ResponseMessage(Dictionary<string, string> dictionary)
    {
        Values = dictionary;
    }
}