using MediatR;
using AydaMusavirlik.Application.Common.Interfaces;
using AydaMusavirlik.Application.Common.Models;
using AydaMusavirlik.Application.Common.Exceptions;

namespace AydaMusavirlik.Application.Features.Companies.Queries.GetCompanyById;

/// <summary>
/// ID ile firma getir query
/// </summary>
public record GetCompanyByIdQuery(int Id) : IRequest<Result<CompanyDetailDto>>;

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, Result<CompanyDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCompanyByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CompanyDetailDto>> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(request.Id, cancellationToken);

        if (company == null)
            throw new NotFoundException("Company", request.Id);

        var dto = new CompanyDetailDto
        {
            Id = company.Id,
            Name = company.Name,
            TaxNumber = company.TaxNumber,
            TaxOffice = company.TaxOffice,
            Address = company.Address,
            Phone = company.Phone,
            Email = company.Email,
            Website = company.Website,
            CompanyType = company.CompanyType.ToString(),
            Capital = company.Capital,
            FoundationDate = company.FoundationDate,
            TradeRegistryNumber = company.TradeRegistryNumber,
            MersisNumber = company.MersisNumber,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt
        };

        return Result<CompanyDetailDto>.Success(dto);
    }
}

public class CompanyDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string CompanyType { get; set; } = string.Empty;
    public decimal? Capital { get; set; }
    public DateTime? FoundationDate { get; set; }
    public string? TradeRegistryNumber { get; set; }
    public string? MersisNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}