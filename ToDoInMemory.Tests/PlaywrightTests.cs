using FluentAssertions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Xunit;

namespace ToDoInMemory.Tests;

public class PlaywrightTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private string _baseUrl = string.Empty;

    public PlaywrightTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        // Get the Kestrel test server URL
        // WebApplicationFactory creates a Kestrel test server with a random port
        // We need to ensure the server is started, then get its actual URL
        
        // Create a client first to ensure the Kestrel server is fully started
        using var client = _factory.CreateClient();
        
        // Get the server instance from the factory
        var server = _factory.Server.Services.GetRequiredService<IServer>();
        
        // Get the server addresses feature which contains the actual Kestrel server URL
        var addressesFeature = server.Features.Get<IServerAddressesFeature>();
        
        // The Kestrel test server should have at least one address with the actual port
        // Try multiple methods to get the server URL, in order of reliability:
        // 1. Server addresses feature (most reliable - has actual port)
        // 2. Server.BaseAddress (set by WebApplicationFactory)
        // 3. Client.BaseAddress (fallback)
        if (addressesFeature?.Addresses?.Any() == true)
        {
            // Use the first address (Kestrel test server typically has one with actual port)
            _baseUrl = addressesFeature.Addresses.First().TrimEnd('/');
        }
        else if (_factory.Server.BaseAddress != null && !_factory.Server.BaseAddress.ToString().EndsWith("/"))
        {
            // Use the server's base address if it's not just "http://localhost/"
            var serverUrl = _factory.Server.BaseAddress.ToString().TrimEnd('/');
            if (serverUrl != "http://localhost")
            {
                _baseUrl = serverUrl;
            }
            else
            {
                // Server.BaseAddress is just localhost, try client
                _baseUrl = client.BaseAddress?.ToString().TrimEnd('/') 
                    ?? throw new InvalidOperationException("Could not determine Kestrel test server URL. Server may not be started.");
            }
        }
        else
        {
            // Final fallback: use the client's base address
            _baseUrl = client.BaseAddress?.ToString().TrimEnd('/') 
                ?? throw new InvalidOperationException("Could not determine Kestrel test server URL. Server may not be started.");
        }
        
        // Debug: Output the URL to console (will show in test output)
        Console.WriteLine($"[Playwright] Connecting to Kestrel test server at: {_baseUrl}");

        // Initialize Playwright to connect to the Kestrel test server
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // Run in headless mode for CI/CD
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_page != null)
            await _page.CloseAsync();
        if (_browser != null)
            await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    [Fact]
    public async Task HomePage_ShouldLoadSuccessfully()
    {
        // Act
        await _page!.GotoAsync(_baseUrl);

        // Assert
        var title = await _page.TitleAsync();
        title.Should().Contain("ToDo App");
    }

    [Fact]
    public async Task HomePage_ShouldDisplayExpectedContent()
    {
        // Act
        await _page!.GotoAsync(_baseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var heading = await _page.Locator("h1").TextContentAsync();
        heading.Should().Contain("My Tasks");

        var subtitle = await _page.Locator(".subtitle").TextContentAsync();
        subtitle.Should().Contain("Stay organized");
    }

    [Fact]
    public async Task AddTodo_ShouldCreateNewTodoItem()
    {
        // Arrange
        await _page!.GotoAsync(_baseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for the page to be fully loaded
        await _page.WaitForSelectorAsync(".todo-input", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

        // Act - Add a new todo
        await _page.FillAsync(".todo-input", "Test Todo from Playwright");
        await _page.SelectOptionAsync(".priority-select", "High");
        await _page.ClickAsync("button[type='submit']");

        // Wait for the todo to appear
        await _page.WaitForTimeoutAsync(1000); // Wait for Blazor to update

        // Assert
        var todoItems = await _page.Locator(".todo-item").CountAsync();
        todoItems.Should().BeGreaterThan(0);

        var todoText = await _page.Locator(".todo-title").First.TextContentAsync();
        todoText.Should().Contain("Test Todo from Playwright");
    }

    [Fact]
    public async Task ToggleTodo_ShouldMarkAsComplete()
    {
        // Arrange
        await _page!.GotoAsync(_baseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForSelectorAsync(".todo-input", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

        // Add a todo first
        await _page.FillAsync(".todo-input", "Todo to Complete");
        await _page.ClickAsync("button[type='submit']");
        await _page.WaitForTimeoutAsync(1000);

        // Act - Toggle complete
        await _page.Locator(".check-btn").First.ClickAsync();
        await _page.WaitForTimeoutAsync(500);

        // Assert
        var completedTodo = await _page.Locator(".todo-item.completed").CountAsync();
        completedTodo.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FilterTodos_ShouldFilterByStatus()
    {
        // Arrange
        await _page!.GotoAsync(_baseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForSelectorAsync(".todo-input", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

        // Add a todo
        await _page.FillAsync(".todo-input", "Pending Todo");
        await _page.ClickAsync("button[type='submit']");
        await _page.WaitForTimeoutAsync(1000);

        // Act - Click Pending filter
        await _page.ClickAsync("text=Pending");
        await _page.WaitForTimeoutAsync(500);

        // Assert
        var activeFilter = await _page.Locator(".filter-btn.active").TextContentAsync();
        activeFilter.Should().Contain("Pending");
    }

    [Fact]
    public async Task DeleteTodo_ShouldRemoveTodoItem()
    {
        // Arrange
        await _page!.GotoAsync(_baseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForSelectorAsync(".todo-input", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

        // Add a todo
        await _page.FillAsync(".todo-input", "Todo to Delete");
        await _page.ClickAsync("button[type='submit']");
        await _page.WaitForTimeoutAsync(1000);

        var initialCount = await _page.Locator(".todo-item").CountAsync();
        initialCount.Should().BeGreaterThan(0);

        // Act - Delete the todo
        await _page.Locator(".delete-btn").First.ClickAsync();
        await _page.WaitForTimeoutAsync(500);

        // Assert
        var finalCount = await _page.Locator(".todo-item").CountAsync();
        finalCount.Should().BeLessThan(initialCount);
    }

    [Fact]
    public async Task Stats_ShouldDisplayCorrectCounts()
    {
        // Arrange
        await _page!.GotoAsync(_baseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForSelectorAsync(".todo-input", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

        // Add a todo
        await _page.FillAsync(".todo-input", "Stats Test Todo");
        await _page.ClickAsync("button[type='submit']");
        await _page.WaitForTimeoutAsync(1000);

        // Assert - Check that stats are displayed
        var pendingStat = await _page.Locator(".stat-number").First.TextContentAsync();
        pendingStat.Should().NotBeNullOrEmpty();

        var statLabels = await _page.Locator(".stat-label").AllTextContentsAsync();
        statLabels.Should().Contain("Pending");
        statLabels.Should().Contain("Completed");
    }
}

