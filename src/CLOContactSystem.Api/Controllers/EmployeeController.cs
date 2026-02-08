using CLOContactSystem.Api.Application.Commands.CreateEmployees;
using CLOContactSystem.Api.Application.DTOs;
using CLOContactSystem.Api.Application.Queries.GetEmployeeByName;
using CLOContactSystem.Api.Application.Queries.GetEmployees;
using CLOContactSystem.Api.Infrastructure.Parsing;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CLOContactSystem.Api.Controllers;

/// <summary>
/// 직원 긴급 연락 정보 관리 API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmployeeController> _logger;
    private readonly CsvEmployeeParser _csvParser;
    private readonly JsonEmployeeParser _jsonParser;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public EmployeeController(
        IMediator mediator,
        ILogger<EmployeeController> logger,
        CsvEmployeeParser csvParser,
        JsonEmployeeParser jsonParser)
    {
        _mediator = mediator;
        _logger = logger;
        _csvParser = csvParser;
        _jsonParser = jsonParser;
    }

    /// <summary>
    /// 직원들의 기본 연락 정보를 페이징하여 조회합니다.
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작)</param>
    /// <param name="pageSize">페이지 크기</param>
    /// <param name="ct">취소 토큰</param>
    /// <returns>페이징된 직원 목록</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<EmployeeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (page < 1)
            return ApiError<object>("Page must be greater than or equal to 1.", StatusCodes.Status400BadRequest);
        if (pageSize < 1 || pageSize > 100)
            return ApiError<object>("PageSize must be between 1 and 100.", StatusCodes.Status400BadRequest);

        _logger.LogInformation("GET /api/employee - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var query = new GetEmployeesQuery(page, pageSize);
        var result = await _mediator.Send(query, ct);
        return ApiOk(result);
    }

    /// <summary>
    /// 이름으로 직원의 상세 연락 정보를 조회합니다.
    /// </summary>
    /// <param name="name">직원 이름</param>
    /// <param name="ct">취소 토큰</param>
    /// <returns>직원 연락 정보</returns>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name, CancellationToken ct = default)
    {
        var decodedName = Uri.UnescapeDataString(name);
        _logger.LogInformation("GET /api/employee/{Name}", decodedName);

        var query = new GetEmployeeByNameQuery(decodedName);
        var result = await _mediator.Send(query, ct);

        if (result is null)
            return ApiError<object>($"Employee '{decodedName}' not found.", StatusCodes.Status404NotFound);

        return ApiOk(result);
    }

    /// <summary>
    /// 직원의 기본 연락 정보를 추가합니다.
    /// CSV/JSON 파일 업로드 또는 텍스트 직접 입력을 지원합니다.
    /// </summary>
    /// <remarks>
    /// 지원 형식:
    /// - multipart/form-data로 CSV 또는 JSON 파일 업로드 (파일 필드명: file)
    /// - multipart/form-data로 CSV 텍스트 직접 입력 (필드명: csv)
    /// - multipart/form-data로 JSON 텍스트 직접 입력 (필드명: json)
    /// - application/json 바디로 JSON 직접 입력
    /// - text/csv 또는 text/plain 바디로 CSV 직접 입력
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<List<EmployeeDto>>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        List<CreateEmployeeDto> employees;

        try
        {
            if (Request.HasFormContentType)
            {
                employees = await HandleFormDataAsync(ct);
            }
            else
            {
                employees = await HandleRawBodyAsync(ct);
            }
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Failed to parse employee data");
            return ApiError<object>(ex.Message, StatusCodes.Status400BadRequest);
        }

        if (employees.Count == 0)
        {
            return ApiError<object>("No employee data provided.", StatusCodes.Status400BadRequest);
        }

        _logger.LogInformation("POST /api/employee - Parsed {Count} employee(s)", employees.Count);

        var command = new CreateEmployeesCommand(employees);
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            if (result.ValidationErrors.Count > 0)
                return ApiError<object>("Validation failed.", StatusCodes.Status400BadRequest);

            if (result.DuplicateNames.Count > 0)
                return ApiError<object>($"Duplicate employee names: {string.Join(", ", result.DuplicateNames)}", StatusCodes.Status400BadRequest);

            return ApiError<object>("Failed to create employees.", StatusCodes.Status400BadRequest);
        }

        return ApiCreated(result.CreatedEmployees);
    }

    /// <summary>
    /// 성공 응답 (200 OK)
    /// </summary>
    private ContentResult ApiOk<T>(T data)
    {
        var response = ApiResponse<T>.Ok(data, StatusCodes.Status200OK);
        return ToJsonContent(response, StatusCodes.Status200OK);
    }

    /// <summary>
    /// 생성 성공 응답 (201 Created)
    /// </summary>
    private ContentResult ApiCreated<T>(T data)
    {
        var response = ApiResponse<T>.Ok(data, StatusCodes.Status201Created);
        return ToJsonContent(response, StatusCodes.Status201Created);
    }

    /// <summary>
    /// 실패 응답
    /// </summary>
    private ContentResult ApiError<T>(string message, int statusCode)
    {
        var response = ApiResponse<T>.Fail(message, statusCode);
        return ToJsonContent(response, statusCode);
    }

    private ContentResult ToJsonContent<T>(ApiResponse<T> response, int statusCode)
    {
        return new ContentResult
        {
            Content = JsonSerializer.Serialize(response, JsonOptions),
            ContentType = "application/json",
            StatusCode = statusCode
        };
    }

    private async Task<List<CreateEmployeeDto>> HandleFormDataAsync(CancellationToken ct)
    {
        var form = await Request.ReadFormAsync(ct);
        var file = form.Files.FirstOrDefault();

        if (file is not null && file.Length > 0)
        {
            _logger.LogInformation("Processing file upload: {FileName} ({ContentType})", file.FileName, file.ContentType);
            var content = await ReadFileContentAsync(file);
            return ParseByFileType(file.FileName, file.ContentType, content);
        }

        // Check for text field input
        var csvText = form["csv"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(csvText))
        {
            _logger.LogInformation("Processing CSV text from form field");
            return _csvParser.Parse(csvText);
        }

        var jsonText = form["json"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(jsonText))
        {
            _logger.LogInformation("Processing JSON text from form field");
            return _jsonParser.Parse(jsonText);
        }

        // Check for generic "data" field and auto-detect format
        var dataText = form["data"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(dataText))
        {
            _logger.LogInformation("Processing data text from form field (auto-detecting format)");
            return DetectAndParse(dataText);
        }

        throw new FormatException("No file or text data provided. Use 'file' for file upload, 'csv' or 'json' for text input.");
    }

    private async Task<List<CreateEmployeeDto>> HandleRawBodyAsync(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var bodyContent = await reader.ReadToEndAsync(ct);

        if (string.IsNullOrWhiteSpace(bodyContent))
            throw new FormatException("Request body is empty.");

        var contentType = Request.ContentType?.ToLowerInvariant() ?? "";

        if (contentType.Contains("application/json"))
        {
            _logger.LogInformation("Processing JSON from request body");
            return _jsonParser.Parse(bodyContent);
        }

        if (contentType.Contains("text/csv"))
        {
            _logger.LogInformation("Processing CSV from request body");
            return _csvParser.Parse(bodyContent);
        }

        // Auto-detect format
        _logger.LogInformation("Auto-detecting format from request body");
        return DetectAndParse(bodyContent);
    }

    private List<CreateEmployeeDto> ParseByFileType(string fileName, string? contentType, string content)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

        return extension switch
        {
            ".csv" => _csvParser.Parse(content),
            ".json" => _jsonParser.Parse(content),
            _ when contentType?.Contains("json") == true => _jsonParser.Parse(content),
            _ when contentType?.Contains("csv") == true => _csvParser.Parse(content),
            _ => DetectAndParse(content)
        };
    }

    private List<CreateEmployeeDto> DetectAndParse(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("[") || trimmed.StartsWith("{"))
            return _jsonParser.Parse(trimmed);

        return _csvParser.Parse(trimmed);
    }

    private static async Task<string> ReadFileContentAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
