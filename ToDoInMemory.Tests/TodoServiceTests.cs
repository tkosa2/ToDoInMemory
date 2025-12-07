using FluentAssertions;
using Moq;
using ToDoInMemory.Models;
using ToDoInMemory.Services;
using System.Text.Json;

namespace ToDoInMemory.Tests;

public class TodoServiceTests
{
    private readonly Mock<ILocalStorageService> _localStorageMock;
    private readonly TodoService _todoService;

    public TodoServiceTests()
    {
        _localStorageMock = new Mock<ILocalStorageService>();
        _todoService = new TodoService(_localStorageMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_WhenNoStoredData_ShouldInitializeWithEmptyList()
    {
        // Arrange
        _localStorageMock.Setup(x => x.GetItemAsync("todos"))
            .ReturnsAsync((string?)null);

        // Act
        await _todoService.InitializeAsync();

        // Assert
        var todos = _todoService.GetAll();
        todos.Should().BeEmpty();
    }

    [Fact]
    public async Task InitializeAsync_WhenStoredDataExists_ShouldLoadTodos()
    {
        // Arrange
        var storedTodos = new List<TodoItem>
        {
            new TodoItem { Id = Guid.NewGuid(), Title = "Test Todo 1", Priority = Priority.High },
            new TodoItem { Id = Guid.NewGuid(), Title = "Test Todo 2", Priority = Priority.Medium }
        };
        var json = JsonSerializer.Serialize(storedTodos);
        _localStorageMock.Setup(x => x.GetItemAsync("todos"))
            .ReturnsAsync(json);

        // Act
        await _todoService.InitializeAsync();

        // Assert
        var todos = _todoService.GetAll();
        todos.Should().HaveCount(2);
        todos.Should().Contain(t => t.Title == "Test Todo 1");
        todos.Should().Contain(t => t.Title == "Test Todo 2");
    }

    [Fact]
    public async Task InitializeAsync_WhenStoredDataIsCorrupted_ShouldInitializeWithEmptyList()
    {
        // Arrange
        _localStorageMock.Setup(x => x.GetItemAsync("todos"))
            .ReturnsAsync("invalid json data");

        // Act
        await _todoService.InitializeAsync();

        // Assert
        var todos = _todoService.GetAll();
        todos.Should().BeEmpty();
    }

    [Fact]
    public async Task InitializeAsync_WhenCalledMultipleTimes_ShouldOnlyInitializeOnce()
    {
        // Arrange
        _localStorageMock.Setup(x => x.GetItemAsync("todos"))
            .ReturnsAsync((string?)null);

        // Act
        await _todoService.InitializeAsync();
        await _todoService.InitializeAsync();
        await _todoService.InitializeAsync();

        // Assert
        _localStorageMock.Verify(x => x.GetItemAsync("todos"), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldAddTodoAndSaveToStorage()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var todo = await _todoService.AddAsync("New Todo", Priority.High);

        // Assert
        todo.Should().NotBeNull();
        todo.Title.Should().Be("New Todo");
        todo.Priority.Should().Be(Priority.High);
        todo.IsCompleted.Should().BeFalse();
        todo.Id.Should().NotBeEmpty();
        todo.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));

        var todos = _todoService.GetAll();
        todos.Should().Contain(t => t.Id == todo.Id);
        _localStorageMock.Verify(x => x.SetItemAsync("todos", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WithDefaultPriority_ShouldUseMediumPriority()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var todo = await _todoService.AddAsync("New Todo");

        // Assert
        todo.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public async Task GetAll_ShouldReturnTodosOrderedByCreatedAtDescending()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var todo1 = await _todoService.AddAsync("First Todo");
        await Task.Delay(10); // Small delay to ensure different timestamps
        var todo2 = await _todoService.AddAsync("Second Todo");
        await Task.Delay(10);
        var todo3 = await _todoService.AddAsync("Third Todo");

        // Act
        var todos = _todoService.GetAll().ToList();

        // Assert
        todos.Should().HaveCount(3);
        todos[0].Title.Should().Be("Third Todo");
        todos[1].Title.Should().Be("Second Todo");
        todos[2].Title.Should().Be("First Todo");
    }

    [Fact]
    public async Task GetById_WhenTodoExists_ShouldReturnTodo()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var todo = await _todoService.AddAsync("Test Todo");

        // Act
        var result = _todoService.GetById(todo.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(todo.Id);
        result.Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task GetById_WhenTodoDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        await _todoService.InitializeAsync();

        // Act
        var result = _todoService.GetById(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenTodoExists_ShouldUpdateTitleAndSave()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var todo = await _todoService.AddAsync("Original Title");

        // Act
        var result = await _todoService.UpdateAsync(todo.Id, "Updated Title");

        // Assert
        result.Should().BeTrue();
        var updatedTodo = _todoService.GetById(todo.Id);
        updatedTodo.Should().NotBeNull();
        updatedTodo!.Title.Should().Be("Updated Title");
        _localStorageMock.Verify(x => x.SetItemAsync("todos", It.IsAny<string>()), Times.Exactly(2)); // Once for Add, once for Update
    }

    [Fact]
    public async Task UpdateAsync_WhenTodoDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        await _todoService.InitializeAsync();

        // Act
        var result = await _todoService.UpdateAsync(Guid.NewGuid(), "New Title");

        // Assert
        result.Should().BeFalse();
        _localStorageMock.Verify(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePriorityAsync_WhenTodoExists_ShouldUpdatePriorityAndSave()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var todo = await _todoService.AddAsync("Test Todo", Priority.Medium);

        // Act
        var result = await _todoService.UpdatePriorityAsync(todo.Id, Priority.High);

        // Assert
        result.Should().BeTrue();
        var updatedTodo = _todoService.GetById(todo.Id);
        updatedTodo.Should().NotBeNull();
        updatedTodo!.Priority.Should().Be(Priority.High);
        _localStorageMock.Verify(x => x.SetItemAsync("todos", It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdatePriorityAsync_WhenTodoDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        await _todoService.InitializeAsync();

        // Act
        var result = await _todoService.UpdatePriorityAsync(Guid.NewGuid(), Priority.High);

        // Assert
        result.Should().BeFalse();
        _localStorageMock.Verify(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenTodoIsIncomplete_ShouldMarkAsComplete()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var todo = await _todoService.AddAsync("Test Todo");
        todo.IsCompleted.Should().BeFalse();
        todo.CompletedAt.Should().BeNull();

        // Act
        var result = await _todoService.ToggleCompleteAsync(todo.Id);

        // Assert
        result.Should().BeTrue();
        var updatedTodo = _todoService.GetById(todo.Id);
        updatedTodo.Should().NotBeNull();
        updatedTodo!.IsCompleted.Should().BeTrue();
        updatedTodo.CompletedAt.Should().NotBeNull();
        updatedTodo.CompletedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _localStorageMock.Verify(x => x.SetItemAsync("todos", It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenTodoIsComplete_ShouldMarkAsIncomplete()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var todo = await _todoService.AddAsync("Test Todo");
        await _todoService.ToggleCompleteAsync(todo.Id);

        // Act
        var result = await _todoService.ToggleCompleteAsync(todo.Id);

        // Assert
        result.Should().BeTrue();
        var updatedTodo = _todoService.GetById(todo.Id);
        updatedTodo.Should().NotBeNull();
        updatedTodo!.IsCompleted.Should().BeFalse();
        updatedTodo.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenTodoDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        await _todoService.InitializeAsync();

        // Act
        var result = await _todoService.ToggleCompleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _localStorageMock.Verify(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenTodoExists_ShouldRemoveTodoAndSave()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var todo = await _todoService.AddAsync("Test Todo");
        _todoService.GetAll().Should().HaveCount(1);

        // Act
        var result = await _todoService.DeleteAsync(todo.Id);

        // Assert
        result.Should().BeTrue();
        _todoService.GetAll().Should().BeEmpty();
        _todoService.GetById(todo.Id).Should().BeNull();
        _localStorageMock.Verify(x => x.SetItemAsync("todos", It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteAsync_WhenTodoDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        await _todoService.InitializeAsync();

        // Act
        var result = await _todoService.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _localStorageMock.Verify(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetCompletedCount_ShouldReturnCorrectCount()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var todo1 = await _todoService.AddAsync("Todo 1");
        var todo2 = await _todoService.AddAsync("Todo 2");
        var todo3 = await _todoService.AddAsync("Todo 3");

        await _todoService.ToggleCompleteAsync(todo1.Id);
        await _todoService.ToggleCompleteAsync(todo2.Id);

        // Act
        var count = _todoService.GetCompletedCount();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetCompletedCount_WhenNoCompletedTodos_ShouldReturnZero()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _todoService.AddAsync("Todo 1");
        await _todoService.AddAsync("Todo 2");

        // Act
        var count = _todoService.GetCompletedCount();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetPendingCount_ShouldReturnCorrectCount()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var todo1 = await _todoService.AddAsync("Todo 1");
        var todo2 = await _todoService.AddAsync("Todo 2");
        var todo3 = await _todoService.AddAsync("Todo 3");

        await _todoService.ToggleCompleteAsync(todo1.Id);

        // Act
        var count = _todoService.GetPendingCount();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetPendingCount_WhenNoPendingTodos_ShouldReturnZero()
    {
        // Arrange
        await _todoService.InitializeAsync();
        _localStorageMock.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var todo1 = await _todoService.AddAsync("Todo 1");
        var todo2 = await _todoService.AddAsync("Todo 2");

        await _todoService.ToggleCompleteAsync(todo1.Id);
        await _todoService.ToggleCompleteAsync(todo2.Id);

        // Act
        var count = _todoService.GetPendingCount();

        // Assert
        count.Should().Be(0);
    }
}

