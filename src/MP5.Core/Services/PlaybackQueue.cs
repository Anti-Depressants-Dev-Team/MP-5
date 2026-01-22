using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.Services;

/// <summary>
/// Manages the playback queue and track ordering.
/// </summary>
public class PlaybackQueue
{
    private readonly List<Track> _queue = [];
    private int _currentIndex = -1;
    private readonly Random _random = new();
    
    public IReadOnlyList<Track> Tracks => _queue.AsReadOnly();
    public int CurrentIndex => _currentIndex;
    public Track? CurrentTrack => _currentIndex >= 0 && _currentIndex < _queue.Count 
        ? _queue[_currentIndex] 
        : null;
    
    public bool ShuffleEnabled { get; set; }
    public RepeatMode RepeatMode { get; set; } = RepeatMode.None;
    
    private List<int>? _shuffledIndices;
    private int _shufflePosition = -1;
    
    /// <summary>
    /// Set the queue with a list of tracks, optionally starting from a specific track.
    /// </summary>
    public void SetQueue(IEnumerable<Track> tracks, Track? startTrack = null)
    {
        _queue.Clear();
        _queue.AddRange(tracks);
        
        if (startTrack != null)
        {
            _currentIndex = _queue.FindIndex(t => t.Id == startTrack.Id);
            if (_currentIndex < 0) _currentIndex = 0;
        }
        else
        {
            _currentIndex = _queue.Count > 0 ? 0 : -1;
        }
        
        if (ShuffleEnabled)
        {
            GenerateShuffledIndices();
        }
    }
    
    /// <summary>
    /// Add a track to the end of the queue.
    /// </summary>
    public void AddToQueue(Track track)
    {
        _queue.Add(track);
        if (_currentIndex < 0) _currentIndex = 0;
        
        if (ShuffleEnabled && _shuffledIndices != null)
        {
            // Insert at random position in remaining shuffle
            var insertPos = _random.Next(_shufflePosition + 1, _shuffledIndices.Count + 1);
            _shuffledIndices.Insert(insertPos, _queue.Count - 1);
        }
    }
    
    /// <summary>
    /// Get the next track in the queue.
    /// </summary>
    public Track? GetNext()
    {
        if (_queue.Count == 0) return null;
        
        if (RepeatMode == RepeatMode.One)
        {
            return CurrentTrack;
        }
        
        if (ShuffleEnabled && _shuffledIndices != null)
        {
            _shufflePosition++;
            if (_shufflePosition >= _shuffledIndices.Count)
            {
                if (RepeatMode == RepeatMode.All)
                {
                    GenerateShuffledIndices();
                    _shufflePosition = 0;
                }
                else
                {
                    return null;
                }
            }
            _currentIndex = _shuffledIndices[_shufflePosition];
        }
        else
        {
            _currentIndex++;
            if (_currentIndex >= _queue.Count)
            {
                if (RepeatMode == RepeatMode.All)
                {
                    _currentIndex = 0;
                }
                else
                {
                    _currentIndex = _queue.Count - 1;
                    return null;
                }
            }
        }
        
        return CurrentTrack;
    }
    
    /// <summary>
    /// Get the previous track in the queue.
    /// </summary>
    public Track? GetPrevious()
    {
        if (_queue.Count == 0) return null;
        
        if (ShuffleEnabled && _shuffledIndices != null)
        {
            _shufflePosition--;
            if (_shufflePosition < 0)
            {
                if (RepeatMode == RepeatMode.All)
                {
                    _shufflePosition = _shuffledIndices.Count - 1;
                }
                else
                {
                    _shufflePosition = 0;
                    return CurrentTrack;
                }
            }
            _currentIndex = _shuffledIndices[_shufflePosition];
        }
        else
        {
            _currentIndex--;
            if (_currentIndex < 0)
            {
                if (RepeatMode == RepeatMode.All)
                {
                    _currentIndex = _queue.Count - 1;
                }
                else
                {
                    _currentIndex = 0;
                    return CurrentTrack;
                }
            }
        }
        
        return CurrentTrack;
    }
    
    /// <summary>
    /// Clear the queue.
    /// </summary>
    public void Clear()
    {
        _queue.Clear();
        _currentIndex = -1;
        _shuffledIndices = null;
        _shufflePosition = -1;
    }
    
    /// <summary>
    /// Toggle shuffle mode.
    /// </summary>
    public void ToggleShuffle()
    {
        ShuffleEnabled = !ShuffleEnabled;
        if (ShuffleEnabled)
        {
            GenerateShuffledIndices();
        }
        else
        {
            _shuffledIndices = null;
            _shufflePosition = -1;
        }
    }
    
    private void GenerateShuffledIndices()
    {
        _shuffledIndices = Enumerable.Range(0, _queue.Count).ToList();
        
        // Fisher-Yates shuffle, keeping current track at position 0
        if (_currentIndex >= 0 && _currentIndex < _queue.Count)
        {
            _shuffledIndices.Remove(_currentIndex);
            for (int i = _shuffledIndices.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (_shuffledIndices[i], _shuffledIndices[j]) = (_shuffledIndices[j], _shuffledIndices[i]);
            }
            _shuffledIndices.Insert(0, _currentIndex);
        }
        else
        {
            for (int i = _shuffledIndices.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (_shuffledIndices[i], _shuffledIndices[j]) = (_shuffledIndices[j], _shuffledIndices[i]);
            }
        }
        
        _shufflePosition = 0;
    }
}
