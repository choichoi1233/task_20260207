using CLOContactSystem.Api.Application.DTOs;
using MediatR;

namespace CLOContactSystem.Api.Application.Commands.CreateEmployees;

public record CreateEmployeesCommand(List<CreateEmployeeDto> Employees) : IRequest<CreateEmployeesResult>;

public class CreateEmployeesResult
{
    public bool Success { get; set; }
    public List<EmployeeDto> CreatedEmployees { get; set; } = new();
    public Dictionary<int, List<string>> ValidationErrors { get; set; } = new();
    public List<string> DuplicateNames { get; set; } = new();
}
