using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.API.Controllers;
using UserService.Application.Interfaces;
using UserService.Application.Models.DTOs;

namespace UserService.UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserManagementService> _userServiceMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userServiceMock = new Mock<IUserManagementService>();
        _controller = new UsersController(_userServiceMock.Object);

        SetupUserContext(TestData.TestUserId, "User");
    }

    private void SetupUserContext(Guid userId, string role)
    {
        var user = TestData.GetTestUser(userId, role);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetUser_WhenUserExists_ReturnsOk()
    {
        var userId = TestData.TestUserId;
        var userDto = new UserDto { Id = userId, Name = "Test User" };

        _userServiceMock
            .Setup(service => service.GetUserByIdAsync(userId))
            .ReturnsAsync(userDto);

        var result = await _controller.GetUser(userId);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var userId = TestData.TestUserId;

        _userServiceMock
            .Setup(service => service.GetUserByIdAsync(userId))
            .ReturnsAsync((UserDto)null);

        var result = await _controller.GetUser(userId);

        var notFoundResult = result as NotFoundResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetAllUsers_WhenAdmin_ReturnsAllUsers()
    {
        SetupUserContext(TestData.TestUserId, "Admin");
        var users = new List<UserDto>
        {
            new UserDto { Id = TestData.TestUserId, Name = "User 1" },
            new UserDto { Id = TestData.AdminUserId, Name = "User 2" }
        };

        _userServiceMock
            .Setup(service => service.GetAllUsersAsync())
            .ReturnsAsync(users);

        var result = await _controller.GetAllUsers();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var returnedUsers = okResult.Value as List<UserDto>;
        returnedUsers.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateUser_WhenUserIsOwner_UpdatesUser()
    {
        var userId = TestData.TestUserId;
        var updateUserDto = TestData.GetUpdateUserDto();
        var updatedUser = new UserDto { Id = userId, Name = "Updated User" };

        _userServiceMock
            .Setup(service => service.UpdateUserAsync(userId, updateUserDto))
            .ReturnsAsync(updatedUser);

        var result = await _controller.UpdateUser(userId, updateUserDto);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateUser_WhenUserIsNotOwnerAndNotAdmin_ReturnsForbid()
    {
        var differentUserId = TestData.OtherUserId;
        var updateUserDto = TestData.GetUpdateUserDto();

        var result = await _controller.UpdateUser(differentUserId, updateUserDto);

        var forbidResult = result as ForbidResult;
        forbidResult.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteUser_WhenUserIsOwner_DeletesUser()
    {
        var userId = TestData.TestUserId;

        _userServiceMock
            .Setup(service => service.DeleteUserAsync(userId))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteUser(userId);

        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult.StatusCode.Should().Be(204);
    }
}