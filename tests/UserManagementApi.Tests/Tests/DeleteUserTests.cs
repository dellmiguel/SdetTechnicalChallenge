using System.Net;
using NUnit.Framework;
using UserManagementApi.Tests.Clients;
using UserManagementApi.Tests.Helpers;
using UserManagementApi.Tests.Models;

namespace UserManagementApi.Tests.Tests;

[TestFixture]
public class DeleteUserTests
{
    private UserApiClient _apiClient = null!;

    [SetUp]
    public void Setup()
    {
        _apiClient = new UserApiClient(TestConfiguration.ApiUrl);
    }

    [Test]
    public async Task DeleteUser_WithValidToken_ShouldReturn204()
    {
        // Arrange
        var user = new User
        {
            Name = "Delete User Test",
            Email = $"delete.{Guid.NewGuid()}@example.com",
            Age = 30
        };
        var createResponse = await _apiClient.CreateUserAsync(user);
        Assert.That(
            createResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.Created),
            "Precondition failed: user could not be created.");
        // Act
        var response = await _apiClient.DeleteUserAsync(user.Email, TestConfiguration.AuthToken);
        // Assert
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent));

        // Verify that the user no longer exists
        var getUsersResponse = await _apiClient.GetUsersAsync();

        Assert.That(getUsersResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var users =
            System.Text.Json.JsonSerializer.Deserialize<List<User>>(
                getUsersResponse.Content!,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        Assert.That(users, Is.Not.Null);

        Assert.That(
            users!.Any(u => u.Email == user.Email),
            Is.False,
            "The deleted user is still present in the database.");
    }

    [Test]
    public async Task DeleteUser_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var user = new User
        {
            Name = "Delete Without Test",
            Email = $"delete.no.token{Guid.NewGuid()}@example.com",
            Age = 30
        };
        var createResponse = await _apiClient.CreateUserAsync(user);
        Assert.That(
            createResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.Created),
            "Precondition failed: user could not be created.");
        // Act
        var response = await _apiClient.DeleteUserAsync(user.Email);

        // Assert
        var getUsersResponse = await _apiClient.GetUsersAsync();

        Assert.That(getUsersResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var users =
            System.Text.Json.JsonSerializer.Deserialize<List<User>>(
                getUsersResponse.Content!,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        Assert.That(users, Is.Not.Null);

        bool userStillExists = users!.Any(u => u.Email == user.Email);

        Assert.Multiple(() =>
        {
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Unauthorized),
                "DELETE without authentication must return 401.");

            Assert.That(
                userStillExists,
                Is.True,
                "Security issue: DELETE removed the user without authentication.");
        });
    }

    [Test]
    public async Task DeleteUser_WithInvalidToken_ShouldReturn401()
    {
        // Arrange
        var user = new User
        {
            Name = "Delete Invalid Token",
            Email = $"delete.invalid.token.{Guid.NewGuid()}@example.com",
            Age = 30
        };

        var createResponse = await _apiClient.CreateUserAsync(user);
        Assert.That(
            createResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.Created),
            "Precondition failed: user could not be created.");
        // Act
        var response = await _apiClient.DeleteUserAsync(user.Email, "invalid-token");
        // Assert
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DeleteUser_WithNonExistingEmail_ShouldReturn404()
    {
        // Arrange
        var nonExistingEmail = $"delete.non.existing.{Guid.NewGuid()}@example.com";
        // Act
        var response = await _apiClient.DeleteUserAsync(nonExistingEmail, TestConfiguration.AuthToken);
        // Assert
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound));
    }
}