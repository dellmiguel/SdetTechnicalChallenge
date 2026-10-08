using System.Net;
using System.Text.Json;
using NUnit.Framework;
using UserManagementApi.Tests.Clients;
using UserManagementApi.Tests.Models;
using UserManagementApi.Tests.Helpers;

namespace UserManagementApi.Tests.Tests;

[TestFixture]
public class CreateUserTests
{
    private UserApiClient _apiClient = null!;

    [SetUp]
    public void Setup()
    {
        _apiClient = new UserApiClient(TestConfiguration.ApiUrl);
    }

    [Test]
    public async Task CreateUser_WithValidData_ShouldReturn201()
    {
        var user = new User
        {
            Name = "Miguel Test",
            Email = $"miguel.{Guid.NewGuid()}@example.com",
            Age = 30
        };
        var response = await _apiClient.CreateUserAsync(user);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdUser = JsonSerializer.Deserialize<User>(response.Content!, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.Multiple(() =>
        {
            Assert.That(createdUser, Is.Not.Null);
            Assert.That(createdUser!.Name, Is.EqualTo(user.Name));
            Assert.That(createdUser.Email, Is.EqualTo(user.Email));
            Assert.That(createdUser.Age, Is.EqualTo(user.Age));
        });
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(151)]
    public async Task CreateUser_WithInvalidAge_ShouldReturn400(int invalidAge)
    {
        var user = new User
        {
            Name = "Invalid Age User",
            Email = $"invalid.age.{Guid.NewGuid()}@example.com",
            Age = invalidAge
        };

        var response = await _apiClient.CreateUserAsync(user);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("invalid-email")]
    [TestCase("missing-at-sign.com")]
    public async Task CreateUser_WithInvalidEmail_ShouldReturn400(string invalidEmail)
    {
        var uniqueInvalidEmail = $"{Guid.NewGuid()}.{invalidEmail}";
        var user = new User
        {
            Name = "Invalid Email User",
            Email = uniqueInvalidEmail,
            Age = 30
        };

        var response = await _apiClient.CreateUserAsync(user);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase(1)]
    [TestCase(150)]
    public async Task CreateUser_WithValidBoundaryAge_ShouldReturn201(int validAge)
    {
        var user = new User
        {
            Name = "Boundary Age User",
            Email = $"boundary.{validAge}.{Guid.NewGuid()}@example.com",
            Age = validAge
        };

        var response = await _apiClient.CreateUserAsync(user);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task CreateUser_WithDuplicateEmail_ShouldReturn409()
    {
        var user = new User
        {
            Name = "Duplicate User",
            Email = $"duplicate.{Guid.NewGuid()}@example.com",
            Age = 30
        };
        // Arrange:
        // Create the user for the first time.
        var firstResponse = await _apiClient.CreateUserAsync(user);
        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Precondition failed: the initial user was not created.");
        // Act:
        // Try to create another user with the same email.
        var duplicateResponse = await _apiClient.CreateUserAsync(user);
        // Assert:
        Assert.That(duplicateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task CreateUser_WithoutName_ShouldReturn400()
    {
        var body = new
        {
            email = $"missing.name.{Guid.NewGuid()}@example.com",
            age = 30
        };

        var response = await _apiClient.CreateUserAsync(body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateUser_WithoutEmail_ShouldReturn400()
    {
        var body = new
        {
            name = "Missing Email User",
            age = 30
        };

        var response = await _apiClient.CreateUserAsync(body);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateUser_WithoutAge_ShouldReturn400()
    {
        var body = new
        {
            name = "Missing Age User",
            email = $"missing.age.{Guid.NewGuid()}@example.com"
        };

        var response = await _apiClient.CreateUserAsync(body);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}