using System;
using System.IO;
using System.Text.RegularExpressions;
using BusinessLogic.Ports;

namespace BusinessLogic.Adapter.FileStorage;

public class LocalFileStorage : IFileStorage
{
    public async Task<string> SaveBase64ImageAsync(string base64Image, string folder)
    {
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var base64Data = base64Image;
        var extension = "png";
        var match = Regex.Match(base64Data, @"data:image/(?<type>.+?);base64,(?<data>.+)");
        if (match.Success)
        {
            extension = match.Groups["type"].Value;
            base64Data = match.Groups["data"].Value;
        }

        var fileName = $"{Guid.NewGuid()}.{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);
        var bytes = Convert.FromBase64String(base64Data);
        await File.WriteAllBytesAsync(filePath, bytes);
        return fileName;
    }

    public async Task<string?> GetBase64ImageAsync(string fileName, string folder)
    {
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
        var filePath = Path.Combine(uploadsFolder, fileName);
        if (!File.Exists(filePath))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(filePath);
        var extension = Path.GetExtension(fileName).TrimStart('.');
        var base64 = Convert.ToBase64String(bytes);
        return $"data:image/{extension};base64,{base64}";
    }
}
