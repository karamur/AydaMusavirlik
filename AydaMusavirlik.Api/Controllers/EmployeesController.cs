using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AydaMusavirlik.Data.Repositories;
using AydaMusavirlik.Core.Models.Payroll;

namespace AydaMusavirlik.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("company/{companyId}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetByCompany(int companyId)
    {
        var employees = await _unitOfWork.Employees.GetByCompanyAsync(companyId);
        return Ok(employees.Select(e => MapToDto(e)));
    }

    [HttpGet("company/{companyId}/active")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetActiveByCompany(int companyId)
    {
        var employees = await _unitOfWork.Employees.GetActiveEmployeesAsync(companyId);
        return Ok(employees.Select(e => MapToDto(e)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        return Ok(MapToDto(employee));
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(CreateEmployeeDto dto)
    {
        var existing = await _unitOfWork.Employees.GetByTcKimlikAsync(dto.TcKimlikNo);
        if (existing != null)
            return BadRequest("Bu TC Kimlik No ile kayitli personel mevcut.");

        var employee = new Employee
        {
            CompanyId = dto.CompanyId,
            EmployeeNumber = dto.EmployeeNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            TcKimlikNo = dto.TcKimlikNo,
            BirthDate = dto.BirthDate,
            Gender = dto.Gender,
            MaritalStatus = dto.MaritalStatus,
            NumberOfChildren = dto.NumberOfChildren,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            IbanNumber = dto.IbanNumber,
            SgkNumber = dto.SgkNumber,
            HireDate = dto.HireDate,
            Department = dto.Department,
            Position = dto.Position,
            EmploymentType = dto.EmploymentType,
            WorkType = dto.WorkType,
            GrossSalary = dto.GrossSalary,
            SalaryType = dto.SalaryType,
            IsMinimumWageExempt = dto.IsMinimumWageExempt,
            IsDisabled = dto.IsDisabled,
            DisabilityDegree = dto.DisabilityDegree
        };

        await _unitOfWork.Employees.AddAsync(employee);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, MapToDto(employee));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        employee.EmployeeNumber = dto.EmployeeNumber;
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Address = dto.Address;
        employee.Phone = dto.Phone;
        employee.Email = dto.Email;
        employee.IbanNumber = dto.IbanNumber;
        employee.Department = dto.Department;
        employee.Position = dto.Position;
        employee.GrossSalary = dto.GrossSalary;
        employee.TerminationDate = dto.TerminationDate;

        await _unitOfWork.Employees.UpdateAsync(employee);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        await _unitOfWork.Employees.DeleteAsync(employee);
        return NoContent();
    }

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        Id = e.Id,
        CompanyId = e.CompanyId,
        EmployeeNumber = e.EmployeeNumber,
        FirstName = e.FirstName,
        LastName = e.LastName,
        FullName = e.FullName,
        TcKimlikNo = e.TcKimlikNo,
        Department = e.Department,
        Position = e.Position,
        HireDate = e.HireDate,
        TerminationDate = e.TerminationDate,
        GrossSalary = e.GrossSalary,
        IsActive = e.TerminationDate == null
    };
}

public class EmployeeDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal GrossSalary { get; set; }
    public bool IsActive { get; set; }
}

public class CreateEmployeeDto
{
    public int CompanyId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public int NumberOfChildren { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IbanNumber { get; set; }
    public string? SgkNumber { get; set; }
    public DateTime HireDate { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public WorkType WorkType { get; set; }
    public decimal GrossSalary { get; set; }
    public SalaryType SalaryType { get; set; }
    public bool IsMinimumWageExempt { get; set; }
    public bool IsDisabled { get; set; }
    public int? DisabilityDegree { get; set; }
}

public class UpdateEmployeeDto
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IbanNumber { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public decimal GrossSalary { get; set; }
    public DateTime? TerminationDate { get; set; }
}