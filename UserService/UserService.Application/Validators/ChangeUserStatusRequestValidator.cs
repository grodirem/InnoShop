using FluentValidation;
using UserService.Application.Models.Requests;

namespace UserService.Application.Validators;

public class ChangeUserStatusRequestValidator : AbstractValidator<ChangeUserStatusRequest>
{
    public ChangeUserStatusRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}