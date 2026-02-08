using CLOContactSystem.Api.Application.DTOs;
using CLOContactSystem.Api.Application.Mapping;
using CLOContactSystem.Api.Domain.Interfaces;
using MediatR;

namespace CLOContactSystem.Api.Application.Queries.GetEmployeeByName;

public class GetEmployeeByNameQueryHandler : IRequestHandler<GetEmployeeByNameQuery, EmployeeDto?>
{
    private readonly IEmployeeRepository _repository;
    private readonly ILogger<GetEmployeeByNameQueryHandler> _logger;

    public GetEmployeeByNameQueryHandler(IEmployeeRepository repository, ILogger<GetEmployeeByNameQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<EmployeeDto?> Handle(GetEmployeeByNameQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching for employee: {Name}", request.Name);

        var employee = await _repository.GetByNameAsync(request.Name, cancellationToken);

        if (employee is null)
        {
            _logger.LogWarning("Employee not found: {Name}", request.Name);
            return null;
        }

        _logger.LogInformation("Employee found: {Name}", request.Name);
        return employee.ToDto();
    }
}
