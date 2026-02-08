using CLOContactSystem.Api.Application.Queries.GetEmployees;
using CLOContactSystem.Api.Domain.Entities;
using CLOContactSystem.Api.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CLOContactSystem.Tests.Handlers;

public class GetEmployeesQueryHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly Mock<ILogger<GetEmployeesQueryHandler>> _loggerMock;
    private readonly GetEmployeesQueryHandler _handler;

    public GetEmployeesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<GetEmployeesQueryHandler>>();
        _handler = new GetEmployeesQueryHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPagedResults()
    {
        var employees = new List<Employee>
        {
            new() { Id = 1, Name = "김철수", Email = "charles@clovf.com", PhoneNumber = "01075312468", JoinedDate = new DateOnly(2018, 3, 7) },
            new() { Id = 2, Name = "박영희", Email = "matilda@clovf.com", PhoneNumber = "01087654321", JoinedDate = new DateOnly(2021, 4, 28) }
        };

        _repositoryMock.Setup(r => r.GetAllAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((employees, 5));

        var query = new GetEmployeesQuery(1, 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        _repositoryMock.Setup(r => r.GetAllAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Employee>(), 0));

        var query = new GetEmployeesQuery(1, 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Page2_CorrectPageInfo()
    {
        _repositoryMock.Setup(r => r.GetAllAsync(2, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Employee>
            {
                new() { Id = 3, Name = "홍길동", Email = "hong@clovf.com", PhoneNumber = "01012345678", JoinedDate = new DateOnly(2015, 8, 15) }
            }, 5));

        var query = new GetEmployeesQuery(2, 2);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }
}
