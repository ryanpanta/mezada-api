using FluentValidation;
using WebApiMezada.DTOs.Suggestion;

namespace WebApiMezada.Services.FamilyGroup.Validators;

public class SuggestionCreateValidator : AbstractValidator<SuggestionCreateDTO>
{
    public SuggestionCreateValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("A mensagem da sugestão é obrigatória.")
            .MaximumLength(500).WithMessage("A mensagem não pode ter mais de 500 caracteres.");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID da sugestão é obrigatório para edição.")
            .When(x => !string.IsNullOrEmpty(x.Id));
    }
}