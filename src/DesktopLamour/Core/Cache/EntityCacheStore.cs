// EntityCacheStore.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Core.Cache;

/// <summary>
/// Thread-safe in-memory cache keyed by an int id. Lives for the process lifetime of a
/// singleton registration; callers decide when to warm it up (SetAll) and reset it (Clear).
/// </summary>
public class EntityCacheStore<TDto> : IEntityCacheStore<TDto> where TDto : class
{
    private readonly Func<TDto, int> _idSelector;
    private readonly object _lock = new();
    private Dictionary<int, TDto> _items = new();
    private bool _isInitialized;

    public EntityCacheStore(Func<TDto, int> idSelector)
    {
        _idSelector = idSelector;
    }

    public bool IsInitialized
    {
        get { lock (_lock) return _isInitialized; }
    }

    public IReadOnlyList<TDto> GetAll()
    {
        lock (_lock) return _items.Values.ToList();
    }

    public void SetAll(IEnumerable<TDto> items)
    {
        lock (_lock)
        {
            _items = items.ToDictionary(_idSelector);
            _isInitialized = true;
        }
    }

    public void Upsert(TDto item)
    {
        lock (_lock) _items[_idSelector(item)] = item;
    }

    public void Remove(int id)
    {
        lock (_lock) _items.Remove(id);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _items.Clear();
            _isInitialized = false;
        }
    }
}
