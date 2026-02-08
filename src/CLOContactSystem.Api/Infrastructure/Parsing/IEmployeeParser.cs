using CLOContactSystem.Api.Application.DTOs;

namespace CLOContactSystem.Api.Infrastructure.Parsing;

public interface IEmployeeParser
{
    List<CreateEmployeeDto> Parse(string content);
}
