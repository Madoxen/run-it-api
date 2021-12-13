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

namespace Api.Tests
{
    public class AuthUserServiceTests : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<ApiContext> _contextOptions;

        public AuthUserServiceTests()
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

                var user = new User()
                {
                    Id = 1,
                    Weight = 1,
                    FacebookId = "aaa",
                    GoogleId = "bbb"
                };
                context.Add(user);
                context.SaveChanges();
            }
        }

        [Fact]
        public async void TestGetUserByFacebookIdExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                AuthUserService service = new AuthUserService(context);

                //Act
                var result = await service.GetUserByFacebookId("aaa");

                //Assert
                Assert.IsType<User>(result);
                Assert.Equal(1, result.Id);
            }
        }

        [Fact]
        public async void TestGetUserByFacebookIdNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                AuthUserService service = new AuthUserService(context);

                //Act
                var result = await service.GetUserByFacebookId("xd");

                //Assert
                Assert.Null(result);
            }
        }

         [Fact]
        public async void TestGetUserByGoogleIdExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                AuthUserService service = new AuthUserService(context);

                //Act
                var result = await service.GetUserByGoogleId("bbb");

                //Assert
                Assert.IsType<User>(result);
                Assert.Equal(1, result.Id);
            }
        }

        [Fact]
        public async void TestGetUserByGoogleIdNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                AuthUserService service = new AuthUserService(context);

                //Act
                var result = await service.GetUserByGoogleId("xd");

                //Assert
                Assert.Null(result);
            }
        }




        public void Dispose() => _connection.Dispose();
    }
}