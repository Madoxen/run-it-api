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
    public class RunServiceTests : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<ApiContext> _contextOptions;

        public RunServiceTests()
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

                var run = new Run()
                {
                    Id = 1
                };

                var user = new User()
                {
                    Runs = new List<Run>() { run }
                };

                context.Runs.Add(run);
                context.Users.Add(user);
                context.SaveChanges();
            }
        }

        [Fact]
        public async void TestGetRunById()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.GetRunById(1);

                //Assert
                Assert.Equal(1, result.Id);
                Assert.NotNull(result);
            }
        }

        [Fact]
        public async void TestGetRunByIdNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.GetRunById(2);

                //Assert
                Assert.Null(result);
            }
        }


        [Fact]
        public async void TestGetUserRuns()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.GetUserRuns(1);

                //Assert
                Assert.NotNull(result.Value);
                Assert.Single(result.Value);
            }
        }

        [Fact]
        public async void TestGetUserRunsNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.GetUserRuns(2);

                //Assert
                Assert.Null(result.Value);
                Assert.NotNull(result.Result);
                Assert.IsType<NotFoundServiceResult>(result.Result);
            }
        }

        [Fact]
        public async void TestCreateRun()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.CreateRun(new Run() { UserId = 1, Id = 2 });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<SuccessServiceResult>(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var run = await context.Runs.FirstOrDefaultAsync(x => x.Id == 2);
                Assert.NotNull(run);
            }
        }

        [Fact]
        public async void TestCreateRunForNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.CreateRun(new Run() { UserId = 2, Id = 2 });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        [Fact]
        public async void TestUpdateRun()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.UpdateRun(new Run() { Id = 1, UserId = 1, Title = "xd" });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<SuccessServiceResult>(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var run = await context.Runs.FirstOrDefaultAsync(x => x.Id == 1);
                Assert.NotNull(run);
                Assert.Equal("xd", run.Title);
            }
        }

        [Fact]
        public async void TestCreateUpdateForNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.UpdateRun(new Run() { UserId = 2, Id = 1 });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        [Fact]
        public async void TestRemoveRun()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.RemoveRun(new Run() { Id = 1 });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<SuccessServiceResult>(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var run = await context.Runs.FirstOrDefaultAsync(x => x.Id == 1);
                Assert.Null(run);
            }
        }

        [Fact]
        public async void TestRemoveRunNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.RemoveRun(new Run() { Id = 2 });

                //Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        [Fact]
        public async void TestRemoveRunById()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.RemoveRunById(1);

                //Assert
                Assert.NotNull(result);
                Assert.IsType<SuccessServiceResult>(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var run = await context.Runs.FirstOrDefaultAsync(x => x.Id == 1);
                Assert.Null(run);
            }
        }

        [Fact]
        public async void TestRemoveRunByIdNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                RunService service = new RunService(context);

                //Act
                var result = await service.RemoveRunById(2);

                //Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundServiceResult>(result);
            }
        }

        





        public void Dispose() => _connection.Dispose();
    }
}