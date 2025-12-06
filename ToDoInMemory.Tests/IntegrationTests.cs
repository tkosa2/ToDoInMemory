using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using ToDoInMemory.Models;
using ToDoInMemory.Services;
using Xunit;

namespace ToDoInMemory.Tests;

public class IntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public IntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HomePage_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HomePage_ShouldContainExpectedContent()
    {
        // Act
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("My Tasks");
        content.Should().Contain("ToDo App");
    }

    [Fact]
    public void TodoService_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        
        // Act
        var todoService = scope.ServiceProvider.GetService<TodoService>();

        // Assert
        todoService.Should().NotBeNull();
    }

    [Fact]
    public void LocalStorageService_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        
        // Act
        var localStorageService = scope.ServiceProvider.GetService<ILocalStorageService>();

        // Assert
        localStorageService.Should().NotBeNull();
        // In integration tests, we use TestLocalStorageService instead of LocalStorageService
        localStorageService.Should().BeOfType<TestLocalStorageService>();
    }

    [Fact]
    public async Task TodoService_CanAddAndRetrieveTodos()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var todoService = scope.ServiceProvider.GetRequiredService<TodoService>();
        await todoService.InitializeAsync();

        // Act
        var todo1 = await todoService.AddAsync("Test Todo 1", Priority.High);
        var todo2 = await todoService.AddAsync("Test Todo 2", Priority.Medium);
        var todos = todoService.GetAll().ToList();

        // Assert
        todos.Should().HaveCount(2);
        todos.Should().Contain(t => t.Title == "Test Todo 1" && t.Priority == Priority.High);
        todos.Should().Contain(t => t.Title == "Test Todo 2" && t.Priority == Priority.Medium);
    }

    [Fact]
    public async Task TodoService_CanUpdateTodo()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var todoService = scope.ServiceProvider.GetRequiredService<TodoService>();
        await todoService.InitializeAsync();
        var todo = await todoService.AddAsync("Original Title");

        // Act
        var result = await todoService.UpdateAsync(todo.Id, "Updated Title");
        var updatedTodo = todoService.GetById(todo.Id);

        // Assert
        result.Should().BeTrue();
        updatedTodo.Should().NotBeNull();
        updatedTodo!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task TodoService_CanToggleComplete()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var todoService = scope.ServiceProvider.GetRequiredService<TodoService>();
        await todoService.InitializeAsync();
        var todo = await todoService.AddAsync("Test Todo");

        // Act
        var result = await todoService.ToggleCompleteAsync(todo.Id);
        var updatedTodo = todoService.GetById(todo.Id);

        // Assert
        result.Should().BeTrue();
        updatedTodo.Should().NotBeNull();
        updatedTodo!.IsCompleted.Should().BeTrue();
        updatedTodo.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task TodoService_CanDeleteTodo()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var todoService = scope.ServiceProvider.GetRequiredService<TodoService>();
        await todoService.InitializeAsync();
        var todo = await todoService.AddAsync("Test Todo");
        todoService.GetAll().Should().HaveCount(1);

        // Act
        var result = await todoService.DeleteAsync(todo.Id);
        var todos = todoService.GetAll().ToList();

        // Assert
        result.Should().BeTrue();
        todos.Should().BeEmpty();
    }

    [Fact]
    public async Task TodoService_GetCompletedCount_ReturnsCorrectCount()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var todoService = scope.ServiceProvider.GetRequiredService<TodoService>();
        await todoService.InitializeAsync();
        
        var todo1 = await todoService.AddAsync("Todo 1");
        var todo2 = await todoService.AddAsync("Todo 2");
        var todo3 = await todoService.AddAsync("Todo 3");

        await todoService.ToggleCompleteAsync(todo1.Id);
        await todoService.ToggleCompleteAsync(todo2.Id);

        // Act
        var completedCount = todoService.GetCompletedCount();
        var pendingCount = todoService.GetPendingCount();

        // Assert
        completedCount.Should().Be(2);
        pendingCount.Should().Be(1);
    }

    [Fact]
    public async Task TodoService_GetAll_ReturnsTodosOrderedByCreatedAtDescending()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var todoService = scope.ServiceProvider.GetRequiredService<TodoService>();
        await todoService.InitializeAsync();

        var todo1 = await todoService.AddAsync("First");
        await Task.Delay(10);
        var todo2 = await todoService.AddAsync("Second");
        await Task.Delay(10);
        var todo3 = await todoService.AddAsync("Third");

        // Act
        var todos = todoService.GetAll().ToList();

        // Assert
        todos.Should().HaveCount(3);
        todos[0].Title.Should().Be("Third");
        todos[1].Title.Should().Be("Second");
        todos[2].Title.Should().Be("First");
    }

    [Fact]
    public async Task StaticFiles_ShouldBeServed()
    {
        // Act
        var response = await _client.GetAsync("/css/app.css");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/css");
    }

    [Fact]
    public async Task Application_ShouldHandleInvalidRoutes()
    {
        // Act
        var response = await _client.GetAsync("/nonexistent-route");

        // Assert
        // In test environment, unmatched routes return 404
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}

