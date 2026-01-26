namespace MP5.Core.Models;

public class Lyrics
{
    public string PlainText { get; set; } = string.Empty;
    public List<LyricsLine> SyncedLines { get; set; } = new();
    public bool IsSynced => SyncedLines.Count > 0;
    public string Source { get; set; } = string.Empty;
}

public class LyricsLine
{
    public TimeSpan Time { get; set; }
    public string Text { get; set; } = string.Empty;
}
