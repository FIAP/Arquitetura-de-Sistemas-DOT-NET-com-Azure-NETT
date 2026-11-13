using FluentValidation;
using TaskManagement.Functions.Models.DTOs;

namespace TaskManagement.Functions.Validators
{
    public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
    {
        public UpdateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200)
                .WithMessage("O título deve ter no máximo 200 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("A descrição deve ter no máximo 1000 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.AssignedTo)
                .MaximumLength(100)
                .WithMessage("O responsável deve ter no máximo 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.AssignedTo));

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Status inválido")
                .When(x => x.Status.HasValue);

            RuleFor(x => x.Tags)
                .Must(tags => tags == null || tags.Count <= 10)
                .WithMessage("Máximo de 10 tags permitidas")
                .Must(tags => tags == null || tags.All(tag => !string.IsNullOrWhiteSpace(tag) && tag.Length <= 50))
                .WithMessage("Cada tag deve ter entre 1 e 50 caracteres")
                .When(x => x.Tags != null);
        }
    }
}
