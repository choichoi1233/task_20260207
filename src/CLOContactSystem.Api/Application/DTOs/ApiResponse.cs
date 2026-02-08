namespace CLOContactSystem.Api.Application.DTOs;

/// <summary>
/// 모든 API 응답에 사용되는 공통 응답 모델
/// </summary>
/// <typeparam name="T">반환 데이터 타입</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 요청 성공 여부
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 에러 또는 안내 메시지 (성공 시 null)
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// HTTP 상태 코드
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 반환 데이터 (실패 시 null)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 성공 응답 생성
    /// </summary>
    public static ApiResponse<T> Ok(T data, int code = 200) => new()
    {
        Success = true,
        Message = null,
        Code = code,
        Data = data
    };

    /// <summary>
    /// 실패 응답 생성
    /// </summary>
    public static ApiResponse<T> Fail(string message, int code = 400) => new()
    {
        Success = false,
        Message = message,
        Code = code,
        Data = default
    };
}
