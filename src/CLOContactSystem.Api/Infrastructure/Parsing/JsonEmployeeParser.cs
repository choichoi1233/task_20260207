using CLOContactSystem.Api.Application.DTOs;
using System.Text.Json;

namespace CLOContactSystem.Api.Infrastructure.Parsing;

public class JsonEmployeeParser : IEmployeeParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public List<CreateEmployeeDto> Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new List<CreateEmployeeDto>();

        content = content.Trim();

        // Handle single object (wrap in array)
        if (content.StartsWith("{"))
            content = $"[{content}]";

        try
        {
            var employees = JsonSerializer.Deserialize<List<CreateEmployeeDto>>(content, Options);
            return employees ?? throw new FormatException("JSON deserialization returned null.");
        }
        catch (JsonException ex)
        {
            throw new FormatException($"Invalid JSON format: {ex.Message}", ex);
        }
    }
}
