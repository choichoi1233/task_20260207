using CLOContactSystem.Api.Application.DTOs;
using MediatR;

namespace CLOContactSystem.Api.Application.Queries.GetEmployeeByName;

public record GetEmployeeByNameQuery(string Name) : IRequest<EmployeeDto?>;
