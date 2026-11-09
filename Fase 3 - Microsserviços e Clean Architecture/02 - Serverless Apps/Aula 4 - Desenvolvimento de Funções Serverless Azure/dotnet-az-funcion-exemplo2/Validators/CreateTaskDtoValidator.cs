using FluentValidation;
using TaskManagement.Functions.Models.DTOs;

namespace TaskManagement.Functions.Validators
{
    public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("O título é obrigatório")
                .MaximumLength(200)
                .WithMessage("O título deve ter no máximo 200 caracteres");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("A descrição deve ter no máximo 1000 caracteres");

            RuleFor(x => x.AssignedTo)
                .MaximumLength(100)
                .WithMessage("O responsável deve ter no máximo 100 caracteres");

            RuleFor(x => x.Tags)
                .Must(tags => tags == null || tags.Count <= 10)
                .WithMessage("Máximo de 10 tags permitidas")
                .Must(tags => tags == null || tags.All(tag => !string.IsNullOrWhiteSpace(tag) && tag.Length <= 50))
                .WithMessage("Cada tag deve ter entre 1 e 50 caracteres");
        }
    }
}
