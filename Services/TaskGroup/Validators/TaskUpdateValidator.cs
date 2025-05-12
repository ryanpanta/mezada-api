using FluentValidation;
using WebApiMezada.DTOs.Task;
using WebApiMezada.Models.Enums;

namespace WebApiMezada.Services.TaskGroup.Validators
{
    public class TaskUpdateValidator : AbstractValidator<TaskUpdateDTO>
    {
        public TaskUpdateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("O título da tarefa é obrigatório.")
                .MaximumLength(100).WithMessage("O título não pode ter mais de 100 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("A descrição não pode ter mais de 500 caracteres.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("A categoria deve ser Recompensa ou Penalidade.");

            RuleFor(x => x.DefaultIncrement)
                .NotEqual(0).WithMessage("O incremento padrão não pode ser zero.")
                .GreaterThan(-1000).WithMessage("O incremento padrão não pode ser menor que -1000.")
                .LessThan(1000).WithMessage("O incremento padrão não pode ser maior que 1000.");

            RuleFor(x => x.LimitValue)
                .GreaterThanOrEqualTo(0).WithMessage("O valor limite deve ser maior ou igual a zero.")
                .When(x => x.Category == EnumCategory.Reward)
                .LessThanOrEqualTo(10000).WithMessage("O valor limite não pode ser maior que 10000.");

            RuleFor(x => x.LimitValue)
                .Equal(0).WithMessage("O valor limite deve ser 0 para penalidades.")
                .When(x => x.Category == EnumCategory.Penalty);
            
        }
    }
}
