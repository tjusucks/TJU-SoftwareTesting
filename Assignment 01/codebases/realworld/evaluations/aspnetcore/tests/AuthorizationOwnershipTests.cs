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

public class AuthorizationOwnershipTests : IDisposable
{
    private readonly HttpClient _client;
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("REALWORLD_BASE_URL") ?? "http://localhost:5000";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuthorizationOwnershipTests()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public void Dispose() => _client.Dispose();

    private static string Uid() => Guid.NewGuid().ToString("N")[..8];

    private static StringContent JsonContent(object payload) =>
        new(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json");

    private HttpClient AuthenticatedClient(string token)
    {
        var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        client.DefaultRequestHeaders.Add("Authorization", $"Token {token}");
        return client;
    }

    // ── Helper: Register a fresh user and return (username, token) ──

    private async Task<(string username, string token)> RegisterAsync()
    {
        var uid = Uid();
        var username = $"authz_{uid}";
        var email = $"authz_{uid}@test.com";
        var payload = new { user = new { username, email, password = "password123" } };
        var response = await _client.PostAsync("/api/users", JsonContent(payload));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Registration failed: {response.StatusCode} {raw}");
        var body = JsonNode.Parse(raw)!;
        var token = body["user"]!["token"]!.GetValue<string>();
        return (username, token);
    }

    // ── Helper: Create an article and return (slug, title, description, body) ──

    private async Task<(string slug, string title, string description, string body)> CreateArticleAsync(
        string token, string title, string description, string body)
    {
        var payload = new { article = new { title, description, body } };
        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PostAsync("/api/articles", JsonContent(payload));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Article creation failed: {response.StatusCode} {raw}");
        var json = JsonNode.Parse(raw)!;
        var slug = json["article"]!["slug"]!.GetValue<string>();
        return (slug, title, description, body);
    }

    // ── Helper: Create a comment and return its id ──

    private async Task<int> CreateCommentAsync(string token, string slug, string body)
    {
        var payload = new { comment = new { body } };
        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PostAsync($"/api/articles/{slug}/comments", JsonContent(payload));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Comment creation failed: {response.StatusCode} {raw}");
        var json = JsonNode.Parse(raw)!;
        return json["comment"]!["id"]!.GetValue<int>();
    }

    // ── Helper: Get article by slug ──

    private async Task<(HttpStatusCode status, JsonNode body)> GetArticleAsync(string slug)
    {
        var response = await _client.GetAsync($"/api/articles/{slug}");
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;
        return (response.StatusCode, body);
    }

    // ── Helper: List comments ──

    private async Task<JsonArray> ListCommentsAsync(string slug)
    {
        var response = await _client.GetAsync($"/api/articles/{slug}/comments");
        var raw = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = JsonNode.Parse(raw)!;
        return body["comments"]!.AsArray();
    }

    // ══════════════════════════════════════════════════════════
    // TC01: Owner creates article successfully (R1)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task OwnerCreatesArticle_Returns201WithPayload()
    {
        var (username, token) = await RegisterAsync();
        var uid = Uid();
        var title = $"Authz Article {uid}";

        var (slug, _, _, _) = await CreateArticleAsync(token, title, "test desc", "test body");

        Assert.False(string.IsNullOrEmpty(slug));

        var (status, body) = await GetArticleAsync(slug);
        Assert.Equal(HttpStatusCode.OK, status);
        var article = body["article"]!;
        Assert.Equal(title, article["title"]?.GetValue<string>());
        Assert.Equal(username, article["author"]?["username"]?.GetValue<string>());
    }

    // ══════════════════════════════════════════════════════════
    // TC02: Non-owner delete article returns 403 (R2, R4)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task NonOwnerDeleteArticle_Returns403()
    {
        var (_, ownerToken) = await RegisterAsync();
        var (_, nonOwnerToken) = await RegisterAsync();
        var uid = Uid();

        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, $"AuthzDel {uid}", "desc", "body");

        using var nonOwnerClient = AuthenticatedClient(nonOwnerToken);
        var response = await nonOwnerClient.DeleteAsync($"/api/articles/{slug}");
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var articleErrors = body["errors"]?["article"]?.AsArray();
        Assert.NotNull(articleErrors);
        Assert.Contains(articleErrors, e => e?.GetValue<string>() == "forbidden");
    }

    // ══════════════════════════════════════════════════════════
    // TC03: Non-owner update article returns 403 (R3, R4)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task NonOwnerUpdateArticle_Returns403()
    {
        var (_, ownerToken) = await RegisterAsync();
        var (_, nonOwnerToken) = await RegisterAsync();
        var uid = Uid();

        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, $"AuthzUpd {uid}", "desc", "original body");

        var updatePayload = new { article = new { body = "hijacked" } };
        using var nonOwnerClient = AuthenticatedClient(nonOwnerToken);
        var response = await nonOwnerClient.PutAsync($"/api/articles/{slug}", JsonContent(updatePayload));
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var articleErrors = body["errors"]?["article"]?.AsArray();
        Assert.NotNull(articleErrors);
        Assert.Contains(articleErrors, e => e?.GetValue<string>() == "forbidden");
    }

    // ══════════════════════════════════════════════════════════
    // TC04: Article persists after forbidden delete (R5)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task ArticlePersistsAfterForbiddenDelete()
    {
        var (_, ownerToken) = await RegisterAsync();
        var (_, nonOwnerToken) = await RegisterAsync();
        var uid = Uid();

        var origTitle = $"SurvivesDel {uid}";
        var origDesc = "original description";
        var origBody = "original body";
        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, origTitle, origDesc, origBody);

        // Non-owner attempts delete → 403
        using var nonOwnerClient = AuthenticatedClient(nonOwnerToken);
        var deleteResponse = await nonOwnerClient.DeleteAsync($"/api/articles/{slug}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        // Verify article still exists with original data
        var (status, body) = await GetArticleAsync(slug);
        Assert.Equal(HttpStatusCode.OK, status);

        var article = body["article"]!;
        Assert.Equal(origTitle, article["title"]?.GetValue<string>());
        Assert.Equal(origDesc, article["description"]?.GetValue<string>());
        Assert.Equal(origBody, article["body"]?.GetValue<string>());
    }

    // ══════════════════════════════════════════════════════════
    // TC05: Article persists after forbidden update (R6)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task ArticlePersistsAfterForbiddenUpdate()
    {
        var (_, ownerToken) = await RegisterAsync();
        var (_, nonOwnerToken) = await RegisterAsync();
        var uid = Uid();

        var origTitle = $"SurvivesUpd {uid}";
        var origDesc = "original description";
        var origBody = "original body";
        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, origTitle, origDesc, origBody);

        // Non-owner attempts update → 403
        var updatePayload = new { article = new { body = "hijacked" } };
        using var nonOwnerClient = AuthenticatedClient(nonOwnerToken);
        var updateResponse = await nonOwnerClient.PutAsync($"/api/articles/{slug}", JsonContent(updatePayload));
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);

        // Verify article still exists with original data
        var (status, body) = await GetArticleAsync(slug);
        Assert.Equal(HttpStatusCode.OK, status);

        var article = body["article"]!;
        Assert.Equal(origTitle, article["title"]?.GetValue<string>());
        Assert.Equal(origDesc, article["description"]?.GetValue<string>());
        Assert.Equal(origBody, article["body"]?.GetValue<string>());
    }

    // ══════════════════════════════════════════════════════════
    // TC06: Owner creates comment on article (R7)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task OwnerCreatesComment_Returns201WithPayload()
    {
        var (username, token) = await RegisterAsync();
        var uid = Uid();
        var (slug, _, _, _) = await CreateArticleAsync(token, $"CmtArt {uid}", "desc", "body");

        var commentId = await CreateCommentAsync(token, slug, "A's comment");
        Assert.True(commentId > 0, "Comment id should be a positive integer");

        // Verify comment appears in listing
        var comments = await ListCommentsAsync(slug);
        Assert.NotEmpty(comments);
        Assert.Contains(comments, c => c!["body"]!.GetValue<string>() == "A's comment");
    }

    // ══════════════════════════════════════════════════════════
    // TC07: Non-owner delete comment returns 403 (R8, R9)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task NonOwnerDeleteComment_Returns403()
    {
        var (_, ownerToken) = await RegisterAsync();
        var (_, nonOwnerToken) = await RegisterAsync();
        var uid = Uid();

        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, $"CmtAuthz {uid}", "desc", "body");
        var commentId = await CreateCommentAsync(ownerToken, slug, "A's comment");

        using var nonOwnerClient = AuthenticatedClient(nonOwnerToken);
        var response = await nonOwnerClient.DeleteAsync($"/api/articles/{slug}/comments/{commentId}");
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var commentErrors = body["errors"]?["comment"]?.AsArray();
        Assert.NotNull(commentErrors);
        Assert.Contains(commentErrors, e => e?.GetValue<string>() == "forbidden");
    }

    // ══════════════════════════════════════════════════════════
    // TC08: Comment persists after forbidden delete (R10)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task CommentPersistsAfterForbiddenDelete()
    {
        var (_, ownerToken) = await RegisterAsync();
        var (_, nonOwnerToken) = await RegisterAsync();
        var uid = Uid();

        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, $"CmtSurvive {uid}", "desc", "body");
        var commentId = await CreateCommentAsync(ownerToken, slug, "A's comment");

        // Non-owner attempts delete → 403
        using var nonOwnerClient = AuthenticatedClient(nonOwnerToken);
        var deleteResponse = await nonOwnerClient.DeleteAsync($"/api/articles/{slug}/comments/{commentId}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        // Verify comment still exists
        var comments = await ListCommentsAsync(slug);
        Assert.NotEmpty(comments);
        Assert.Contains(comments, c => c!["body"]!.GetValue<string>() == "A's comment");
    }

    // ══════════════════════════════════════════════════════════
    // TC09: Unauthenticated article delete returns 401 (R11)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task UnauthenticatedArticleDelete_Returns401()
    {
        var (_, ownerToken) = await RegisterAsync();
        var uid = Uid();
        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, $"UnauthDel {uid}", "desc", "body");

        var response = await _client.DeleteAsync($"/api/articles/{slug}");
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var tokenErrors = body["errors"]?["token"]?.AsArray();
        Assert.NotNull(tokenErrors);
        Assert.Contains(tokenErrors, e => e?.GetValue<string>() == "is missing");
    }

    // ══════════════════════════════════════════════════════════
    // TC10: Unauthenticated article update returns 401 (R11)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task UnauthenticatedArticleUpdate_Returns401()
    {
        var (_, ownerToken) = await RegisterAsync();
        var uid = Uid();
        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, $"UnauthUpd {uid}", "desc", "body");

        var updatePayload = new { article = new { body = "no auth update" } };
        var response = await _client.PutAsync($"/api/articles/{slug}", JsonContent(updatePayload));
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var tokenErrors = body["errors"]?["token"]?.AsArray();
        Assert.NotNull(tokenErrors);
        Assert.Contains(tokenErrors, e => e?.GetValue<string>() == "is missing");
    }

    // ══════════════════════════════════════════════════════════
    // TC11: Unauthenticated comment delete returns 401 (R11)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task UnauthenticatedCommentDelete_Returns401()
    {
        var (_, ownerToken) = await RegisterAsync();
        var uid = Uid();
        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, $"UnauthCmt {uid}", "desc", "body");
        var commentId = await CreateCommentAsync(ownerToken, slug, "A's comment");

        var response = await _client.DeleteAsync($"/api/articles/{slug}/comments/{commentId}");
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var tokenErrors = body["errors"]?["token"]?.AsArray();
        Assert.NotNull(tokenErrors);
        Assert.Contains(tokenErrors, e => e?.GetValue<string>() == "is missing");
    }

    // ══════════════════════════════════════════════════════════
    // TC12: Owner deletes own article returns 204 (R12)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task OwnerDeletesOwnArticle_Returns204()
    {
        var (_, token) = await RegisterAsync();
        var uid = Uid();
        var (slug, _, _, _) = await CreateArticleAsync(token, $"OwnerDel {uid}", "desc", "body");

        using var authClient = AuthenticatedClient(token);
        var response = await authClient.DeleteAsync($"/api/articles/{slug}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ══════════════════════════════════════════════════════════
    // TC13: Owner updates own article returns 200 (R13)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task OwnerUpdatesOwnArticle_Returns200WithUpdatedPayload()
    {
        var (_, token) = await RegisterAsync();
        var uid = Uid();
        var origTitle = $"OwnerUpd {uid}";
        var (slug, _, _, _) = await CreateArticleAsync(token, origTitle, "original desc", "original body");

        var updatePayload = new { article = new { body = "updated by owner" } };
        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PutAsync($"/api/articles/{slug}", JsonContent(updatePayload));
        var raw = await response.Content.ReadAsStringAsync();
        var body = JsonNode.Parse(raw)!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var article = body["article"]!;
        Assert.Equal("updated by owner", article["body"]?.GetValue<string>());
        Assert.Equal(origTitle, article["title"]?.GetValue<string>());
    }

    // ══════════════════════════════════════════════════════════
    // TC14: Owner deletes own comment returns 204 (R14)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task OwnerDeletesOwnComment_Returns204()
    {
        var (_, token) = await RegisterAsync();
        var uid = Uid();
        var (slug, _, _, _) = await CreateArticleAsync(token, $"CmtOwnerDel {uid}", "desc", "body");
        var commentId = await CreateCommentAsync(token, slug, "My comment");

        using var authClient = AuthenticatedClient(token);
        var response = await authClient.DeleteAsync($"/api/articles/{slug}/comments/{commentId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ══════════════════════════════════════════════════════════
    // TC15: Sequential forbidden operations — article survives both (R2, R3, R5, R6)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task SequentialForbiddenOps_ArticleSurvivesBoth()
    {
        var (_, ownerToken) = await RegisterAsync();
        var (_, nonOwnerToken) = await RegisterAsync();
        var uid = Uid();

        var origTitle = $"SeqSurvive {uid}";
        var origDesc = "original description";
        var origBody = "original body";
        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, origTitle, origDesc, origBody);

        using var nonOwnerClient = AuthenticatedClient(nonOwnerToken);

        // First: non-owner delete → 403
        var deleteResponse = await nonOwnerClient.DeleteAsync($"/api/articles/{slug}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        // Second: non-owner update → 403
        var updatePayload = new { article = new { body = "hijacked" } };
        var updateResponse = await nonOwnerClient.PutAsync($"/api/articles/{slug}", JsonContent(updatePayload));
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);

        // Verify article still exists with original data
        var (status, body) = await GetArticleAsync(slug);
        Assert.Equal(HttpStatusCode.OK, status);

        var article = body["article"]!;
        Assert.Equal(origTitle, article["title"]?.GetValue<string>());
        Assert.Equal(origDesc, article["description"]?.GetValue<string>());
        Assert.Equal(origBody, article["body"]?.GetValue<string>());
    }

    // ══════════════════════════════════════════════════════════
    // TC16: Forbidden comment delete with multiple comments — non-target survives (R8, R10)
    // ══════════════════════════════════════════════════════════

    [Fact]
    public async Task ForbiddenCommentDelete_MultipleCommentsAllSurvive()
    {
        var (_, ownerToken) = await RegisterAsync();
        var (_, nonOwnerToken) = await RegisterAsync();
        var uid = Uid();

        var (slug, _, _, _) = await CreateArticleAsync(ownerToken, $"MultiCmt {uid}", "desc", "body");
        var comment1Id = await CreateCommentAsync(ownerToken, slug, "First comment");
        var comment2Id = await CreateCommentAsync(ownerToken, slug, "Second comment");

        // Verify both comments exist
        var beforeDelete = await ListCommentsAsync(slug);
        Assert.Equal(2, beforeDelete.Count);

        // Non-owner attempts to delete comment 1 → 403
        using var nonOwnerClient = AuthenticatedClient(nonOwnerToken);
        var deleteResponse = await nonOwnerClient.DeleteAsync($"/api/articles/{slug}/comments/{comment1Id}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        // Verify both comments still exist
        var afterDelete = await ListCommentsAsync(slug);
        Assert.Equal(2, afterDelete.Count);
        Assert.Contains(afterDelete, c => c!["body"]!.GetValue<string>() == "First comment");
        Assert.Contains(afterDelete, c => c!["body"]!.GetValue<string>() == "Second comment");
    }
}
