using ToDoInMemory.Services;

namespace ToDoInMemory.Tests;

/// <summary>
/// In-memory implementation of ILocalStorageService for testing purposes.
/// This avoids JavaScript interop calls during integration tests.
/// </summary>
public class TestLocalStorageService : ILocalStorageService
{
    private readonly Dictionary<string, string> _storage = new();

    public Task SetItemAsync(string key, string value)
    {
        _storage[key] = value;
        return Task.CompletedTask;
    }

    public Task<string?> GetItemAsync(string key)
    {
        _storage.TryGetValue(key, out var value);
        return Task.FromResult<string?>(value);
    }

    public Task RemoveItemAsync(string key)
    {
        _storage.Remove(key);
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _storage.Clear();
        return Task.CompletedTask;
    }
}

