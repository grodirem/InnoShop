using UserService.Application.Models.DTOs;
using UserService.Application.Models.Requests;

namespace UserService.Application.Interfaces;

public interface IUserManagementService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
    Task DeleteUserAsync(Guid id);
    Task ChangeUserStatusAsync(Guid userId, bool isActive);
}