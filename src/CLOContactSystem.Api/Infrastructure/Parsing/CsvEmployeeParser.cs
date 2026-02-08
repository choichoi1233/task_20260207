using CLOContactSystem.Api.Application.DTOs;

namespace CLOContactSystem.Api.Infrastructure.Parsing;

public class CsvEmployeeParser : IEmployeeParser
{
    public List<CreateEmployeeDto> Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new List<CreateEmployeeDto>();

        var results = new List<CreateEmployeeDto>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',').Select(p => p.Trim()).ToArray();
            if (parts.Length < 4)
                throw new FormatException(
                    $"Invalid CSV at line {i + 1}: expected at least 4 fields (name, email, phone, joined date), got {parts.Length}.");

            results.Add(new CreateEmployeeDto
            {
                Name = parts[0],
                Email = parts[1],
                PhoneNumber = parts[2],
                JoinedDate = parts[3]
            });
        }

        return results;
    }
}
