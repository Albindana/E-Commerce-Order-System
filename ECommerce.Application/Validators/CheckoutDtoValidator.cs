using ECommerce.Application.DTOs.Order;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class CheckoutDtoValidator : AbstractValidator<CheckoutDto>
{
    public CheckoutDtoValidator()
    {
        RuleFor(x => x.ShippingAddress).NotNull();
        RuleFor(x => x.ShippingAddress.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ShippingAddress.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ShippingAddress.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ShippingAddress.ZipCode).NotEmpty().MaximumLength(20);
    }
}
