using CLOContactSystem.Api.Application.DTOs;
using CLOContactSystem.Api.Domain.Entities;
using System.Globalization;

namespace CLOContactSystem.Api.Application.Mapping;

public static class EmployeeMappingExtensions
{
    private static readonly string[] DateFormats = { "yyyy.MM.dd", "yyyy-MM-dd", "yyyy/MM/dd" };

    public static Employee ToEntity(this CreateEmployeeDto dto)
    {
        return new Employee
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            PhoneNumber = NormalizePhone(dto.PhoneNumber),
            JoinedDate = ParseFlexibleDate(dto.JoinedDate),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static EmployeeDto ToDto(this Employee entity)
    {
        return new EmployeeDto
        {
            Name = entity.Name,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            JoinedDate = entity.JoinedDate.ToString("yyyy-MM-dd")
        };
    }

    public static string NormalizePhone(string phone)
    {
        return phone.Replace("-", "").Replace(" ", "").Trim();
    }

    public static DateOnly ParseFlexibleDate(string input)
    {
        if (DateOnly.TryParseExact(input.Trim(), DateFormats, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var date))
        {
            return date;
        }

        throw new FormatException($"Cannot parse date: '{input}'. Expected formats: yyyy.MM.dd, yyyy-MM-dd, yyyy/MM/dd");
    }
}
