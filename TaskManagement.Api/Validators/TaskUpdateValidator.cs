using FluentValidation;
using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Validators
{
    public class TaskUpdateValidator : AbstractValidator<TaskUpdateDto>
    {
        public TaskUpdateValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500);

            RuleFor(x => x.DueDate)
                .Must(d => d == null || d.Value > DateTime.UtcNow)
                .WithMessage("DueDate must be in the future.");
        }
    }
}
