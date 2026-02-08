using System.Text.Json.Serialization;

namespace CLOContactSystem.Api.Application.DTOs;

public class CreateEmployeeDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("tel")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("joined")]
    public string JoinedDate { get; set; } = string.Empty;
}
