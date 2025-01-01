
namespace LCSC.Core.Services;

public class CacheService
{
    private readonly string _cacheDirectory;

    public CacheService(string cacheDirectory)
    {
        _cacheDirectory = cacheDirectory;
        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }
    }

    public async Task CacheFileAsync(string fileName, string content)
    {
        var filePath = Path.Combine(_cacheDirectory, fileName);
        await File.WriteAllTextAsync(filePath, content);
    }

    public async Task<string?> GetCachedTextAsync(string fileName)
    {
        var filePath = Path.Combine(_cacheDirectory, fileName);
        if (File.Exists(filePath))
        {
            return await File.ReadAllTextAsync(filePath);
        }
        return null;
    }
}
