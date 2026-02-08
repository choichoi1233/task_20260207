using CLOContactSystem.Api.Application.DTOs;
using MediatR;

namespace CLOContactSystem.Api.Application.Queries.GetEmployees;

public record GetEmployeesQuery(int Page, int PageSize) : IRequest<PaginatedResult<EmployeeDto>>;
