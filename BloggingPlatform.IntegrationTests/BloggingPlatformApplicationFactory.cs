using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BloggingPlatform.Data;
using Microsoft.Extensions.Logging;

namespace BloggingPlatform.IntegrationTests
{
    public abstract class BloggingPlatformApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's DbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<BloggingPlatformDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add a database context (BloggingPlatformDbContext) using an in-memory database for testing.
                services.AddDbContext<BloggingPlatformDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Build the service provider.
                var serviceProvider = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context (BloggingPlatformDbContext).
                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<BloggingPlatformDbContext>();
                
                try
                {
                    // Ensure the database is created.
                    db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    var logger = scopedServices.GetRequiredService<ILogger<BloggingPlatformApplicationFactory<TStartup>>>();
                    logger.LogError(ex, "An error occurred creating the in-memory database.");
                }
            });
        }
    }
}