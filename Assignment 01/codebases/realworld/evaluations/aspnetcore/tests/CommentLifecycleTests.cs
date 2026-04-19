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

public class CommentLifecycleTests : IDisposable
{
    private readonly HttpClient _client;
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("REALWORLD_BASE_URL") ?? "http://localhost:5000";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CommentLifecycleTests()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public void Dispose() => _client.Dispose();

    private static string Uid() => Guid.NewGuid().ToString("N")[..8];

    private static StringContent JsonContent(object payload) =>
        new(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json");

    private HttpRequestMessage AuthRequest(HttpMethod method, string url, string token) =>
        new(method, url)
        {
            Headers = { { "Authorization", $"Token {token}" } }
        };

    // ── Helper: Register a user and return (token, username) ──

    private async Task<(string token, string username)> RegisterAsync()
    {
        var uid = Uid();
        var username = $"cmt_{uid}";
        var email = $"cmt_{uid}@test.com";
        var payload = new { user = new { username, email, password = "password123" } };
        var response = await _client.PostAsync("/users", JsonContent(payload));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Registration failed: {response.StatusCode}");
        var body = JsonNode.Parse(raw)!;
        var token = body["user"]!["token"]!.GetValue<string>();
        return (token, username);
    }

    // ── Helper: Create an article and return its slug ──

    private async Task<string> CreateArticleAsync(string token)
    {
        var uid = Uid();
        var payload = new
        {
            article = new
            {
                title = $"Comment Article {uid}",
                description = "For comments",
                body = "Article body"
            }
        };
        var request = AuthRequest(HttpMethod.Post, "/articles", token);
        request.Content = JsonContent(payload);
        var response = await _client.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Article creation failed: {response.StatusCode}");
        var body = JsonNode.Parse(raw)!;
        return body["article"]!["slug"]!.GetValue<string>();
    }

    // ── Helper: Create a comment and return its id ──

    private async Task<int> CreateCommentAsync(string token, string slug, string body)
    {
        var payload = new { comment = new { body } };
        var request = AuthRequest(HttpMethod.Post, $"/articles/{slug}/comments", token);
        request.Content = JsonContent(payload);
        var response = await _client.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Comment creation failed: {response.StatusCode}: {raw}");
        var json = JsonNode.Parse(raw)!;
        return json["comment"]!["id"]!.GetValue<int>();
    }

    // ── Helper: List comments ──

    private async Task<JsonArray> ListCommentsAsync(string slug, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/articles/{slug}/comments");
        if (token != null)
            request.Headers.Add("Authorization", $"Token {token}");
        var response = await _client.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = JsonNode.Parse(raw)!;
        return body["comments"]!.AsArray();
    }

    // ── Helper: Delete a comment ──

    private async Task<HttpResponseMessage> DeleteCommentAsync(string token, string slug, int commentId)
    {
        var request = AuthRequest(HttpMethod.Delete, $"/articles/{slug}/comments/{commentId}", token);
        return await _client.SendAsync(request);
    }

    // ── Helper: Full setup: register, create article, create one comment ──

    private async Task<(string token, string username, string slug, int commentId)> SetupWithCommentAsync()
    {
        var (token, username) = await RegisterAsync();
        var slug = await CreateArticleAsync(token);
        var commentId = await CreateCommentAsync(token, slug, "Test comment body");
        return (token, username, slug, commentId);
    }

    // ══════════════════════════════════════════════════════════
    // TC01: Create comment successfully (R1)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateCommentSuccessfully_Returns201WithPayload()
    {
        var (token, username, slug, _) = await SetupWithCommentAsync();

        // Re-create to inspect the response directly
        var payload = new { comment = new { body = "Test comment body" } };
        var request = AuthRequest(HttpMethod.Post, $"/articles/{slug}/comments", token);
        request.Content = JsonContent(payload);
        var response = await _client.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var comment = body["comment"]!;
        Assert.True(comment["id"]!.GetValue<int>() > 0, "Comment id should be a positive integer");
        Assert.Equal("Test comment body", comment["body"]!.GetValue<string>());
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", comment["createdAt"]!.GetValue<string>());
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", comment["updatedAt"]!.GetValue<string>());
        Assert.Equal(username, comment["author"]!["username"]!.GetValue<string>());
    }

    // ══════════════════════════════════════════════════════════
    // TC02: List comments with authentication (R2)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task ListCommentsWithAuth_Returns200WithArray()
    {
        var (token, _, slug, _) = await SetupWithCommentAsync();

        var comments = await ListCommentsAsync(slug, token);

        Assert.NotEmpty(comments);
        var c = comments[0]!;
        Assert.True(c["id"]!.GetValue<int>() > 0);
        Assert.NotNull(c["body"]);
        Assert.NotNull(c["createdAt"]);
        Assert.NotNull(c["updatedAt"]);
        Assert.NotNull(c["author"]!["username"]);
    }

    // ══════════════════════════════════════════════════════════
    // TC03: List comments without authentication (R3)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task ListCommentsWithoutAuth_Returns200WithSameStructure()
    {
        var (token, _, slug, _) = await SetupWithCommentAsync();

        var comments = await ListCommentsAsync(slug, token: null);

        Assert.NotEmpty(comments);
        var c = comments[0]!;
        Assert.True(c["id"]!.GetValue<int>() > 0);
        Assert.Equal("Test comment body", c["body"]!.GetValue<string>());
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", c["createdAt"]!.GetValue<string>());
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", c["updatedAt"]!.GetValue<string>());
        Assert.NotNull(c["author"]!["username"]);
    }

    // ══════════════════════════════════════════════════════════
    // TC04: Create comment without authentication (R4)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateCommentNoAuth_Returns401WithTokenError()
    {
        var (token, _, slug, _) = await SetupWithCommentAsync();

        var payload = new { comment = new { body = "test" } };
        var response = await _client.PostAsync($"/articles/{slug}/comments", JsonContent(payload));
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = JsonNode.Parse(raw)!;
        var tokenErrors = body["errors"]!["token"]?.AsArray();
        Assert.NotNull(tokenErrors);
        Assert.Equal("is missing", tokenErrors![0]!.GetValue<string>());
    }

    // ══════════════════════════════════════════════════════════
    // TC05: Delete comment without authentication (R5)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteCommentNoAuth_Returns401WithTokenError()
    {
        var (token, _, slug, commentId) = await SetupWithCommentAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/articles/{slug}/comments/{commentId}");
        var response = await _client.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = JsonNode.Parse(raw)!;
        var tokenErrors = body["errors"]!["token"]?.AsArray();
        Assert.NotNull(tokenErrors);
        Assert.Equal("is missing", tokenErrors![0]!.GetValue<string>());
    }

    // ══════════════════════════════════════════════════════════
    // TC06: Delete owned comment successfully (R6)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteOwnedComment_Returns204()
    {
        var (token, _, slug, commentId) = await SetupWithCommentAsync();

        var response = await DeleteCommentAsync(token, slug, commentId);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ══════════════════════════════════════════════════════════
    // TC07: Deleted comment absent from listing (R7)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task DeletedCommentNotInListing_ReturnsEmptyArray()
    {
        var (token, _, slug, commentId) = await SetupWithCommentAsync();

        // Delete the comment
        var deleteResponse = await DeleteCommentAsync(token, slug, commentId);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // List comments — should be empty
        var comments = await ListCommentsAsync(slug);
        Assert.Empty(comments);
    }

    // ══════════════════════════════════════════════════════════
    // TC08: Selective deletion preserves other comment (R8)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task SelectiveDeletion_PreservesOtherComment()
    {
        var (token, _, slug, _) = await SetupWithCommentAsync();

        // Create two comments
        var firstId = await CreateCommentAsync(token, slug, "First comment");
        var secondId = await CreateCommentAsync(token, slug, "Second comment");

        // Verify both exist
        var beforeDelete = await ListCommentsAsync(slug);
        Assert.Equal(3, beforeDelete.Count); // 1 from setup + 2 new

        // Delete only the first
        var deleteResponse = await DeleteCommentAsync(token, slug, firstId);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // List and verify second survives
        var afterDelete = await ListCommentsAsync(slug);
        Assert.Equal(2, afterDelete.Count);
        Assert.Contains(afterDelete, c => c!["body"]!.GetValue<string>() == "Second comment");
        Assert.DoesNotContain(afterDelete, c => c!["body"]!.GetValue<string>() == "First comment");
    }

    // ══════════════════════════════════════════════════════════
    // TC09: Delete another user's comment is forbidden (R9)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteOtherUsersComment_Returns403AndCommentSurvives()
    {
        // User A: register, create article, create comment
        var (tokenA, _) = await RegisterAsync();
        var slug = await CreateArticleAsync(tokenA);
        var commentId = await CreateCommentAsync(tokenA, slug, "A's comment");

        // User B: register, try to delete A's comment
        var (tokenB, _) = await RegisterAsync();
        var deleteResponse = await DeleteCommentAsync(tokenB, slug, commentId);

        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        var raw = await deleteResponse.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;
        var commentErrors = body["errors"]!["comment"]?.AsArray();
        Assert.NotNull(commentErrors);
        Assert.Equal("forbidden", commentErrors![0]!.GetValue<string>());

        // Verify the comment survived
        var comments = await ListCommentsAsync(slug);
        Assert.NotEmpty(comments);
        Assert.Contains(comments, c => c!["body"]!.GetValue<string>() == "A's comment");
    }

    // ══════════════════════════════════════════════════════════
    // TC10: Create comment with empty body (R1 boundary)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateCommentWithEmptyBody_Returns422()
    {
        var (token, _, slug, _) = await SetupWithCommentAsync();

        var payload = new { comment = new { body = "" } };
        var request = AuthRequest(HttpMethod.Post, $"/articles/{slug}/comments", token);
        request.Content = JsonContent(payload);
        var response = await _client.SendAsync(request);

        Assert.True(
            response.StatusCode == HttpStatusCode.UnprocessableEntity ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected 422 or 400 for empty body, got {response.StatusCode}");
    }

    // ══════════════════════════════════════════════════════════
    // TC11: Create comment with missing body field (R1 edge)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateCommentWithMissingBodyField_Returns422()
    {
        var (token, _, slug, _) = await SetupWithCommentAsync();

        var payload = new { comment = new { } };
        var request = AuthRequest(HttpMethod.Post, $"/articles/{slug}/comments", token);
        request.Content = JsonContent(payload);
        var response = await _client.SendAsync(request);

        Assert.True(
            response.StatusCode == HttpStatusCode.UnprocessableEntity ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected 422 or 400 for missing body field, got {response.StatusCode}");
    }

    // ══════════════════════════════════════════════════════════
    // TC12: Create comment with single-character body (R1 boundary)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateCommentWithSingleCharBody_Returns201()
    {
        var (token, _, slug, _) = await SetupWithCommentAsync();

        var payload = new { comment = new { body = "a" } };
        var request = AuthRequest(HttpMethod.Post, $"/articles/{slug}/comments", token);
        request.Content = JsonContent(payload);
        var response = await _client.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = JsonNode.Parse(raw)!;
        Assert.Equal("a", body["comment"]!["body"]!.GetValue<string>());
    }

    // ══════════════════════════════════════════════════════════
    // TC13: Create comment on non-existent article (R1 edge)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateCommentOnNonExistentArticle_Returns404()
    {
        var (token, _) = await RegisterAsync();

        var payload = new { comment = new { body = "test" } };
        var request = AuthRequest(HttpMethod.Post, "/articles/non-existent-slug-xyz/comments", token);
        request.Content = JsonContent(payload);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ══════════════════════════════════════════════════════════
    // TC14: Delete non-existent comment id (R6 edge)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteNonExistentCommentId_Returns404()
    {
        var (token, _, slug, _) = await SetupWithCommentAsync();

        var response = await DeleteCommentAsync(token, slug, 999999);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ══════════════════════════════════════════════════════════
    // TC15: Delete already-deleted comment (R6 state edge)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteAlreadyDeletedComment_Returns404()
    {
        var (token, _, slug, commentId) = await SetupWithCommentAsync();

        // First delete succeeds
        var firstDelete = await DeleteCommentAsync(token, slug, commentId);
        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);

        // Second delete should fail
        var secondDelete = await DeleteCommentAsync(token, slug, commentId);
        Assert.Equal(HttpStatusCode.NotFound, secondDelete.StatusCode);
    }
}
