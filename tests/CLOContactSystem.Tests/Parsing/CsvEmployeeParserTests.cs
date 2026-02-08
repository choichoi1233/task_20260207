using CLOContactSystem.Api.Infrastructure.Parsing;
using FluentAssertions;

namespace CLOContactSystem.Tests.Parsing;

public class CsvEmployeeParserTests
{
    private readonly CsvEmployeeParser _parser = new();

    [Fact]
    public void Parse_ValidSingleLine_ReturnsOneEmployee()
    {
        var csv = "김철수, charles@clovf.com, 01075312468, 2018.03.07";

        var result = _parser.Parse(csv);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("김철수");
        result[0].Email.Should().Be("charles@clovf.com");
        result[0].PhoneNumber.Should().Be("01075312468");
        result[0].JoinedDate.Should().Be("2018.03.07");
    }

    [Fact]
    public void Parse_ValidMultipleLines_ReturnsAll()
    {
        var csv = """
            김철수, charles@clovf.com, 01075312468, 2018.03.07
            박영희, matilda@clovf.com, 01087654321, 2021.04.28
            홍길동, kildong.hong@clovf.com, 01012345678, 2015.08.15
            """;

        var result = _parser.Parse(csv);

        result.Should().HaveCount(3);
        result[0].Name.Should().Be("김철수");
        result[1].Name.Should().Be("박영희");
        result[2].Name.Should().Be("홍길동");
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsEmptyList()
    {
        var result = _parser.Parse("");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhitespaceOnly_ReturnsEmptyList()
    {
        var result = _parser.Parse("   \n  \n  ");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_MalformedLine_ThrowsFormatException()
    {
        var csv = "김철수, charles@clovf.com";

        var act = () => _parser.Parse(csv);

        act.Should().Throw<FormatException>()
            .WithMessage("*expected at least 4 fields*");
    }

    [Fact]
    public void Parse_TrimsWhitespace_Correctly()
    {
        var csv = "  김철수  ,  charles@clovf.com  ,  01075312468  ,  2018.03.07  ";

        var result = _parser.Parse(csv);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("김철수");
        result[0].Email.Should().Be("charles@clovf.com");
        result[0].PhoneNumber.Should().Be("01075312468");
        result[0].JoinedDate.Should().Be("2018.03.07");
    }

    [Fact]
    public void Parse_ExtraFields_TakesFirstFour()
    {
        var csv = "김철수, charles@clovf.com, 01075312468, 2018.03.07, 개발팀";

        var result = _parser.Parse(csv);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("김철수");
    }

    [Fact]
    public void Parse_PhoneWithHyphens_PreservedInDto()
    {
        var csv = "김클로, clo@clovf.com, 010-1111-2424, 2012-01-05";

        var result = _parser.Parse(csv);

        result[0].PhoneNumber.Should().Be("010-1111-2424");
    }
}
