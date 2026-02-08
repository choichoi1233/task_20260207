using CLOContactSystem.Api.Application.DTOs;
using CLOContactSystem.Api.Application.Mapping;
using CLOContactSystem.Api.Domain.Interfaces;
using MediatR;

namespace CLOContactSystem.Api.Application.Queries.GetEmployees;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, PaginatedResult<EmployeeDto>>
{
    private readonly IEmployeeRepository _repository;
    private readonly ILogger<GetEmployeesQueryHandler> _logger;

    public GetEmployeesQueryHandler(IEmployeeRepository repository, ILogger<GetEmployeesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PaginatedResult<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching employees - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        var (items, totalCount) = await _repository.GetAllAsync(request.Page, request.PageSize, cancellationToken);

        _logger.LogInformation("Found {Count} employees (total: {Total})", items.Count, totalCount);

        return new PaginatedResult<EmployeeDto>
        {
            Items = items.Select(e => e.ToDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
