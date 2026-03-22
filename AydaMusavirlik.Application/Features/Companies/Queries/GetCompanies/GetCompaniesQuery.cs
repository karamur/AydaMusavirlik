using MediatR;
using AydaMusavirlik.Application.Common.Interfaces;
using AydaMusavirlik.Application.Common.Models;

namespace AydaMusavirlik.Application.Features.Companies.Queries.GetCompanies;

/// <summary>
/// Tum firmalari getir query
/// </summary>
public record GetCompaniesQuery : IRequest<Result<List<CompanyDto>>>;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, Result<List<CompanyDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCompaniesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<CompanyDto>>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _unitOfWork.Companies.GetActiveCompaniesAsync(cancellationToken);

        var dtos = companies.Select(c => new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            TaxNumber = c.TaxNumber,
            TaxOffice = c.TaxOffice,
            Address = c.Address,
            Phone = c.Phone,
            Email = c.Email,
            CompanyType = c.CompanyType.ToString(),
            Capital = c.Capital,
            IsActive = c.IsActive
        }).ToList();

        return Result<List<CompanyDto>>.Success(dtos);
    }
}

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string CompanyType { get; set; } = string.Empty;
    public decimal? Capital { get; set; }
    public bool IsActive { get; set; }
}