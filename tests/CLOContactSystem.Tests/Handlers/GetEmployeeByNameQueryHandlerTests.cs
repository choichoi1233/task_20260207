using CLOContactSystem.Api.Application.Queries.GetEmployeeByName;
using CLOContactSystem.Api.Domain.Entities;
using CLOContactSystem.Api.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CLOContactSystem.Tests.Handlers;

public class GetEmployeeByNameQueryHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly Mock<ILogger<GetEmployeeByNameQueryHandler>> _loggerMock;
    private readonly GetEmployeeByNameQueryHandler _handler;

    public GetEmployeeByNameQueryHandlerTests()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<GetEmployeeByNameQueryHandler>>();
        _handler = new GetEmployeeByNameQueryHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_EmployeeExists_ReturnsDto()
    {
        var employee = new Employee
        {
            Id = 1,
            Name = "김철수",
            Email = "charles@clovf.com",
            PhoneNumber = "01075312468",
            JoinedDate = new DateOnly(2018, 3, 7)
        };

        _repositoryMock.Setup(r => r.GetByNameAsync("김철수", It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var query = new GetEmployeeByNameQuery("김철수");
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("김철수");
        result.Email.Should().Be("charles@clovf.com");
        result.PhoneNumber.Should().Be("01075312468");
        result.JoinedDate.Should().Be("2018-03-07");
    }

    [Fact]
    public async Task Handle_EmployeeNotExists_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByNameAsync("존재하지않는이름", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var query = new GetEmployeeByNameQuery("존재하지않는이름");
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
