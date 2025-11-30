using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.API.Controllers;
using UserService.Application.Interfaces;
using UserService.Application.Models.DTOs;
using UserService.Application.Models.Requests;

namespace UserService.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        var loginDto = TestData.GetLoginDto();
        var authResponse = new AuthResponseDto
        {
            Token = "jwt-token",
            User = new UserDto { Id = TestData.TestUserId, Email = loginDto.Email },
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _authServiceMock
            .Setup(service => service.LoginAsync(loginDto))
            .ReturnsAsync(authResponse);

        var result = await _controller.Login(loginDto);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ConfirmEmail_WithValidToken_ReturnsOk()
    {
        var request = new ConfirmEmailRequest { Token = "valid-token" };

        _authServiceMock
            .Setup(service => service.ConfirmEmailAsync(request.Token))
            .Returns(Task.CompletedTask);

        var result = await _controller.ConfirmEmail(request.Token);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ReturnsOk()
    {
        var request = new ForgotPasswordRequest { Email = "test@example.com" };

        _authServiceMock
            .Setup(service => service.ForgotPasswordAsync(request.Email))
            .Returns(Task.CompletedTask);

        var result = await _controller.ForgotPassword(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
    }
}