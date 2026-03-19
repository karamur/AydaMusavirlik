using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AydaMusavirlik.Desktop.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthTokenStore _tokenStore;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, AuthTokenStore tokenStore)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private void SetAuthHeader()
    {
        if (_tokenStore.IsAuthenticated)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _tokenStore.Token);
        }
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync(endpoint);
            return await ProcessResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Fail($"Baglanti hatasi: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
            return await ProcessResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Fail($"Baglanti hatasi: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);
            return await ProcessResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Fail($"Baglanti hatasi: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync(endpoint);

            if (response.IsSuccessStatusCode)
                return ApiResponse<bool>.Ok(true);

            var error = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail(error);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Baglanti hatasi: {ex.Message}");
        }
    }

    private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrEmpty(content))
                return ApiResponse<T>.Ok(default!);

            var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
            return ApiResponse<T>.Ok(data!);
        }

        return ApiResponse<T>.Fail(content);
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(string error) => new() { Success = false, Error = error };
}