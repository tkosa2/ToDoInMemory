using System.Text.Json;
using ToDoInMemory.Models;

namespace ToDoInMemory.Services;

public class TodoService
{
    private readonly List<TodoItem> _todos = new();
    private readonly LocalStorageService _localStorage;
    private const string StorageKey = "todos";
    private bool _isInitialized = false;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public TodoService(LocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized) return;

            var json = await _localStorage.GetItemAsync(StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var todos = JsonSerializer.Deserialize<List<TodoItem>>(json);
                    if (todos != null)
                    {
                        _todos.Clear();
                        _todos.AddRange(todos);
                    }
                }
                catch (JsonException)
                {
                    // If localStorage data is corrupted, start with empty list
                    _todos.Clear();
                }
            }
            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task SaveToStorageAsync()
    {
        var json = JsonSerializer.Serialize(_todos);
        await _localStorage.SetItemAsync(StorageKey, json);
    }

    public IReadOnlyList<TodoItem> GetAll() => _todos.OrderByDescending(t => t.CreatedAt).ToList();

    public TodoItem? GetById(Guid id) => _todos.FirstOrDefault(t => t.Id == id);

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem { Title = title };
        _todos.Add(todo);
        await SaveToStorageAsync();
        return todo;
    }

    public async Task<bool> UpdateAsync(Guid id, string title)
    {
        var todo = GetById(id);
        if (todo == null) return false;
        
        todo.Title = title;
        await SaveToStorageAsync();
        return true;
    }

    public async Task<bool> ToggleCompleteAsync(Guid id)
    {
        var todo = GetById(id);
        if (todo == null) return false;

        todo.IsCompleted = !todo.IsCompleted;
        todo.CompletedAt = todo.IsCompleted ? DateTime.Now : null;
        await SaveToStorageAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var todo = GetById(id);
        if (todo == null) return false;

        _todos.Remove(todo);
        await SaveToStorageAsync();
        return true;
    }

    public int GetCompletedCount() => _todos.Count(t => t.IsCompleted);
    
    public int GetPendingCount() => _todos.Count(t => !t.IsCompleted);
}

