using CLOContactSystem.Api.Application.DTOs;
using CLOContactSystem.Api.Application.Validators;
using FluentAssertions;

namespace CLOContactSystem.Tests.Validators;

public class EmployeeValidatorTests
{
    private readonly EmployeeValidator _validator = new();

    private static CreateEmployeeDto ValidDto() => new()
    {
        Name = "김철수",
        Email = "charles@clovf.com",
        PhoneNumber = "01075312468",
        JoinedDate = "2018.03.07"
    };

    [Fact]
    public void Validate_ValidEmployee_NoErrors()
    {
        var errors = _validator.Validate(ValidDto());

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var dto = ValidDto();
        dto.Name = "";

        var errors = _validator.Validate(dto);

        errors.Should().Contain(e => e.Contains("Name"));
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsError()
    {
        var dto = ValidDto();
        dto.Email = "invalid-email";

        var errors = _validator.Validate(dto);

        errors.Should().Contain(e => e.Contains("email"));
    }

    [Theory]
    [InlineData("charles@clovf.com")]
    [InlineData("kildong.hong@clovf.com")]
    [InlineData("user@example.co.kr")]
    public void Validate_ValidEmailFormats_NoErrors(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var errors = _validator.Validate(dto);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_PhoneWithHyphens_Valid()
    {
        var dto = ValidDto();
        dto.PhoneNumber = "010-1111-2424";

        var errors = _validator.Validate(dto);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_PhoneWithoutHyphens_Valid()
    {
        var dto = ValidDto();
        dto.PhoneNumber = "01075312468";

        var errors = _validator.Validate(dto);

        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abcdefghijk")]
    [InlineData("")]
    public void Validate_InvalidPhone_ReturnsError(string phone)
    {
        var dto = ValidDto();
        dto.PhoneNumber = phone;

        var errors = _validator.Validate(dto);

        errors.Should().Contain(e => e.ToLower().Contains("phone") || e.ToLower().Contains("required"));
    }

    [Fact]
    public void Validate_DotDateFormat_Valid()
    {
        var dto = ValidDto();
        dto.JoinedDate = "2018.03.07";

        var errors = _validator.Validate(dto);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_DashDateFormat_Valid()
    {
        var dto = ValidDto();
        dto.JoinedDate = "2012-01-05";

        var errors = _validator.Validate(dto);

        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("2018/13/45")]
    [InlineData("invalid-date")]
    [InlineData("")]
    public void Validate_InvalidDate_ReturnsError(string date)
    {
        var dto = ValidDto();
        dto.JoinedDate = date;

        var errors = _validator.Validate(dto);

        errors.Should().Contain(e => e.ToLower().Contains("date") || e.ToLower().Contains("required"));
    }

    [Fact]
    public void ValidateAll_MixedValid_ReturnsOnlyErrors()
    {
        var dtos = new List<CreateEmployeeDto>
        {
            ValidDto(),
            new() { Name = "", Email = "bad", PhoneNumber = "123", JoinedDate = "nope" },
            ValidDto()
        };

        var errors = _validator.ValidateAll(dtos);

        errors.Should().ContainKey(1);
        errors.Should().NotContainKey(0);
        errors.Should().NotContainKey(2);
    }
}
