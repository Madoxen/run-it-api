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
    public class RouteServiceTests : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<ApiContext> _contextOptions;

        public RouteServiceTests()
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

                var user = new User()
                {
                    Routes = new List<Route>() { route }
                };

                context.Routes.Add(route);
                context.Users.Add(user);
                context.SaveChanges();
            }
        }

        [Fact]
        public async void TestGetRouteById()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.GetRouteById(1);

                //Assert
                Assert.Equal(1, result.Id);
                Assert.NotNull(result);
            }
        }

        [Fact]
        public async void TestGetRouteByIdNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.GetRouteById(2);

                //Assert
                Assert.Null(result);
            }
        }


        [Fact]
        public async void TestGetUserRoutes()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.GetUserRoutes(1);

                //Assert
                Assert.NotNull(result.Value);
                Assert.Single(result.Value);
            }
        }

        [Fact]
        public async void TestGetUserRoutesNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.GetUserRoutes(2);

                //Assert
                Assert.Null(result.Value);
                Assert.NotNull(result.Result);
                Assert.IsType<NotFoundServiceResult>(result.Result);
            }
        }

        [Fact]
        public async void TestCreateRoute()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.CreateRoute(new Route() { UserId = 1, Id = 2 });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<SuccessServiceResult>(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var route = await context.Routes.FirstOrDefaultAsync(x => x.Id == 2);
                Assert.NotNull(route);
            }
        }

        [Fact]
        public async void TestCreateRouteForNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.CreateRoute(new Route() { UserId = 2, Id = 2 });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        [Fact]
        public async void TestUpdateRoute()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.UpdateRoute(new Route() { Id = 1, UserId = 1, Title = "xd" });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<SuccessServiceResult>(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var route = await context.Routes.FirstOrDefaultAsync(x => x.Id == 1);
                Assert.NotNull(route);
                Assert.Equal("xd", route.Title);
            }
        }

        [Fact]
        public async void TestCreateUpdateForNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.UpdateRoute(new Route() { UserId = 2, Id = 1 });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        [Fact]
        public async void TestRemoveRoute()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.RemoveRoute(new Route() { Id = 1 });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<SuccessServiceResult>(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var route = await context.Routes.FirstOrDefaultAsync(x => x.Id == 1);
                Assert.Null(route);
            }
        }

        [Fact]
        public async void TestRemoveRouteNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.RemoveRoute(new Route() { Id = 2 });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        [Fact]
        public async void TestRemoveRouteById()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.RemoveRouteById(1);

                //Assert
                Assert.NotNull(result);
                Assert.IsType<SuccessServiceResult>(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var route = await context.Routes.FirstOrDefaultAsync(x => x.Id == 1);
                Assert.Null(route);
            }
        }

        [Fact]
        public async void TestRemoveRouteByIdNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RouteService service = new RouteService(context);

                //Act
                var result = await service.RemoveRouteById(2);

                //Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }







        public void Dispose() => _connection.Dispose();
    }
}