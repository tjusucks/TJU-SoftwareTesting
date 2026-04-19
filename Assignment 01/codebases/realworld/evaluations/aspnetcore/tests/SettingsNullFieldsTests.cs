using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;

namespace RealWorld.Blackbox.Tests;

public class SettingsNullFieldsTests : IDisposable
{
    private readonly HttpClient _client;
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("REALWORLD_BASE_URL") ?? "http://localhost:5000";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _token;
    private readonly string _username;
    private readonly string _email;

    public SettingsNullFieldsTests()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        (_token, _username, _email) = RegisterFreshUserAsync().GetAwaiter().GetResult();
    }

    public void Dispose() => _client.Dispose();

    private static string Uid() => Guid.NewGuid().ToString("N")[..8];

    private static StringContent JsonContent(object payload) =>
        new(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json");

    private async Task<(string token, string username, string email)> RegisterFreshUserAsync()
    {
        var uid = Uid();
        var username = $"snf_{uid}";
        var email = $"snf_{uid}@test.com";
        var password = "password123";

        var payload = new { user = new { username, email, password } };
        var response = await _client.PostAsync("/users", JsonContent(payload));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode,
            $"Fresh user registration failed: {response.StatusCode} - {raw}");

        var body = JsonNode.Parse(raw)!;
        var token = body["user"]!["token"]!.GetValue<string>();
        return (token, username, email);
    }

    private HttpRequestMessage AuthedRequest(HttpMethod method, string url, object? payload = null)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _token);
        if (payload != null)
            req.Content = JsonContent(payload);
        return req;
    }

    private async Task<(HttpResponseMessage Response, JsonNode Body)> PutUserAsync(object userFields)
    {
        var payload = new { user = userFields };
        using var req = AuthedRequest(HttpMethod.Put, "/user", payload);
        var response = await _client.SendAsync(req);
        var raw = await response.Content.ReadAsStringAsync();
        var body = string.IsNullOrEmpty(raw) ? new JsonObject() : JsonNode.Parse(raw)!;
        return (response, body);
    }

    private async Task<(HttpResponseMessage Response, JsonNode Body)> GetCurrentUserAsync()
    {
        using var req = AuthedRequest(HttpMethod.Get, "/user");
        var response = await _client.SendAsync(req);
        var raw = await response.Content.ReadAsStringAsync();
        var body = string.IsNullOrEmpty(raw) ? new JsonObject() : JsonNode.Parse(raw)!;
        return (response, body);
    }

    // ── TC01: Set bio to a valid non-empty string ──

    [Fact]
    public async Task BioSetToValidString_Returns200WithBioValue()
    {
        var (response, body) = await PutUserAsync(new { bio = "Hello world" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Hello world", body["user"]!["bio"]?.GetValue<string>());
    }

    // ── TC02: Set bio to empty string — normalizes to null ──

    [Fact]
    public async Task BioEmptyString_NormalizesToNull()
    {
        var (response, body) = await PutUserAsync(new { bio = "" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(body["user"]!["bio"]);
    }

    // ── TC03: Set bio to explicit null — accepted ──

    [Fact]
    public async Task BioExplicitNull_AcceptedAndReturnsNull()
    {
        // Use raw JSON to send explicit null
        var json = @"{""user"":{""bio"":null}}";
        using var req = AuthedRequest(HttpMethod.Put, "/user");
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(req);
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(body["user"]!["bio"]);
    }

    // ── TC04: Set image to a valid URL ──

    [Fact]
    public async Task ImageSetToValidUrl_Returns200WithImageUrl()
    {
        var (response, body) = await PutUserAsync(new { image = "https://example.com/photo.jpg" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("https://example.com/photo.jpg", body["user"]!["image"]?.GetValue<string>());
    }

    // ── TC05: Set image to empty string — normalizes to null ──

    [Fact]
    public async Task ImageEmptyString_NormalizesToNull()
    {
        // First set a valid image, then normalize
        await PutUserAsync(new { image = "https://example.com/photo.jpg" });

        var (response, body) = await PutUserAsync(new { image = "" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(body["user"]!["image"]);
    }

    // ── TC06: Set image to explicit null — accepted ──

    [Fact]
    public async Task ImageExplicitNull_AcceptedAndReturnsNull()
    {
        // First set a valid image, then null it
        await PutUserAsync(new { image = "https://example.com/photo.jpg" });

        var json = @"{""user"":{""image"":null}}";
        using var req = AuthedRequest(HttpMethod.Put, "/user");
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(req);
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(body["user"]!["image"]);
    }

    // ── TC07: Null bio (via empty string) persists across GET ──

    [Fact]
    public async Task BioNormalizedToNull_PersistsAcrossGet()
    {
        await PutUserAsync(new { bio = "" });

        var (getResponse, getBody) = await GetCurrentUserAsync();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Null(getBody["user"]!["bio"]);
    }

    // ── TC08: Null bio (via explicit null) persists across GET ──

    [Fact]
    public async Task BioExplicitNull_PersistsAcrossGet()
    {
        var json = @"{""user"":{""bio"":null}}";
        using var putReq = AuthedRequest(HttpMethod.Put, "/user");
        putReq.Content = new StringContent(json, Encoding.UTF8, "application/json");
        await _client.SendAsync(putReq);

        var (getResponse, getBody) = await GetCurrentUserAsync();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Null(getBody["user"]!["bio"]);
    }

    // ── TC09: Null image (via empty string) persists across GET ──

    [Fact]
    public async Task ImageNormalizedToNull_PersistsAcrossGet()
    {
        await PutUserAsync(new { image = "" });

        var (getResponse, getBody) = await GetCurrentUserAsync();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Null(getBody["user"]!["image"]);
    }

    // ── TC10: Null image (via explicit null) persists across GET ──

    [Fact]
    public async Task ImageExplicitNull_PersistsAcrossGet()
    {
        var json = @"{""user"":{""image"":null}}";
        using var putReq = AuthedRequest(HttpMethod.Put, "/user");
        putReq.Content = new StringContent(json, Encoding.UTF8, "application/json");
        await _client.SendAsync(putReq);

        var (getResponse, getBody) = await GetCurrentUserAsync();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Null(getBody["user"]!["image"]);
    }

    // ── TC11: Email empty string — rejected with 422 ──

    [Fact]
    public async Task EmailEmptyString_RejectedWith422()
    {
        var (response, body) = await PutUserAsync(new { email = "" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(body["errors"]);
    }

    // ── TC12: Username empty string — rejected with 422 ──

    [Fact]
    public async Task UsernameEmptyString_RejectedWith422()
    {
        var (response, body) = await PutUserAsync(new { username = "" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(body["errors"]);
    }

    // ── TC13: Email null — rejected with 422 ──

    [Fact]
    public async Task EmailNull_RejectedWith422()
    {
        var json = @"{""user"":{""email"":null}}";
        using var req = AuthedRequest(HttpMethod.Put, "/user");
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(req);
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(body["errors"]);
    }

    // ── TC14: Username null — rejected with 422 ──

    [Fact]
    public async Task UsernameNull_RejectedWith422()
    {
        var json = @"{""user"":{""username"":null}}";
        using var req = AuthedRequest(HttpMethod.Put, "/user");
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(req);
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(body["errors"]);
    }

    // ── TC15: Bio whitespace-only — not normalized to null ──

    [Fact]
    public async Task BioWhitespaceOnly_NotNormalizedToNull()
    {
        var (response, body) = await PutUserAsync(new { bio = " " });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Whitespace-only should NOT be normalized to null per spec (only empty string is)
        Assert.Equal(" ", body["user"]!["bio"]?.GetValue<string>());
    }

    // ── TC16: Image whitespace-only — not normalized to null ──

    [Fact]
    public async Task ImageWhitespaceOnly_NotNormalizedToNull()
    {
        var (response, body) = await PutUserAsync(new { image = " " });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Whitespace-only should NOT be normalized to null per spec (only empty string is)
        Assert.Equal(" ", body["user"]!["image"]?.GetValue<string>());
    }

    // ── TC17: Full bio lifecycle: set → normalize → null → restore ──

    [Fact]
    public async Task BioLifecycle_SetThenNormalizeThenNullThenRestore()
    {
        // Step 1: Set bio to a value
        var (r1, b1) = await PutUserAsync(new { bio = "Test bio" });
        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);
        Assert.Equal("Test bio", b1["user"]!["bio"]?.GetValue<string>());

        // Step 2: Set bio to empty string — normalizes to null
        var (r2, b2) = await PutUserAsync(new { bio = "" });
        Assert.Equal(HttpStatusCode.OK, r2.StatusCode);
        Assert.Null(b2["user"]!["bio"]);

        // Step 3: Verify persistence via GET
        var (g1, gb1) = await GetCurrentUserAsync();
        Assert.Equal(HttpStatusCode.OK, g1.StatusCode);
        Assert.Null(gb1["user"]!["bio"]);

        // Step 4: Set bio to explicit null
        var json = @"{""user"":{""bio"":null}}";
        using var req3 = AuthedRequest(HttpMethod.Put, "/user");
        req3.Content = new StringContent(json, Encoding.UTF8, "application/json");
        var r3 = await _client.SendAsync(req3);
        var raw3 = await r3.Content.ReadAsStringAsync();
        var b3 = JsonNode.Parse(raw3)!;
        Assert.Equal(HttpStatusCode.OK, r3.StatusCode);
        Assert.Null(b3["user"]!["bio"]);

        // Step 5: Verify persistence via GET
        var (g2, gb2) = await GetCurrentUserAsync();
        Assert.Equal(HttpStatusCode.OK, g2.StatusCode);
        Assert.Null(gb2["user"]!["bio"]);

        // Step 6: Restore bio to a new value
        var (r4, b4) = await PutUserAsync(new { bio = "Restored bio" });
        Assert.Equal(HttpStatusCode.OK, r4.StatusCode);
        Assert.Equal("Restored bio", b4["user"]!["bio"]?.GetValue<string>());
    }

    // ── TC18: Full image lifecycle: set → normalize → null → restore ──

    [Fact]
    public async Task ImageLifecycle_SetThenNormalizeThenNullThenRestore()
    {
        // Step 1: Set image to a URL
        var (r1, b1) = await PutUserAsync(new { image = "https://example.com/pic.jpg" });
        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);
        Assert.Equal("https://example.com/pic.jpg", b1["user"]!["image"]?.GetValue<string>());

        // Step 2: Set image to empty string — normalizes to null
        var (r2, b2) = await PutUserAsync(new { image = "" });
        Assert.Equal(HttpStatusCode.OK, r2.StatusCode);
        Assert.Null(b2["user"]!["image"]);

        // Step 3: Verify persistence via GET
        var (g1, gb1) = await GetCurrentUserAsync();
        Assert.Equal(HttpStatusCode.OK, g1.StatusCode);
        Assert.Null(gb1["user"]!["image"]);

        // Step 4: Set image to explicit null
        var json = @"{""user"":{""image"":null}}";
        using var req3 = AuthedRequest(HttpMethod.Put, "/user");
        req3.Content = new StringContent(json, Encoding.UTF8, "application/json");
        var r3 = await _client.SendAsync(req3);
        var raw3 = await r3.Content.ReadAsStringAsync();
        var b3 = JsonNode.Parse(raw3)!;
        Assert.Equal(HttpStatusCode.OK, r3.StatusCode);
        Assert.Null(b3["user"]!["image"]);

        // Step 5: Verify persistence via GET
        var (g2, gb2) = await GetCurrentUserAsync();
        Assert.Equal(HttpStatusCode.OK, g2.StatusCode);
        Assert.Null(gb2["user"]!["image"]);

        // Step 6: Restore image to a new URL
        var (r4, b4) = await PutUserAsync(new { image = "https://example.com/new.jpg" });
        Assert.Equal(HttpStatusCode.OK, r4.StatusCode);
        Assert.Equal("https://example.com/new.jpg", b4["user"]!["image"]?.GetValue<string>());
    }

    // ── TC19: Nullable valid + non-nullable invalid in same request ──

    [Fact]
    public async Task NullableValidWithNonNullableInvalid_RejectedWith422()
    {
        // bio="" is valid (nullable, normalizes), email="" is invalid (non-nullable)
        var json = @"{""user"":{""bio"":"""",""email"":""""}}";
        using var req = AuthedRequest(HttpMethod.Put, "/user");
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(req);
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(body["errors"]);
    }
}
