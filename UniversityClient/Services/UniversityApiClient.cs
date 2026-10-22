using System.Net.Http.Headers;
using System.Net.Http.Json;
using UniversityClient.Models;

namespace UniversityClient.Services;

public class UniversityApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UniversityApiClient> _logger;

    public UniversityApiClient(HttpClient httpClient, ILogger<UniversityApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<ApiResponse<List<CourseV1>>> GetCoursesV1Async() => SendAsync<List<CourseV1>>(HttpMethod.Get, "api/v1/courses");
    public Task<ApiResponse<List<CourseV2>>> GetCoursesV2Async() => SendAsync<List<CourseV2>>(HttpMethod.Get, "api/v2/courses");
    public Task<ApiResponse<CourseV1>> GetCourseV1Async(int id) => SendAsync<CourseV1>(HttpMethod.Get, $"api/v1/courses/{id}");
    public Task<ApiResponse<CourseV2>> GetCourseV2Async(int id) => SendAsync<CourseV2>(HttpMethod.Get, $"api/v2/courses/{id}");

    public Task<ApiResponse<TokenResponse>> LoginAsync(string userName, string password) =>
        SendAsync<TokenResponse>(HttpMethod.Post, "api/auth/login", body: new { userName, password });

    public Task<ApiResponse<UserInfo>> GetMeAsync(string token) => SendAsync<UserInfo>(HttpMethod.Get, "api/auth/me", token);
    public Task<ApiResponse<List<EnrollmentInfo>>> GetEnrollmentsAsync(string token) => SendAsync<List<EnrollmentInfo>>(HttpMethod.Get, "api/enrollments", token);

    private async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string url, string? token = null, object? body = null) where T : class
    {
        using var request = new HttpRequestMessage(method, url);
        if (token != null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body != null)
            request.Content = JsonContent.Create(body);

        try
        {
            using var response = await _httpClient.SendAsync(request);
            var envelope = await ReadEnvelopeAsync<T>(response);

            if (response.IsSuccessStatusCode && envelope?.Result != null)
                return ApiResponse<T>.Ok(envelope.Result);

            var status = (int)response.StatusCode;
            _logger.LogWarning("API {Method} {Url} returned {StatusCode}", method, url, status);

            var message = envelope?.ErrorMessage ?? status switch
            {
                401 => "Требуется вход в систему",
                403 => "Недостаточно прав",
                404 => "Не найдено",
                _ => $"API вернул ошибку {status}"
            };
            return ApiResponse<T>.Fail(message, status);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API is unavailable: {Method} {Url}", method, url);
            return ApiResponse<T>.Fail("API недоступен. Проверьте, что University Course Management API запущен.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "API request timed out: {Method} {Url}", method, url);
            return ApiResponse<T>.Fail("API не ответил вовремя. Попробуйте позже.");
        }
    }

    private static async Task<ApiEnvelope<T>?> ReadEnvelopeAsync<T>(HttpResponseMessage response) where T : class
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>();
        }
        catch (Exception)
        {
            return null;
        }
    }
}
