namespace BusinessLogic.Ports;

public interface IFileStorage
{
    Task<string> SaveBase64ImageAsync(string base64Image, string folder);
    Task<string?> GetBase64ImageAsync(string fileName, string folder);
}
