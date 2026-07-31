using FluentValidation;
using Wms.Application.DTOs.Categories;

namespace Wms.Application.Validators.Categories;

public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва категорії є обов'язковою.")
            .MaximumLength(150).WithMessage("Назва не повинна перевищувати 150 символів.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Опис не повинен перевищувати 500 символів.");
    }
}

public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва категорії є обов'язковою.")
            .MaximumLength(150).WithMessage("Назва не повинна перевищувати 150 символів.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Опис не повинен перевищувати 500 символів.");
    }
}