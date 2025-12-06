using FluentAssertions;
using Moq;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using ToDoInMemory.Services;

namespace ToDoInMemory.Tests;

public class LocalStorageServiceTests
{
    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    private readonly LocalStorageService _localStorageService;

    public LocalStorageServiceTests()
    {
        _jsRuntimeMock = new Mock<IJSRuntime>();
        _localStorageService = new LocalStorageService(_jsRuntimeMock.Object);
    }

    [Fact]
    public async Task SetItemAsync_ShouldNotThrow()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";
        _jsRuntimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(new ValueTask<IJSVoidResult>(Mock.Of<IJSVoidResult>()));

        // Act & Assert
        await _localStorageService.Invoking(x => x.SetItemAsync(key, value))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetItemAsync_WhenItemExists_ShouldReturnValue()
    {
        // Arrange
        var key = "testKey";
        var expectedValue = "testValue";
        _jsRuntimeMock.Setup(x => x.InvokeAsync<string?>("localStorageHelper.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(expectedValue);

        // Act
        var result = await _localStorageService.GetItemAsync(key);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public async Task GetItemAsync_WhenItemDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var key = "nonExistentKey";
        _jsRuntimeMock.Setup(x => x.InvokeAsync<string?>("localStorageHelper.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _localStorageService.GetItemAsync(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldNotThrow()
    {
        // Arrange
        var key = "testKey";
        _jsRuntimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(new ValueTask<IJSVoidResult>(Mock.Of<IJSVoidResult>()));

        // Act & Assert
        await _localStorageService.Invoking(x => x.RemoveItemAsync(key))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task ClearAsync_ShouldNotThrow()
    {
        // Arrange
        _jsRuntimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(new ValueTask<IJSVoidResult>(Mock.Of<IJSVoidResult>()));

        // Act & Assert
        await _localStorageService.Invoking(x => x.ClearAsync())
            .Should().NotThrowAsync();
    }
}
