namespace UserManagementApi.Tests.Helpers;

public static class TestConfiguration
{
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL")
        ?? "http://localhost:3000";
    public static string EnvironmentName =>
        Environment.GetEnvironmentVariable("TEST_ENV")
        ?? "dev";
    public static string ApiUrl =>
        $"{BaseUrl.TrimEnd('/')}/{EnvironmentName.Trim('/')}/";
    public static string AuthToken =>
        Environment.GetEnvironmentVariable("AUTH_TOKEN")
        ?? "mysecrettoken";
}