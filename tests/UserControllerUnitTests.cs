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

            //Act
            IActionResult result = await controller.Get(1);

            //Assert            
        }


    }
}
