using System.Net;
using System.Text.Json;
using NUnit.Framework;
using UserManagementApi.Tests.Clients;
using UserManagementApi.Tests.Models;
using UserManagementApi.Tests.Helpers;

namespace UserManagementApi.Tests.Tests;

[TestFixture]
public class UpdateUserTests
{
    private UserApiClient _apiClient = null!;

    [SetUp]
    public void Setup()
    {
        _apiClient = new UserApiClient(TestConfiguration.ApiUrl);
    }

    [Test]
    public async Task UpdateUser_WithValidData_ShouldReturn200AndUpdatedUser()
    {
        var originalUser = new User
        {
            Name = "Original User",
            Email = $"update.{Guid.NewGuid()}@example.com",
            Age = 30
        };

        var createResponse = await _apiClient.CreateUserAsync(originalUser);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Precondition failed: test user could not be created.");
        
        var updatedUser = new User
        {
            Name = "Updated User",
            Email = originalUser.Email,
            Age = 35
        };
        var response = await _apiClient.UpdateUserAsync(originalUser.Email, updatedUser);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var returnedUser = JsonSerializer.Deserialize<User>(response.Content!, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.Multiple(() =>
        {
            Assert.That(returnedUser, Is.Not.Null);
            Assert.That(returnedUser!.Name, Is.EqualTo(updatedUser.Name));
            Assert.That(returnedUser.Email, Is.EqualTo(updatedUser.Email));
            Assert.That(returnedUser.Age, Is.EqualTo(updatedUser.Age));
        });

        // Verify that the update was persisted
        var getResponse = await _apiClient.GetUserAsync(updatedUser.Email);
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var persistedUser = JsonSerializer.Deserialize<User>(getResponse.Content!, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.Multiple(() =>
        {
            Assert.That(persistedUser, Is.Not.Null);
            Assert.That(persistedUser!.Name, Is.EqualTo(updatedUser.Name));
            Assert.That(persistedUser.Email, Is.EqualTo(updatedUser.Email));
            Assert.That(persistedUser.Age, Is.EqualTo(updatedUser.Age));
        });
    }

    [Test]
    public async Task UpdateUser_WithNonExistingEmail_ShouldReturn404()
    {
        // Arrange
        var nonExistingEmail = $"non.existing.update.{Guid.NewGuid()}@example.com";
        var updatedUser = new User
        {
            Name = "Non Existing User",
            Email = nonExistingEmail,
            Age = 40
        };
        // Act
        var response = await _apiClient.UpdateUserAsync(nonExistingEmail, updatedUser);
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(151)]
    public async Task UpdateUser_WithInvalidAge_ShouldReturn400(int invalidAge)
    {
        // Arrange
        var originalUser = new User
        {
            Name = "Update Invalid Age",
            Email = $"update.invalid.age.{Guid.NewGuid()}@example.com",
            Age = 30
        };

        var createResponse = await _apiClient.CreateUserAsync(originalUser);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Precondition failed: test user could not be created.");
        var updatedUser = new User
        {
            Name = originalUser.Name,
            Email = originalUser.Email,
            Age = invalidAge
        };
        // Act
        var response = await _apiClient.UpdateUserAsync(originalUser.Email, updatedUser);
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("invalid-email")]
    [TestCase("missing-at-sign.com")]
    public async Task UpdateUser_WithInvalidEmail_ShouldReturn400(string invalidEmail)
    {
        // Arrange
        var originalUser = new User
        {
            Name = "Update Invalid Email",
            Email = $"update.email.{Guid.NewGuid()}@example.com",
            Age = 30
        };
        var createResponse = await _apiClient.CreateUserAsync(originalUser);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Precondition failed: test user could not be created.");
        var uniqueInvalidEmail = $"{Guid.NewGuid()}.{invalidEmail}";
        var updatedUser = new User
        {
            Name = originalUser.Name,
            Email = uniqueInvalidEmail,
            Age = originalUser.Age
        };
        // Act
        var response = await _apiClient.UpdateUserAsync(originalUser.Email, updatedUser);
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UpdateUser_WithDuplicateEmail_ShouldReturn409()
    {
        // Arrange
        var firstUser = new User
        {
            Name = "First User",
            Email = $"first.{Guid.NewGuid()}@example.com",
            Age = 30
        };
        var secondUser = new User
        {
            Name = "Second User",
            Email = $"second.{Guid.NewGuid()}@example.com",
            Age = 35
        };
        var firstCreateResponse = await _apiClient.CreateUserAsync(firstUser);
        var secondCreateResponse = await _apiClient.CreateUserAsync(secondUser);
        Assert.Multiple(() =>
        {
            Assert.That(firstCreateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Precondition failed: first user could not be created.");
            Assert.That(secondCreateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Precondition failed: second user could not be created.");
        });

        var updatedUser = new User
        {
            Name = secondUser.Name,
            Email = firstUser.Email,
            Age = secondUser.Age
        };
        // Act
        var response = await _apiClient.UpdateUserAsync(secondUser.Email, updatedUser);
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}