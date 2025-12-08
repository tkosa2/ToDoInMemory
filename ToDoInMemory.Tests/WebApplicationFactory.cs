using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Create a real Kestrel host that listens on an actual TCP port
        // This is required for Playwright tests to connect to the server
        var host = builder.Build();
        
        // Start the host to get the actual listening address
        host.Start();
        
        // Get the server and its listening address
        var server = host.Services.GetRequiredService<IServer>();
        var addressesFeature = server.Features.Get<IServerAddressesFeature>();
        
        // Set the base address for the test client
        if (addressesFeature?.Addresses?.Any() == true)
        {
            var address = addressesFeature.Addresses.First();
            ClientOptions.BaseAddress = new Uri(address);
        }
        
        return host;
    }
}

