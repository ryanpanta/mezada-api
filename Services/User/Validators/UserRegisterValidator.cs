using FluentValidation;
using WebApiMezada.DTOs.User;

namespace WebApiMezada.Services.User.Validators
{
    public class UserRegisterValidator : AbstractValidator<UserRegisterDTO>
    {
        public UserRegisterValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("O e-mail é inválido.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("A senha deve ter pelo menos 6 caracteres.");
        }
    }
}
