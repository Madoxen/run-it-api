using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Utils
{
    public static class Builders
    {
        public static IAuthorizationService BuildAuthorizationService(Action<IServiceCollection> setupServices = null)
        {
            var services = new ServiceCollection();
            services.AddAuthorization();
            services.AddLogging();
            services.AddOptions();
            setupServices?.Invoke(services);
            return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
        }

        public static DbContextOptions<ApiContext> BuildDefaultDbContext()
        {
            return new DbContextOptionsBuilder<ApiContext>()
                    .UseNpgsql(@"Server=test_db;Database=dev;Username=admin;Password=admin")
                    .Options;
        }
    }
}