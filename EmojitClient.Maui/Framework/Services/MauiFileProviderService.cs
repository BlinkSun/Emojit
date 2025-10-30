using EmojitClient.Maui.Data;

namespace EmojitClient.Maui.Framework.Services;

/// <summary>
/// MAUI implementation of IFileProviderService.
/// Uses MAUI Essentials (FileSystem).
/// </summary>
public class MauiFileProviderService : IFileProviderService
{
    public string GetAppDataPath(string fileName)
    {
        return Path.Combine(FileSystem.AppDataDirectory, fileName);
    }

    public async Task<Stream> OpenPackageFileAsync(string fileName)
    {
        return await FileSystem.OpenAppPackageFileAsync(fileName);
    }
}