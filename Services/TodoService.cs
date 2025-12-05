using ToDoInMemory.Models;

namespace ToDoInMemory.Services;

public class TodoService
{
    private readonly List<TodoItem> _todos = new();

    public IReadOnlyList<TodoItem> GetAll() => _todos.OrderByDescending(t => t.CreatedAt).ToList();

    public TodoItem? GetById(Guid id) => _todos.FirstOrDefault(t => t.Id == id);

    public TodoItem Add(string title)
    {
        var todo = new TodoItem { Title = title };
        _todos.Add(todo);
        return todo;
    }

    public bool Update(Guid id, string title)
    {
        var todo = GetById(id);
        if (todo == null) return false;
        
        todo.Title = title;
        return true;
    }

    public bool ToggleComplete(Guid id)
    {
        var todo = GetById(id);
        if (todo == null) return false;

        todo.IsCompleted = !todo.IsCompleted;
        todo.CompletedAt = todo.IsCompleted ? DateTime.Now : null;
        return true;
    }

    public bool Delete(Guid id)
    {
        var todo = GetById(id);
        if (todo == null) return false;

        _todos.Remove(todo);
        return true;
    }

    public int GetCompletedCount() => _todos.Count(t => t.IsCompleted);
    
    public int GetPendingCount() => _todos.Count(t => !t.IsCompleted);
}

