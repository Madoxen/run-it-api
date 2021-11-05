using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Api.Models;
using Api.Repositories;
using Xunit;
using Api.Controllers;
using Api.Tests.Mocks;
using Microsoft.AspNetCore.Mvc;

namespace Api.Tests
{
    public class UserControllerUnitTests
    {
        [Fact]
        public async void TestGetEndpointWithExistingID()
        {
            //Arrange
            MockUserRepo repo = new MockUserRepo();
            UserController controller = new UserController(repo);
            await repo.Add(new User() { Id = 1 });

            //Act
            IActionResult result = await controller.Get(1);

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(((OkObjectResult)result).Value);
        }


        [Fact]
        public async void TestGetEndpointWithNonExistantID()
        {
            //Arrange
            MockUserRepo repo = new MockUserRepo();
            UserController controller = new UserController(repo);
            await repo.Add(new User() { Id = 2 });

            //Act
            IActionResult result = await controller.Get(1);

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Null(((OkObjectResult)result).Value);
        }
    }
}
