// IEntityCacheStore.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Core.Cache;

public interface IEntityCacheStore<TDto>
{
    bool IsInitialized { get; }
    IReadOnlyList<TDto> GetAll();
    void SetAll(IEnumerable<TDto> items);
    void Upsert(TDto item);
    void Remove(int id);
    void Clear();
}
