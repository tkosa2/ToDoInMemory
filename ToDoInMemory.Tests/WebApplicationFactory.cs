using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ToDoInMemory.Services;

namespace ToDoInMemory.Tests;

// For .NET 6+ with top-level statements, we reference the partial Program class
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        // Override services for testing
        builder.ConfigureServices(services =>
        {
            // Replace LocalStorageService with in-memory test implementation
            // to avoid JavaScript interop calls during integration tests
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ILocalStorageService));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            services.AddScoped<ILocalStorageService, TestLocalStorageService>();
        });
    }
}

