using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CLOContactSystem.Api.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CLOContactSystem.Tests.Integration;

public class EmployeeApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EmployeeApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostJsonBody_Returns201()
    {
        var json = """
            [
                {"name":"김클로","email":"clo@clovf.com","tel":"010-1111-2424","joined":"2012-01-05"}
            ]
            """;

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/employee", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<EmployeeDto>>>(body, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Code.Should().Be(201);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Should().HaveCount(1);
    }

    [Fact]
    public async Task PostCsvBody_Returns201()
    {
        using var client = _factory.CreateClient();

        var csv = "박영희, matilda@clovf.com, 01087654321, 2021.04.28";
        var content = new StringContent(csv, Encoding.UTF8, "text/csv");
        var response = await client.PostAsync("/api/employee", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<EmployeeDto>>>(body, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Code.Should().Be(201);
    }

    [Fact]
    public async Task PostJsonFile_Returns201()
    {
        using var client = _factory.CreateClient();

        var json = """[{"name":"홍길동","email":"kildong.hong@clovf.com","tel":"01012345678","joined":"2015.08.15"}]""";
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "employees.json");

        var response = await client.PostAsync("/api/employee", formData);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<EmployeeDto>>>(body, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PostCsvFile_Returns201()
    {
        using var client = _factory.CreateClient();

        var csv = "홍커넥, connect@clovf.com, 010-8531-7942, 2019-12-05";
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csv));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "employees.csv");

        var response = await client.PostAsync("/api/employee", formData);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<EmployeeDto>>>(body, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PostFormFieldCsv_Returns201()
    {
        using var client = _factory.CreateClient();

        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("박마블, md@clovf.com, 010-3535-7979, 2013-07-01"), "csv");

        var response = await client.PostAsync("/api/employee", formData);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<EmployeeDto>>>(body, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PostFormFieldJson_Returns201()
    {
        using var client = _factory.CreateClient();

        var json = """[{"name":"이자바","email":"java@clovf.com","tel":"01099998888","joined":"2020-03-15"}]""";
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(json), "json");

        var response = await client.PostAsync("/api/employee", formData);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<EmployeeDto>>>(body, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_AfterPost_ReturnsPaginatedData()
    {
        using var client = _factory.CreateClient();

        // Add employees
        var json = """
            [
                {"name":"테스트일","email":"test1@clovf.com","tel":"01011111111","joined":"2020-01-01"},
                {"name":"테스트이","email":"test2@clovf.com","tel":"01022222222","joined":"2020-02-02"},
                {"name":"테스트삼","email":"test3@clovf.com","tel":"01033333333","joined":"2020-03-03"}
            ]
            """;
        var postContent = new StringContent(json, Encoding.UTF8, "application/json");
        await client.PostAsync("/api/employee", postContent);

        // Get all with pagination
        var response = await client.GetAsync("/api/employee?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedResult<EmployeeDto>>>(responseBody, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Code.Should().Be(200);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().HaveCount(2);
        apiResponse.Data.TotalCount.Should().Be(3);
        apiResponse.Data.Page.Should().Be(1);
        apiResponse.Data.PageSize.Should().Be(2);
        apiResponse.Data.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetByName_AfterPost_ReturnsCorrectEmployee()
    {
        using var client = _factory.CreateClient();

        var json = """[{"name":"조회테스트","email":"search@clovf.com","tel":"01044444444","joined":"2019-06-15"}]""";
        var postContent = new StringContent(json, Encoding.UTF8, "application/json");
        await client.PostAsync("/api/employee", postContent);

        var response = await client.GetAsync("/api/employee/%EC%A1%B0%ED%9A%8C%ED%85%8C%EC%8A%A4%ED%8A%B8");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(responseBody, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Code.Should().Be(200);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Name.Should().Be("조회테스트");
        apiResponse.Data.Email.Should().Be("search@clovf.com");
    }

    [Fact]
    public async Task GetByName_NotExists_Returns404()
    {
        var response = await _client.GetAsync("/api/employee/%EC%97%86%EB%8A%94%EC%82%AC%EB%9E%8C");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(body, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Code.Should().Be(404);
        apiResponse.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Post_InvalidData_Returns400()
    {
        var json = """[{"name":"","email":"bad","tel":"123","joined":"nope"}]""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/employee", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(body, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Code.Should().Be(400);
        apiResponse.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Post_EmptyBody_Returns400()
    {
        var content = new StringContent("", Encoding.UTF8, "text/plain");
        var response = await _client.PostAsync("/api/employee", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(body, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Code.Should().Be(400);
    }

    [Fact]
    public async Task GetAll_InvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/api/employee?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(body, JsonOpts);

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Code.Should().Be(400);
        apiResponse.Message.Should().NotBeNullOrEmpty();
    }
}
