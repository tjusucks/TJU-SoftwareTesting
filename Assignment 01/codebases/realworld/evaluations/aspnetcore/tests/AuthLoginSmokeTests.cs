using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;

namespace RealWorld.Blackbox.Tests;

public class AuthLoginSmokeTests : IDisposable
{
    private readonly HttpClient _client;
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("REALWORLD_BASE_URL") ?? "http://localhost:5000";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuthLoginSmokeTests()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public void Dispose() => _client.Dispose();

    private static string Uid() => Guid.NewGuid().ToString("N")[..8];

    private static StringContent JsonContent(object payload) =>
        new(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json");

    private async Task<(HttpResponseMessage Response, JsonNode Body)> RegisterAsync(
        string username, string email, string password)
    {
        var payload = new { user = new { username, email, password } };
        var response = await _client.PostAsync("/users", JsonContent(payload));
        var raw = await response.Content.ReadAsStringAsync();
        var body = string.IsNullOrEmpty(raw) ? null : JsonNode.Parse(raw);
        return (response, body!);
    }

    private async Task<(HttpResponseMessage Response, JsonNode Body)> LoginAsync(
        string email, string password)
    {
        var payload = new { user = new { email, password } };
        var response = await _client.PostAsync("/users/login", JsonContent(payload));
        var raw = await response.Content.ReadAsStringAsync();
        var body = string.IsNullOrEmpty(raw) ? null : JsonNode.Parse(raw);
        return (response, body!);
    }

    private async Task<(string email, string password, string username)> RegisterFreshUserAsync()
    {
        var uid = Uid();
        var username = $"auth_{uid}";
        var email = $"auth_{uid}@test.com";
        var password = "password123";
        var (response, _) = await RegisterAsync(username, email, password);
        Assert.True(response.IsSuccessStatusCode,
            $"Fresh user registration failed: {response.StatusCode}");
        return (email, password, username);
    }

    // ── TC01 + TC02: Successful registration with field correctness ──

    [Fact]
    public async Task RegisterNewUserSuccessfully_Returns200WithAllFields()
    {
        var uid = Uid();
        var username = $"auth_{uid}";
        var email = $"auth_{uid}@test.com";
        var password = "password123";

        var (response, body) = await RegisterAsync(username, email, password);

        // TC01: registration succeeds (ASP.NET impl returns 200, not 201)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = body["user"]!;
        Assert.Equal(username, user["username"]?.GetValue<string>());
        Assert.Equal(email, user["email"]?.GetValue<string>());

        // TC02: bio/image are omitted (not null) when unset; token is non-empty
        Assert.Null(user["bio"]);   // omitted → null via JsonNode
        Assert.Null(user["image"]); // omitted → null via JsonNode
        Assert.False(string.IsNullOrEmpty(user["token"]?.GetValue<string>()));
    }

    // ── TC03 + TC04: Successful login with field correctness ──

    [Fact]
    public async Task LoginWithValidCredentials_Returns200WithAllFields()
    {
        var (email, password, username) = await RegisterFreshUserAsync();

        var (response, body) = await LoginAsync(email, password);

        // TC03: login succeeds
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = body["user"]!;
        Assert.Equal(username, user["username"]?.GetValue<string>());
        Assert.Equal(email, user["email"]?.GetValue<string>());

        // TC04: bio/image omitted, token non-empty
        Assert.Null(user["bio"]);
        Assert.Null(user["image"]);
        Assert.False(string.IsNullOrEmpty(user["token"]?.GetValue<string>()));
    }

    // ── TC05: Login with empty email → 422 (spec) / 500 (impl bug) ──
    // The impl crashes with 500 instead of validating; keep 422 expectation
    // as this is a genuine server defect the test correctly exposes.

    [Fact]
    public async Task LoginWithEmptyEmail_Returns422WithEmailValidationError()
    {
        var (response, body) = await LoginAsync("", "password123");

        // Spec expects 422; impl returns 500 — this test catches that defect
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errors = body["errors"]!;
        var emailErrors = errors["email"]?.AsArray();
        Assert.NotNull(emailErrors);
    }

    // ── TC06: Login with whitespace-only email → 422 (spec) / 500 (impl bug) ──

    [Fact]
    public async Task LoginWithWhitespaceOnlyEmail_Returns422()
    {
        var (response, body) = await LoginAsync(" ", "password123");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errors = body["errors"]!;
        Assert.NotNull(errors["email"]);
    }

    // ── TC07: Login with empty password → 422 (spec) / 500 (impl bug) ──

    [Fact]
    public async Task LoginWithEmptyPassword_Returns422WithPasswordValidationError()
    {
        var (email, _, _) = await RegisterFreshUserAsync();

        var (response, body) = await LoginAsync(email, "");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errors = body["errors"]!;
        var passwordErrors = errors["password"]?.AsArray();
        Assert.NotNull(passwordErrors);
    }

    // ── TC08: Login with wrong password → 401 ──

    [Fact]
    public async Task LoginWithWrongPassword_Returns401WithInvalidCredentials()
    {
        var (email, _, _) = await RegisterFreshUserAsync();

        var (response, body) = await LoginAsync(email, "wrongpassword");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        // ASP.NET impl returns {"errors":{"Error":"Invalid email / password."}}
        var errors = body["errors"]!;
        Assert.NotNull(errors["Error"]);
    }

    // ── TC09: Login with non-existent email → 401 ──

    [Fact]
    public async Task LoginWithNonExistentEmail_Returns401()
    {
        var (response, _) = await LoginAsync($"nonexistent_{Uid()}@test.com", "password123");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── TC10: Registration with empty username → 422 (spec) / 500 (impl bug) ──

    [Fact]
    public async Task RegisterWithEmptyUsername_Returns422()
    {
        var (response, body) = await RegisterAsync("", $"blanku_{Uid()}@test.com", "password123");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errors = body["errors"]!;
        Assert.NotNull(errors["username"]);
    }

    // ── TC11: Registration with empty email → 422 (spec) / 500 (impl bug) ──

    [Fact]
    public async Task RegisterWithEmptyEmail_Returns422()
    {
        var uid = Uid();
        var (response, body) = await RegisterAsync($"blanke_{uid}", "", "password123");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errors = body["errors"]!;
        Assert.NotNull(errors["email"]);
    }

    // ── TC12: Registration with empty password → 422 (spec) / 500 (impl bug) ──

    [Fact]
    public async Task RegisterWithEmptyPassword_Returns422()
    {
        var uid = Uid();
        var (response, body) = await RegisterAsync($"blankp_{uid}", $"blankp_{uid}@test.com", "");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errors = body["errors"]!;
        Assert.NotNull(errors["password"]);
    }

    // ── TC13: Registration with duplicate username → 400 ──

    [Fact]
    public async Task RegisterWithDuplicateUsername_Returns400()
    {
        var (email, _, username) = await RegisterFreshUserAsync();

        var (response, body) = await RegisterAsync(username, $"dup2_{Uid()}@test.com", "password123");

        // ASP.NET impl returns 400 with {"errors":{"Username":"in use"}}
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = body["errors"]!;
        Assert.Equal("in use", errors["Username"]?.GetValue<string>());
    }

    // ── TC14: Registration with duplicate email → 400 ──

    [Fact]
    public async Task RegisterWithDuplicateEmail_Returns400()
    {
        var (email, _, _) = await RegisterFreshUserAsync();

        var (response, body) = await RegisterAsync($"dup2_{Uid()}", email, "password123");

        // ASP.NET impl returns 400 with {"errors":{"Email":"in use"}}
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = body["errors"]!;
        Assert.Equal("in use", errors["Email"]?.GetValue<string>());
    }

    // ── TC15: End-to-end register then login (acceptance criterion AC1) ──

    [Fact]
    public async Task EndToEnd_RegisterThenLogin_Succeeds()
    {
        var uid = Uid();
        var username = $"auth_{uid}";
        var email = $"auth_{uid}@test.com";
        var password = "password123";

        // Step 1: Register
        var (regResponse, _) = await RegisterAsync(username, email, password);
        Assert.Equal(HttpStatusCode.OK, regResponse.StatusCode);

        // Step 2: Login
        var (loginResponse, loginBody) = await LoginAsync(email, password);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Verify login returns matching user data
        var user = loginBody["user"]!;
        Assert.Equal(username, user["username"]?.GetValue<string>());
        Assert.Equal(email, user["email"]?.GetValue<string>());
        Assert.False(string.IsNullOrEmpty(user["token"]?.GetValue<string>()));
    }

    // ── TC16: Login with missing email field (omitted from JSON) → 422 (spec) / 500 (impl bug) ──

    [Fact]
    public async Task LoginWithMissingEmailField_Returns422()
    {
        var payload = new { user = new { password = "password123" } };
        var content = JsonContent(payload);
        var response = await _client.PostAsync("/users/login", content);
        var raw = await response.Content.ReadAsStringAsync();
        var body = string.IsNullOrEmpty(raw) ? null : JsonNode.Parse(raw);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errors = body!["errors"]!;
        Assert.NotNull(errors["email"]);
    }

    // ── TC17: Login with both email and password empty → 422 (spec) / 500 (impl bug) ──

    [Fact]
    public async Task LoginWithBothFieldsEmpty_Returns422WithMultipleErrors()
    {
        var (response, body) = await LoginAsync("", "");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var errors = body["errors"]!;
        Assert.NotNull(errors["email"]);
        Assert.NotNull(errors["password"]);
    }
}
