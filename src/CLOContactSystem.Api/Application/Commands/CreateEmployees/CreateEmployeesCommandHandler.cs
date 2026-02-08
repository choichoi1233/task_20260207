using CLOContactSystem.Api.Application.DTOs;
using CLOContactSystem.Api.Application.Mapping;
using CLOContactSystem.Api.Application.Validators;
using CLOContactSystem.Api.Domain.Interfaces;
using MediatR;

namespace CLOContactSystem.Api.Application.Commands.CreateEmployees;

public class CreateEmployeesCommandHandler : IRequestHandler<CreateEmployeesCommand, CreateEmployeesResult>
{
    private readonly IEmployeeRepository _repository;
    private readonly EmployeeValidator _validator;
    private readonly ILogger<CreateEmployeesCommandHandler> _logger;

    public CreateEmployeesCommandHandler(
        IEmployeeRepository repository,
        EmployeeValidator validator,
        ILogger<CreateEmployeesCommandHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<CreateEmployeesResult> Handle(CreateEmployeesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating {Count} employee(s)", request.Employees.Count);

        if (request.Employees.Count == 0)
        {
            _logger.LogWarning("Empty employee list received");
            return new CreateEmployeesResult
            {
                Success = false,
                ValidationErrors = new Dictionary<int, List<string>>
                {
                    [0] = new List<string> { "No employee data provided." }
                }
            };
        }

        // Validate all employees
        var validationErrors = _validator.ValidateAll(request.Employees);
        if (validationErrors.Count > 0)
        {
            _logger.LogWarning("Validation failed for {Count} employee(s)", validationErrors.Count);
            return new CreateEmployeesResult
            {
                Success = false,
                ValidationErrors = validationErrors
            };
        }

        // Check for duplicate names within the batch
        var batchNames = request.Employees.Select(e => e.Name.Trim()).ToList();
        var batchDuplicates = batchNames
            .GroupBy(n => n)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (batchDuplicates.Count > 0)
        {
            _logger.LogWarning("Duplicate names found within batch: {Names}", string.Join(", ", batchDuplicates));
            return new CreateEmployeesResult
            {
                Success = false,
                DuplicateNames = batchDuplicates
            };
        }

        // Check for existing names in database
        var existingNames = await _repository.GetExistingNamesAsync(batchNames, cancellationToken);
        if (existingNames.Count > 0)
        {
            _logger.LogWarning("Duplicate names found in database: {Names}", string.Join(", ", existingNames));
            return new CreateEmployeesResult
            {
                Success = false,
                DuplicateNames = existingNames
            };
        }

        // Map to entities and save
        var entities = request.Employees.Select(dto => dto.ToEntity()).ToList();
        await _repository.AddRangeAsync(entities, cancellationToken);

        _logger.LogInformation("Successfully created {Count} employee(s)", entities.Count);

        return new CreateEmployeesResult
        {
            Success = true,
            CreatedEmployees = entities.Select(e => e.ToDto()).ToList()
        };
    }
}
