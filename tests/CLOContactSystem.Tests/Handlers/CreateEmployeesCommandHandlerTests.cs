using CLOContactSystem.Api.Application.Commands.CreateEmployees;
using CLOContactSystem.Api.Application.DTOs;
using CLOContactSystem.Api.Application.Validators;
using CLOContactSystem.Api.Domain.Entities;
using CLOContactSystem.Api.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CLOContactSystem.Tests.Handlers;

public class CreateEmployeesCommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly EmployeeValidator _validator;
    private readonly Mock<ILogger<CreateEmployeesCommandHandler>> _loggerMock;
    private readonly CreateEmployeesCommandHandler _handler;

    public CreateEmployeesCommandHandlerTests()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _validator = new EmployeeValidator();
        _loggerMock = new Mock<ILogger<CreateEmployeesCommandHandler>>();
        _handler = new CreateEmployeesCommandHandler(_repositoryMock.Object, _validator, _loggerMock.Object);
    }

    private static CreateEmployeeDto ValidDto(string name = "김철수") => new()
    {
        Name = name,
        Email = "charles@clovf.com",
        PhoneNumber = "01075312468",
        JoinedDate = "2018.03.07"
    };

    [Fact]
    public async Task Handle_ValidEmployees_ReturnsSuccess()
    {
        var command = new CreateEmployeesCommand(new List<CreateEmployeeDto> { ValidDto() });
        _repositoryMock.Setup(r => r.GetExistingNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.CreatedEmployees.Should().HaveCount(1);
        result.CreatedEmployees[0].Name.Should().Be("김철수");
        _repositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Employee>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsFailed()
    {
        var command = new CreateEmployeesCommand(new List<CreateEmployeeDto>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidationFails_ReturnsErrors()
    {
        var dto = new CreateEmployeeDto
        {
            Name = "",
            Email = "invalid",
            PhoneNumber = "123",
            JoinedDate = "bad"
        };
        var command = new CreateEmployeesCommand(new List<CreateEmployeeDto> { dto });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_DuplicateNamesInBatch_ReturnsFailed()
    {
        var command = new CreateEmployeesCommand(new List<CreateEmployeeDto>
        {
            ValidDto("김철수"),
            ValidDto("김철수")
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.DuplicateNames.Should().Contain("김철수");
    }

    [Fact]
    public async Task Handle_DuplicateNameInDatabase_ReturnsFailed()
    {
        var command = new CreateEmployeesCommand(new List<CreateEmployeeDto> { ValidDto("김철수") });
        _repositoryMock.Setup(r => r.GetExistingNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "김철수" });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.DuplicateNames.Should().Contain("김철수");
    }

    [Fact]
    public async Task Handle_MultipleValidEmployees_ReturnsAll()
    {
        var command = new CreateEmployeesCommand(new List<CreateEmployeeDto>
        {
            ValidDto("김철수"),
            new() { Name = "박영희", Email = "matilda@clovf.com", PhoneNumber = "01087654321", JoinedDate = "2021.04.28" }
        });
        _repositoryMock.Setup(r => r.GetExistingNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.CreatedEmployees.Should().HaveCount(2);
    }
}
