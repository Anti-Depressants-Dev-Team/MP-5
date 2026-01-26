using System.Text;
using MP5.Core.Interfaces;

namespace MP5.App.Services;

public class FilePickerService : IFilePickerService
{
    public async Task<string?> PickJsonFileAsync()
    {
        try
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.json" } },
                    { DevicePlatform.Android, new[] { "application/json" } },
                    { DevicePlatform.WinUI, new[] { ".json" } },
                    { DevicePlatform.macOS, new[] { "json" } },
                });

            var options = new PickOptions
            {
                PickerTitle = "Select a JSON file",
                FileTypes = customFileType,
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Pick File Error: {ex.Message}");
        }
        return null;
    }

    public async Task SaveJsonFileAsync(string fileName, string content)
    {
        try
        {
            var folder = FileSystem.CacheDirectory;
            var path = Path.Combine(folder, fileName);
            
            await File.WriteAllTextAsync(path, content, Encoding.UTF8);
            
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Export {fileName}",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Save File Error: {ex.Message}");
        }
    }
}
