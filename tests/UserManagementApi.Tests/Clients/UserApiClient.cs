using RestSharp;
using UserManagementApi.Tests.Models;

namespace UserManagementApi.Tests.Clients;

public class UserApiClient
{
    private readonly RestClient _client;

    public UserApiClient(string baseUrl)
    {
        _client = new RestClient(baseUrl);
    }

    public async Task<RestResponse> GetUsersAsync()
    {
        var request = new RestRequest("users", Method.Get);
        return await _client.ExecuteAsync(request);
    }

    public async Task<RestResponse> GetUserAsync(string email)
    {
        var request = new RestRequest($"users/{email}", Method.Get);
        return await _client.ExecuteAsync(request);
    }

    public async Task<RestResponse> CreateUserAsync(User user)
    {
        var request = new RestRequest("users", Method.Post);
        request.AddJsonBody(user);
        return await _client.ExecuteAsync(request);
    }

    public async Task<RestResponse> UpdateUserAsync(string email, User user)
    {
        var request = new RestRequest($"users/{email}", Method.Put);
        request.AddJsonBody(user);
        return await _client.ExecuteAsync(request);
    }

    public async Task<RestResponse> DeleteUserAsync(string email, string? token = null)
    {
        var request = new RestRequest($"users/{email}", Method.Delete);
        if (!string.IsNullOrEmpty(token))
        {
            request.AddHeader("Authentication", token);
        }
        return await _client.ExecuteAsync(request);
    }

    public async Task<RestResponse> CreateUserAsync(object body)
    {
        var request = new RestRequest("users", Method.Post);
        request.AddJsonBody(body);
        return await _client.ExecuteAsync(request);
    }
}