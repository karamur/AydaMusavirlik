using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AydaMusavirlik.Data.Repositories;
using AydaMusavirlik.Core.Models.Common;

namespace AydaMusavirlik.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(IUnitOfWork unitOfWork, ILogger<CompaniesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAll()
    {
        var companies = await _unitOfWork.Companies.GetActiveCompaniesAsync();
        return Ok(companies.Select(c => new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            TaxNumber = c.TaxNumber,
            TaxOffice = c.TaxOffice,
            City = c.City,
            Phone = c.Phone,
            Email = c.Email
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetById(int id)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company == null)
            return NotFound();

        return Ok(new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            TaxNumber = company.TaxNumber,
            TaxOffice = company.TaxOffice,
            TradeRegistryNumber = company.TradeRegistryNumber,
            MersisNumber = company.MersisNumber,
            Address = company.Address,
            City = company.City,
            District = company.District,
            Phone = company.Phone,
            Email = company.Email,
            Website = company.Website,
            Capital = company.Capital,
            FoundationDate = company.FoundationDate,
            CompanyType = company.CompanyType
        });
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(CreateCompanyDto dto)
    {
        var existing = await _unitOfWork.Companies.GetByTaxNumberAsync(dto.TaxNumber ?? "");
        if (existing != null)
            return BadRequest("Bu vergi numarasi ile kayitli firma mevcut.");

        var company = new Company
        {
            Name = dto.Name,
            TaxNumber = dto.TaxNumber,
            TaxOffice = dto.TaxOffice,
            TradeRegistryNumber = dto.TradeRegistryNumber,
            MersisNumber = dto.MersisNumber,
            Address = dto.Address,
            City = dto.City,
            District = dto.District,
            Phone = dto.Phone,
            Email = dto.Email,
            Website = dto.Website,
            Capital = dto.Capital,
            FoundationDate = dto.FoundationDate,
            CompanyType = dto.CompanyType
        };

        await _unitOfWork.Companies.AddAsync(company);
        _logger.LogInformation("Yeni firma olusturuldu: {CompanyName}", company.Name);

        return CreatedAtAction(nameof(GetById), new { id = company.Id }, new CompanyDto { Id = company.Id, Name = company.Name });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCompanyDto dto)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company == null)
            return NotFound();

        company.Name = dto.Name;
        company.TaxNumber = dto.TaxNumber;
        company.TaxOffice = dto.TaxOffice;
        company.TradeRegistryNumber = dto.TradeRegistryNumber;
        company.MersisNumber = dto.MersisNumber;
        company.Address = dto.Address;
        company.City = dto.City;
        company.District = dto.District;
        company.Phone = dto.Phone;
        company.Email = dto.Email;
        company.Website = dto.Website;
        company.Capital = dto.Capital;
        company.FoundationDate = dto.FoundationDate;
        company.CompanyType = dto.CompanyType;

        await _unitOfWork.Companies.UpdateAsync(company);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company == null)
            return NotFound();

        await _unitOfWork.Companies.DeleteAsync(company);
        return NoContent();
    }
}

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? TradeRegistryNumber { get; set; }
    public string? MersisNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public decimal? Capital { get; set; }
    public DateTime? FoundationDate { get; set; }
    public CompanyType CompanyType { get; set; }
}

public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? TradeRegistryNumber { get; set; }
    public string? MersisNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public decimal? Capital { get; set; }
    public DateTime? FoundationDate { get; set; }
    public CompanyType CompanyType { get; set; }
}

public class UpdateCompanyDto : CreateCompanyDto
{
}