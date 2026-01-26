using MP5.Core.Interfaces;

namespace MP5.App.Services;

public class MauiPromptService : IPromptService
{
    public async Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel")
    {
        if (Application.Current?.MainPage == null) return string.Empty;
        
        return await Application.Current.MainPage.DisplayPromptAsync(title, message, accept, cancel);
    }
}
