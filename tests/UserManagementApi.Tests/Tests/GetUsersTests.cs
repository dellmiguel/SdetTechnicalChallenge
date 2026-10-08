using System.Net;
using System.Text.Json;
using NUnit.Framework;
using UserManagementApi.Tests.Clients;
using UserManagementApi.Tests.Models;
using UserManagementApi.Tests.Helpers;

namespace UserManagementApi.Tests.Tests;

[TestFixture]
public class GetUsersTests
{
    private UserApiClient _apiClient = null!;

    [SetUp]
    public void Setup()
    {
        _apiClient = new UserApiClient(TestConfiguration.ApiUrl);
    }

    [Test]
    public async Task GetUsers_ShouldReturn200AndUserArray()
    {
        var response = await _apiClient.GetUsersAsync();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null);
        });

        var users = JsonSerializer.Deserialize<List<User>>(response.Content!, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(users, Is.Not.Null);
    }
}