using CLOContactSystem.Api.Infrastructure.Parsing;
using FluentAssertions;

namespace CLOContactSystem.Tests.Parsing;

public class JsonEmployeeParserTests
{
    private readonly JsonEmployeeParser _parser = new();

    [Fact]
    public void Parse_ValidArray_ReturnsEmployees()
    {
        var json = """
            [
                {"name":"김클로", "email":"clo@clovf.com", "tel":"010-1111-2424", "joined":"2012-01-05"},
                {"name":"박마블", "email":"md@clovf.com", "tel":"010-3535-7979", "joined":"2013-07-01"}
            ]
            """;

        var result = _parser.Parse(json);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("김클로");
        result[0].Email.Should().Be("clo@clovf.com");
        result[0].PhoneNumber.Should().Be("010-1111-2424");
        result[0].JoinedDate.Should().Be("2012-01-05");
        result[1].Name.Should().Be("박마블");
    }

    [Fact]
    public void Parse_SingleObject_ReturnsOneEmployee()
    {
        var json = """{"name":"김클로", "email":"clo@clovf.com", "tel":"010-1111-2424", "joined":"2012-01-05"}""";

        var result = _parser.Parse(json);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("김클로");
    }

    [Fact]
    public void Parse_EmptyArray_ReturnsEmptyList()
    {
        var result = _parser.Parse("[]");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsEmptyList()
    {
        var result = _parser.Parse("");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsFormatException()
    {
        var json = "{ invalid json }";

        var act = () => _parser.Parse(json);

        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid JSON format*");
    }

    [Fact]
    public void Parse_CaseInsensitiveProperties_Works()
    {
        var json = """[{"Name":"김클로", "Email":"clo@clovf.com", "Tel":"010-1111-2424", "Joined":"2012-01-05"}]""";

        var result = _parser.Parse(json);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("김클로");
    }

    [Fact]
    public void Parse_MissingFields_ReturnsDefaultValues()
    {
        var json = """[{"name":"김클로"}]""";

        var result = _parser.Parse(json);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("김클로");
        result[0].Email.Should().BeEmpty();
        result[0].PhoneNumber.Should().BeEmpty();
        result[0].JoinedDate.Should().BeEmpty();
    }
}
