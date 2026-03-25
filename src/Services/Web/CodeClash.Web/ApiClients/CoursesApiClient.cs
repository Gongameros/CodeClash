using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using CodeClash.Web.ApiClients.Models;
using CodeClash.Web.ApiClients.Models.Requests;
using CodeClash.Web.ApiClients.Models.Responses;
using CodeClash.Web.Auth;

namespace CodeClash.Web.ApiClients;

public sealed class CoursesApiClient(
    HttpClient httpClient,
    TokenAccessor tokenAccessor) : ICoursesApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task<HttpRequestMessage> CreateRequestAsync(
        HttpMethod method,
        string uri,
        CancellationToken ct)
    {
        var token = await tokenAccessor.GetAccessTokenAsync(ct);
        var request = new HttpRequestMessage(method, uri);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<HttpRequestMessage> CreateJsonRequestAsync<T>(
        HttpMethod method,
        string uri,
        T body,
        CancellationToken ct)
    {
        var request = await CreateRequestAsync(method, uri, ct);
        request.Content = JsonContent.Create(body, options: JsonOptions);
        return request;
    }

    // Courses
    public async Task<PagedResult<CourseListItem>> GetCoursesAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, $"/api/courses?page={page}&pageSize={pageSize}", ct);
        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<CourseListItem>>(JsonOptions, ct) ?? [];
        var totalCount = GetTotalCountHeader(response);
        return new PagedResult<CourseListItem>(items, totalCount ?? items.Count, page, pageSize);
    }

    public async Task<CourseDetailResponse?> GetCourseByIdAsync(string courseId, CancellationToken ct = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, $"/api/courses/{courseId}", ct);
        var response = await httpClient.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CourseDetailResponse>(JsonOptions, ct);
    }

    public async Task<CreateCourseResponse?> CreateCourseAsync(CreateCourseRequest body, CancellationToken ct = default)
    {
        var request = await CreateJsonRequestAsync(HttpMethod.Post, "/api/courses", body, ct);
        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateCourseResponse>(JsonOptions, ct);
    }

    public async Task UpdateCourseAsync(string courseId, UpdateCourseRequest body, CancellationToken ct = default)
    {
        var request = await CreateJsonRequestAsync(HttpMethod.Put, $"/api/courses/{courseId}", body, ct);
        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteCourseAsync(string courseId, CancellationToken ct = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Delete, $"/api/courses/{courseId}", ct);
        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    // Modules
    public async Task<List<ModuleListItem>> GetModulesAsync(string courseId, CancellationToken ct = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, $"/api/courses/{courseId}/modules", ct);
        var response = await httpClient.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return [];
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ModuleListItem>>(JsonOptions, ct) ?? [];
    }

    public async Task<ModuleDetailResponse?> GetModuleByIdAsync(string courseId, string moduleId, CancellationToken ct = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, $"/api/courses/{courseId}/modules/{moduleId}", ct);
        var response = await httpClient.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ModuleDetailResponse>(JsonOptions, ct);
    }

    public async Task<AddModuleResponse?> AddModuleAsync(string courseId, AddModuleRequest body, CancellationToken ct = default)
    {
        var request = await CreateJsonRequestAsync(HttpMethod.Post, $"/api/courses/{courseId}/modules", body, ct);
        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AddModuleResponse>(JsonOptions, ct);
    }

    public async Task UpdateModuleAsync(string courseId, string moduleId, UpdateModuleRequest body, CancellationToken ct = default)
    {
        var request = await CreateJsonRequestAsync(HttpMethod.Put, $"/api/courses/{courseId}/modules/{moduleId}", body, ct);
        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteModuleAsync(string courseId, string moduleId, CancellationToken ct = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Delete, $"/api/courses/{courseId}/modules/{moduleId}", ct);
        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    // Lessons
    public async Task<List<LessonListItem>> GetLessonsAsync(string courseId, string moduleId, CancellationToken ct = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, $"/api/courses/{courseId}/modules/{moduleId}/lessons", ct);
        var response = await httpClient.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return [];
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<LessonListItem>>(JsonOptions, ct) ?? [];
    }

    public async Task<LessonDetailResponse?> GetLessonByIdAsync(string courseId, string moduleId, string lessonId, CancellationToken ct = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, $"/api/courses/{courseId}/modules/{moduleId}/lessons/{lessonId}", ct);
        var response = await httpClient.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LessonDetailResponse>(JsonOptions, ct);
    }

    public async Task<AddLessonResponse?> AddLessonAsync(string courseId, string moduleId, AddLessonRequest body, CancellationToken ct = default)
    {
        var request = await CreateJsonRequestAsync(HttpMethod.Post, $"/api/courses/{courseId}/modules/{moduleId}/lessons", body, ct);
        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AddLessonResponse>(JsonOptions, ct);
    }

    public async Task UpdateLessonAsync(string courseId, string moduleId, string lessonId, UpdateLessonRequest body, CancellationToken ct = default)
    {
        var request = await CreateJsonRequestAsync(HttpMethod.Put, $"/api/courses/{courseId}/modules/{moduleId}/lessons/{lessonId}", body, ct);
        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteLessonAsync(string courseId, string moduleId, string lessonId, CancellationToken ct = default)
    {
        var request = await CreateRequestAsync(HttpMethod.Delete, $"/api/courses/{courseId}/modules/{moduleId}/lessons/{lessonId}", ct);
        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    private static int? GetTotalCountHeader(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("X-Total-Count", out var values) &&
            int.TryParse(values.FirstOrDefault(), out var count))
            return count;
        return null;
    }
}
