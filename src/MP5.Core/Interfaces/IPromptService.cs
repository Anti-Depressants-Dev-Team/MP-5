namespace MP5.Core.Interfaces;

public interface IPromptService
{
    Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel");
}
