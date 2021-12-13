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
    public class UserServiceTests : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<ApiContext> _contextOptions;

        public UserServiceTests()
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
                    Weight = 1
                };
                context.Add(user);
                context.SaveChanges();
            }
        }

        [Fact]
        public async void TestGetUserByIdExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                UserService service = new UserService(context);

                //Act
                var result = await service.GetUserById(1);

                //Assert
                Assert.IsType<User>(result);
                Assert.Equal(1, result.Id);
            }
        }

        [Fact]
        public async void TestGetUserByIdNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                UserService service = new UserService(context);

                //Act
                var result = await service.GetUserById(2);

                //Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async void TestCreateUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                UserService service = new UserService(context);

                //Act
                await service.CreateUser(new User() { Id = 2 });
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var result = await context.Users.FirstOrDefaultAsync(x => x.Id == 2);
                Assert.IsType<User>(result);
                Assert.NotNull(result);
            }
        }

        [Fact]
        public async void TestUpdateUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                UserService service = new UserService(context);

                //Act
                var result = await service.UpdateUser(new User() { Id = 1, Weight = 2 });
                Assert.IsType<SuccessServiceResult>(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var result = await context.Users.FindAsync(1);
                Assert.IsType<User>(result);
                Assert.NotNull(result);
                Assert.Equal(2, result.Weight);
            }
        }


        [Fact]
        public async void TestUpdateNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                UserService service = new UserService(context);

                //Act
                var result = await service.UpdateUser(new User() { Id = 2, Weight = 2 });

                //Assert
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        [Fact]
        public async void TestRemoveById()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                UserService service = new UserService(context);

                //Act
                var result = await service.RemoveUserById(1);

                //Assert
                Assert.IsType<SuccessServiceResult>(result);
            }
        }

        [Fact]
        public async void TestRemoveByIdNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                UserService service = new UserService(context);

                //Act
                var result = await service.RemoveUserById(2);

                //Assert
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        [Fact]
        public async void TestRemoveUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                UserService service = new UserService(context);

                //Act
                var result = await service.RemoveUser(new User() { Id = 1 });

                //Assert
                Assert.IsType<SuccessServiceResult>(result);
            }
        }

        [Fact]
        public async void TestRemoveUserNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                UserService service = new UserService(context);

                //Act
                var result = await service.RemoveUser(new User() { Id = 2 });

                //Assert
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }



        public void Dispose() => _connection.Dispose();
    }
}