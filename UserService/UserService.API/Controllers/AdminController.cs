using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Interfaces;
using UserService.Application.Models.Requests;
using UserService.Application.Validators;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IUserManagementService _userService;

    public AdminController(IUserManagementService userService)
    {
        _userService = userService;
    }

    [HttpPost("change-user-status")]
    public async Task<IActionResult> ChangeUserStatus([FromBody] ChangeUserStatusRequest request)
    {
        var validator = new ChangeUserStatusRequestValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        await _userService.ChangeUserStatusAsync(request.UserId, request.IsActive);
        return Ok($"User status changed to {(request.IsActive ? "Active" : "Inactive")}");
    }
}