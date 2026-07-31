using FluentValidation;
using Wms.Application.DTOs.Products;

namespace Wms.Application.Validators.Products;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU товару є обов'язковим.")
            .MaximumLength(50).WithMessage("SKU не повинен перевищувати 50 символів.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва товару є обов'язковою.")
            .MaximumLength(200).WithMessage("Назва не повинна перевищувати 200 символів.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Ціна товару не може бути від'ємною.");

        RuleFor(x => x.UnitOfMeasure)
            .NotEmpty().WithMessage("Одиниця виміру є обов'язковою.")
            .MaximumLength(20).WithMessage("Одиниця виміру не повинна перевищувати 20 символів.");

        RuleFor(x => x.MinimumStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Поріг мінімального залишку не може бути від'ємним.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Категорія є обов'язковою.");
    }
}

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU товару є обов'язковим.")
            .MaximumLength(50).WithMessage("SKU не повинен перевищувати 50 символів.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва товару є обов'язковою.")
            .MaximumLength(200).WithMessage("Назва не повинна перевищувати 200 символів.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Ціна товару не може бути від'ємною.");

        RuleFor(x => x.UnitOfMeasure)
            .NotEmpty().WithMessage("Одиниця виміру є обов'язковою.")
            .MaximumLength(20).WithMessage("Одиниця виміру не повинна перевищувати 20 символів.");

        RuleFor(x => x.MinimumStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Поріг мінімального залишку не може бути від'ємним.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Категорія є обов'язковою.");
    }
}