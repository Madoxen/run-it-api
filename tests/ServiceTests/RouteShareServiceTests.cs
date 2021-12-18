using System;
using System.Data.Common;
using Api.Models;
using Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using Xunit;
using Api.Utils;
using System.Collections.Generic;

namespace Api.Tests
{
    public class RouteShareServiceTests : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<ApiContext> _contextOptions;

        public RouteShareServiceTests()
        {
            _contextOptions = new DbContextOptionsBuilder<ApiContext>()
                   .UseSqlite(CreateInMemoryDatabase())
                   .Options;
            _connection = RelationalOptionsExtension.Extract(_contextOptions).Connection;
            Seed();
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");

            connection.Open();

            return connection;
        }

        private void Seed()
        {
            using (var context = new ApiContext(_contextOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var route = new Route()
                {
                    Id = 1
                };

                var route2 = new Route()
                {
                    Id = 2
                };

                var user = new User()
                {
                    Routes = new List<Route>() { route }
                };

                var user2 = new User()
                {
                    Routes = new List<Route>() { route2 }
                };


                context.Routes.Add(route);
                context.Routes.Add(route2);
                context.Users.Add(user);
                context.Users.Add(user2);
                context.RouteShares.Add(new RouteShare() { RouteId = 1, SharedToId = 2 });
                context.SaveChanges();
            }
        }

        [Fact]
        public async void TestGetSharesForUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteShareService service = new RouteShareService(context);

                //Act
                var result = await service.GetSharesForUser(2);

                //Assert
                Assert.Single(result.Value);
            }
        }

        [Fact]
        public async void TestGetSharesForNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteShareService service = new RouteShareService(context);

                //Act
                var result = await service.GetSharesForUser(3);

                //Assert
                Assert.IsType<NotFoundServiceResult>(result.Result);
            }
        }

        [Fact]
        public async void TestRemoveShare()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteShareService service = new RouteShareService(context);

                //Act
                var result = await service.RemoveShare(1, 2);

                //Assert
                Assert.IsType<SuccessServiceResult>(result);
                Assert.Null(await context.RouteShares.FirstOrDefaultAsync(x => x.RouteId == 1 && x.SharedToId == 1));
            }
        }

        [Fact]
        public async void TestRemoveShareNonExistingShare()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteShareService service = new RouteShareService(context);

                //Act
                var result = await service.RemoveShare(1, 3);

                //Assert
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }
        
        [Fact]
        public async void TestShareRouteWith()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteShareService service = new RouteShareService(context);

                //Act
                var result = await service.ShareRouteWith(2, 1);

                //Assert
                Assert.IsType<SuccessServiceResult>(result);
                Assert.NotNull(await context.RouteShares.FirstOrDefaultAsync(x => x.RouteId == 2 && x.SharedToId == 1));
            }
        }

        [Fact]
        public async void TestShareRouteWithNonExistingRoute()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteShareService service = new RouteShareService(context);

                //Act
                var result = await service.ShareRouteWith(3, 1);

                //Assert
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        [Fact]
        public async void TestShareRouteWithNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteShareService service = new RouteShareService(context);

                //Act
                var result = await service.ShareRouteWith(1, 3);

                //Assert
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        public void Dispose() => _connection.Dispose();
    }
}