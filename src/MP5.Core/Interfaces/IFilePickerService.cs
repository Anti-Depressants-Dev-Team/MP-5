namespace MP5.Core.Interfaces;

public interface IFilePickerService
{
    Task<string?> PickJsonFileAsync();
    Task SaveJsonFileAsync(string fileName, string content);
}
