using FluentValidation;
using MediatR;
using AydaMusavirlik.Application.Common.Interfaces;
using AydaMusavirlik.Application.Common.Models;
using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Application.Features.Companies.Commands.CreateCompany;

/// <summary>
/// Yeni firma olustur command
/// </summary>
public record CreateCompanyCommand : IRequest<Result<int>>
{
    public string Name { get; init; } = string.Empty;
    public string? TaxNumber { get; init; }
    public string? TaxOffice { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Website { get; init; }
    public CompanyType CompanyType { get; init; }
    public decimal? Capital { get; init; }
    public DateTime? FoundationDate { get; init; }
    public string? TradeRegistryNumber { get; init; }
    public string? MersisNumber { get; init; }
}

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCompanyCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Firma adi zorunludur.")
            .MaximumLength(200).WithMessage("Firma adi 200 karakteri asamaz.");

        RuleFor(v => v.TaxNumber)
            .MaximumLength(11).WithMessage("Vergi numarasi 11 karakter olmalidir.")
            .MustAsync(BeUniqueTaxNumber).WithMessage("Bu vergi numarasi zaten kayitli.");

        RuleFor(v => v.Email)
            .EmailAddress().When(v => !string.IsNullOrEmpty(v.Email))
            .WithMessage("Gecerli bir email adresi giriniz.");

        RuleFor(v => v.Capital)
            .GreaterThan(0).When(v => v.Capital.HasValue)
            .WithMessage("Sermaye sifirdan buyuk olmalidir.");
    }

    private async Task<bool> BeUniqueTaxNumber(string? taxNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(taxNumber)) return true;
        var existing = await _unitOfWork.Companies.GetByTaxNumberAsync(taxNumber, cancellationToken);
        return existing == null;
    }
}

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCompanyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = new Company
        {
            Name = request.Name,
            TaxNumber = request.TaxNumber,
            TaxOffice = request.TaxOffice,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            Website = request.Website,
            CompanyType = request.CompanyType,
            Capital = request.Capital,
            FoundationDate = request.FoundationDate,
            TradeRegistryNumber = request.TradeRegistryNumber,
            MersisNumber = request.MersisNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Companies.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(company.Id);
    }
}