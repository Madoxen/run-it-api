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
    public class FriendServiceTests : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<ApiContext> _contextOptions;

        public FriendServiceTests()
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
                };
                var friend = new User()
                {
                    Id = 2,
                    Weight = 1
                };
                var requestedFriend = new User()
                {
                    Id = 3,
                    Weight = 1
                };
                var newGuy = new User()
                {
                    Id = 4,
                    Weight = 1
                };

                context.AddRange(user, friend, requestedFriend, newGuy);
                context.Friends.AddRange(new Friend()
                {
                    Date = DateTimeOffset.UtcNow,
                    Receiver = user,
                    Requester = friend,
                    ReceiverId = user.Id,
                    RequesterId = friend.Id,
                    Status = AcceptanceStatus.Friends
                },
                new Friend()
                {
                    Date = DateTimeOffset.UtcNow,
                    Receiver = user,
                    Requester = requestedFriend,
                    ReceiverId = user.Id,
                    RequesterId = requestedFriend.Id,
                    Status = AcceptanceStatus.Requested
                });

                context.SaveChanges();
            }
        }

        [Fact]
        public async void TestGetFriends()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.GetFriends(1);
                var reverseResult = await service.GetFriends(2);

                //Assert
                Assert.IsType<List<User>>(result.Value);
                Assert.NotNull(result.Value);

                Assert.IsType<List<User>>(reverseResult.Value);
                Assert.NotNull(reverseResult.Value);
            }
        }

        [Fact]
        public async void TestGetFriendsNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.GetFriends(5);

                //Assert
                Assert.IsType<NotFoundServiceResult>(result.Result);
            }
        }

        [Fact]
        public async void TestGetRequests()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.GetRequests(1);
                var reverseResult = await service.GetRequests(3);

                //Assert
                Assert.IsType<List<User>>(result.Value);
                Assert.NotNull(result.Value);
                Assert.Single(result.Value);

                Assert.IsType<List<User>>(reverseResult.Value);
                Assert.NotNull(reverseResult.Value);
                Assert.Empty(reverseResult.Value);
            }
        }

        [Fact]
        public async void TestGetRequestsNonExistingUser()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.GetRequests(5);

                //Assert
                Assert.IsType<NotFoundServiceResult>(result.Result);
            }
        }

        [Fact]
        public async void TestSendFriendRequestToAlreadyRequstedFriendship()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.SendFriendRequest(1, 3);
                Assert.IsType<SuccessServiceResult>(result);
                Assert.NotNull(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var result = await context.Friends.FirstOrDefaultAsync(x => x.ReceiverId == 1 && x.RequesterId == 3);
                Assert.Equal(AcceptanceStatus.Friends, result.Status);
            }
        }

        [Fact]
        public async void TestSendFriendRequestToNewRequstedFriendship()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.SendFriendRequest(1, 4);
                Assert.IsType<SuccessServiceResult>(result);
                Assert.NotNull(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var result = await context.Friends.FirstOrDefaultAsync(x => x.RequesterId == 1 && x.ReceiverId == 4);
                Assert.Equal(AcceptanceStatus.Requested, result.Status);
            }
        }

        [Fact]
        public async void TestFullFriendshipFlow()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.SendFriendRequest(1, 4);
                var result1 = await service.SendFriendRequest(4, 1);

                Assert.IsType<SuccessServiceResult>(result);
                Assert.NotNull(result);

                Assert.IsType<SuccessServiceResult>(result1);
                Assert.NotNull(result1);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var result = await context.Friends.FirstOrDefaultAsync(x => x.RequesterId == 1 && x.ReceiverId == 4);
                Assert.Equal(AcceptanceStatus.Friends, result.Status);
            }
        }

        [Fact]
        public async void TestSameIdConflict()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.SendFriendRequest(4, 4);
                Assert.IsType<ConflictServiceResult>(result);
                Assert.NotNull(result);
            }
        }


        [Fact]
        public async void TestRemoveExistingFriendship()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.RemoveFriend(1, 2);


                Assert.IsType<SuccessServiceResult>(result);
                Assert.NotNull(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var result = await context.Friends.FirstOrDefaultAsync(x => x.RequesterId == 1 && x.ReceiverId == 2);
                Assert.Null(result);
            }
        }

        [Fact]
        public async void TestRemoveExistingFriendshipReverse()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.RemoveFriend(2, 1);


                Assert.IsType<SuccessServiceResult>(result);
                Assert.NotNull(result);
            }

            //Assert
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                var result = await context.Friends.FirstOrDefaultAsync(x => x.RequesterId == 1 && x.ReceiverId == 2);
                Assert.Null(result);
            }
        }

        [Fact]
        public async void TestRemoveNonExisting()
        {
            using (ApiContext context = new ApiContext(_contextOptions))
            {
                //Arrange
                FriendService service = new FriendService(context);

                //Act
                var result = await service.RemoveFriend(1, 4);


                Assert.IsType<NotFoundServiceResult>(result);
            }
        }



        public void Dispose() => _connection.Dispose();
    }
}