using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.API.Controllers;
using UserService.Application.Interfaces;
using UserService.Application.Models.Requests;

namespace UserService.UnitTests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IUserManagementService> _userServiceMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _userServiceMock = new Mock<IUserManagementService>();
        _controller = new AdminController(_userServiceMock.Object);

        SetupAdminContext();
    }

    private void SetupAdminContext()
    {
        var user = TestData.GetTestUser(TestData.TestUserId, "Admin");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task ChangeUserStatus_WithValidRequest_ReturnsOk()
    {
        var request = new ChangeUserStatusRequest
        {
            UserId = TestData.OtherUserId,
            IsActive = false
        };

        _userServiceMock
            .Setup(service => service.ChangeUserStatusAsync(request.UserId, request.IsActive))
            .Returns(Task.CompletedTask);

        var result = await _controller.ChangeUserStatus(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be("User status changed to Inactive");
    }

    [Fact]
    public async Task ChangeUserStatus_WithActiveStatus_ReturnsCorrectMessage()
    {
        var request = new ChangeUserStatusRequest
        {
            UserId = TestData.OtherUserId,
            IsActive = true
        };

        _userServiceMock
            .Setup(service => service.ChangeUserStatusAsync(request.UserId, request.IsActive))
            .Returns(Task.CompletedTask);

        var result = await _controller.ChangeUserStatus(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.Value.Should().Be("User status changed to Active");
    }

    [Fact]
    public async Task ChangeUserStatus_WithInvalidRequest_ReturnsBadRequest()
    {
        var request = new ChangeUserStatusRequest
        {
            UserId = Guid.Empty,
            IsActive = false
        };

        var result = await _controller.ChangeUserStatus(request);

        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ChangeUserStatus_CallsServiceWithCorrectParameters()
    {
        var request = new ChangeUserStatusRequest
        {
            UserId = TestData.OtherUserId,
            IsActive = false
        };

        _userServiceMock
            .Setup(service => service.ChangeUserStatusAsync(request.UserId, request.IsActive))
            .Returns(Task.CompletedTask);

        await _controller.ChangeUserStatus(request);

        _userServiceMock.Verify(service =>
            service.ChangeUserStatusAsync(request.UserId, request.IsActive),
            Times.Once);
    }
}