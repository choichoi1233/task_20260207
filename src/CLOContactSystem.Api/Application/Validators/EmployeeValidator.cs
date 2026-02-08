using CLOContactSystem.Api.Application.DTOs;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CLOContactSystem.Api.Application.Validators;

public class EmployeeValidator
{
    private static readonly string[] DateFormats = { "yyyy.MM.dd", "yyyy-MM-dd", "yyyy/MM/dd" };
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^0\d{9,10}$", RegexOptions.Compiled);

    public List<string> Validate(CreateEmployeeDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Name is required.");

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            errors.Add("Email is required.");
        }
        else if (!EmailRegex.IsMatch(dto.Email.Trim()))
        {
            errors.Add($"Invalid email format: '{dto.Email}'.");
        }

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            errors.Add("Phone number is required.");
        }
        else
        {
            var normalized = dto.PhoneNumber.Replace("-", "").Replace(" ", "").Trim();
            if (!PhoneRegex.IsMatch(normalized))
                errors.Add($"Invalid phone number: '{dto.PhoneNumber}'.");
        }

        if (string.IsNullOrWhiteSpace(dto.JoinedDate))
        {
            errors.Add("Joined date is required.");
        }
        else if (!DateOnly.TryParseExact(dto.JoinedDate.Trim(), DateFormats,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            errors.Add($"Invalid date format: '{dto.JoinedDate}'. Expected: yyyy.MM.dd or yyyy-MM-dd.");
        }

        return errors;
    }

    public Dictionary<int, List<string>> ValidateAll(List<CreateEmployeeDto> dtos)
    {
        var allErrors = new Dictionary<int, List<string>>();
        for (int i = 0; i < dtos.Count; i++)
        {
            var errors = Validate(dtos[i]);
            if (errors.Count > 0)
                allErrors[i] = errors;
        }
        return allErrors;
    }
}
