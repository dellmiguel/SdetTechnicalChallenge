using System.Net;
using System.Text.Json;
using NUnit.Framework;
using UserManagementApi.Tests.Clients;
using UserManagementApi.Tests.Models;
using UserManagementApi.Tests.Helpers;

namespace UserManagementApi.Tests.Tests;

[TestFixture]
public class GetUserTests
{
    private UserApiClient _apiClient = null!;

    [SetUp]
    public void Setup()
    {
        _apiClient = new UserApiClient(TestConfiguration.ApiUrl);
    }

    [Test]
    public async Task GetUser_WithExistingEmail_ShouldReturn200AndUser()
    {
        // Arrange
        var user = new User
        {
            Name = "Get User Test",
            Email = $"get.user.{Guid.NewGuid()}@example.com",
            Age = 30
        };
        var createResponse = await _apiClient.CreateUserAsync(user);

        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Precondition failed: test user could not be created.");

        // Act
        var response = await _apiClient.GetUserAsync(user.Email);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var returnedUser = JsonSerializer.Deserialize<User>(response.Content!,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        Assert.Multiple(() =>
        {
            Assert.That(returnedUser, Is.Not.Null);
            Assert.That(returnedUser!.Name, Is.EqualTo(user.Name));
            Assert.That(returnedUser.Email, Is.EqualTo(user.Email));
            Assert.That(returnedUser.Age, Is.EqualTo(user.Age));
        });
    }

    [Test]
    public async Task GetUser_WithNonExistingEmail_ShouldReturn404AndErrorResponse()
    {
        // Arrange
        var nonExistingEmail = $"non.existing.{Guid.NewGuid()}@example.com";
        // Act
        var response = await _apiClient.GetUserAsync(nonExistingEmail);
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(response.Content!,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        Assert.Multiple(() =>
        {
            Assert.That(errorResponse, Is.Not.Null);
            Assert.That(errorResponse!.Error, Is.Not.Null.And.Not.Empty);
        });
    }
}