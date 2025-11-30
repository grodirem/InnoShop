using FluentAssertions;
using Moq;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.UnitTests.Services;

public class UserManagementServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IProductServiceClient> _productServiceClientMock;
    private readonly UserManagementService _userService;

    public UserManagementServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _productServiceClientMock = new Mock<IProductServiceClient>();
        _userService = new UserManagementService(
            _userRepositoryMock.Object,
            _productServiceClientMock.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUser()
    {
        var userId = TestData.TestUserId;
        var user = TestData.GetTestUserEntity(userId);

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var result = await _userService.GetUserByIdAsync(userId);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Name.Should().Be("Test User");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var userId = TestData.TestUserId;

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User)null);

        var result = await _userService.GetUserByIdAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        var users = new List<User>
        {
            TestData.GetTestUserEntity(TestData.TestUserId),
            TestData.GetAdminUserEntity(TestData.AdminUserId)
        };

        _userRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        var result = await _userService.GetAllUsersAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Role == "User");
        result.Should().Contain(u => u.Role == "Admin");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserExists_UpdatesUser()
    {
        var userId = TestData.TestUserId;
        var existingUser = TestData.GetTestUserEntity(userId);
        var updateUserDto = TestData.GetUpdateUserDto();

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(repo => repo.ExistsByEmailAsync(updateUserDto.Email))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _userService.UpdateUserAsync(userId, updateUserDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Test User");
        result.Email.Should().Be("updated@example.com");
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenEmailAlreadyTaken_ThrowsArgumentException()
    {
        var userId = TestData.TestUserId;
        var existingUser = TestData.GetTestUserEntity(userId);
        var updateUserDto = TestData.GetUpdateUserDto();

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(repo => repo.ExistsByEmailAsync(updateUserDto.Email))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.UpdateUserAsync(userId, updateUserDto));
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_MarksUserAsInactive()
    {
        var userId = TestData.TestUserId;
        var user = TestData.GetTestUserEntity(userId, status: UserStatus.Active);

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _productServiceClientMock
            .Setup(client => client.UpdateProductsUserStatusAsync(userId, false))
            .Returns(Task.CompletedTask);

        await _userService.DeleteUserAsync(userId);

        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<User>(u =>
            u.Status == UserStatus.Inactive)), Times.Once);
        _productServiceClientMock.Verify(client =>
            client.UpdateProductsUserStatusAsync(userId, false), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserDoesNotExist_ThrowsArgumentException()
    {
        var userId = TestData.TestUserId;

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.DeleteUserAsync(userId));
    }

    [Fact]
    public async Task ChangeUserStatusAsync_WhenUserExists_ChangesStatusAndUpdatesProducts()
    {
        var userId = TestData.TestUserId;
        var user = TestData.GetTestUserEntity(userId, status: UserStatus.Active);

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _productServiceClientMock
            .Setup(client => client.UpdateProductsUserStatusAsync(userId, false))
            .Returns(Task.CompletedTask);

        await _userService.ChangeUserStatusAsync(userId, false);

        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<User>(u =>
            u.Status == UserStatus.Inactive)), Times.Once);
        _productServiceClientMock.Verify(client =>
            client.UpdateProductsUserStatusAsync(userId, false), Times.Once);
    }

    [Fact]
    public async Task ChangeUserStatusAsync_WhenUserDoesNotExist_ThrowsArgumentException()
    {
        var userId = TestData.TestUserId;

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.ChangeUserStatusAsync(userId, true));
    }
}