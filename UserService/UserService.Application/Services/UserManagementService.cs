using UserService.Application.Interfaces;
using UserService.Application.Models.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductServiceClient _productServiceClient;

    public UserManagementService(IUserRepository userRepository, IProductServiceClient productServiceClient)
    {
        _userRepository = userRepository;
        _productServiceClient = productServiceClient;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        if (user.Email != updateUserDto.Email &&
            await _userRepository.ExistsByEmailAsync(updateUserDto.Email))
        {
            throw new ArgumentException("Email already taken");
        }

        user.Name = updateUserDto.Name;
        user.Email = updateUserDto.Email;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        user.Status = UserStatus.Inactive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _productServiceClient.UpdateProductsUserStatusAsync(user.Id, false);
    }

    public async Task ChangeUserStatusAsync(Guid userId, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        user.Status = isActive ? UserStatus.Active : UserStatus.Inactive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _productServiceClient.UpdateProductsUserStatusAsync(user.Id, isActive);
    }

    private UserDto MapToDto(User user) => new UserDto
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        Status = user.Status.ToString(),
        IsEmailConfirmed = user.IsEmailConfirmed,
        CreatedAt = user.CreatedAt
    };
}