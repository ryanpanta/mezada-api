using FluentValidation;
using WebApiMezada.DTOs.Task;

namespace WebApiMezada.Services.TaskGroup.Validators
{
    public class TaskCreateValidator : AbstractValidator<TaskCreateDTO>
    {
        public TaskCreateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("O título da tarefa é obrigatório.")
                .MaximumLength(100).WithMessage("O título não pode ter mais de 100 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("A descrição não pode ter mais de 500 caracteres.");

            RuleFor(x => x.Points)
                .GreaterThan(0).WithMessage("Os pontos devem ser maiores que zero.");
        }
    }
}
