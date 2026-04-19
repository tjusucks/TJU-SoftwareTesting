using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;

namespace RealWorld.Blackbox.Tests;

public class ArticleLifecycleTests : IDisposable
{
    private readonly HttpClient _client;
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("REALWORLD_BASE_URL") ?? "http://localhost:3000";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ArticleLifecycleTests()
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

    // ── Helper: Register a fresh user and return (email, password, username, token) ──

    private async Task<(string email, string password, string username, string token)> RegisterFreshUserAsync()
    {
        var uid = Uid();
        var username = $"art_{uid}";
        var email = $"art_{uid}@test.com";
        var password = "password123";
        var payload = new { user = new { username, email, password } };
        var response = await _client.PostAsync("/api/users", JsonContent(payload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        Assert.True(response.IsSuccessStatusCode, $"Fresh user registration failed: {response.StatusCode}");
        var token = body["user"]!["token"]!.GetValue<string>();
        return (email, password, username, token);
    }

    // ── Helper: Create an article and return (slug, title, description, body, tagList) ──

    private async Task<(string slug, string title, string description, string body, JsonArray tagList)> CreateArticleAsync(
        string token, string title, string description, string body, string[]? tagList = null)
    {
        var article = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["description"] = description,
            ["body"] = body
        };

        if (tagList != null)
        {
            article["tagList"] = tagList;
        }

        var payload = new Dictionary<string, object?>
        {
            ["article"] = article
        };

        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PostAsync("/api/articles", JsonContent(payload));
        var responseBody = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        Assert.True(response.IsSuccessStatusCode, $"Article creation failed: {response.StatusCode}");
        var articleNode = responseBody["article"]!;
        var slug = articleNode["slug"]!.GetValue<string>();
        var returnedTagList = articleNode["tagList"]!.AsArray();
        return (slug, title, description, body, returnedTagList);
    }

    // ── Helper: Get an article by slug ──

    private async Task<(HttpResponseMessage Response, JsonNode Body)> GetArticleAsync(string slug)
    {
        var response = await _client.GetAsync($"/api/articles/{slug}");
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        return (response, body);
    }

    // ── TC01: Create article with valid input and tags ──

    [Fact]
    public async Task CreateArticleWithTags_Returns201WithAllFields()
    {
        var (_, _, username, token) = await RegisterFreshUserAsync();
        var uid = Uid();
        var title = $"Test Article {uid}";
        var tag1 = $"d_{uid}";
        var tag2 = $"t_{uid}";

        var payload = new
        {
            article = new
            {
                title,
                description = "Test description",
                body = "Test body content",
                tagList = new[] { tag1, tag2 }
            }
        };

        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PostAsync("/api/articles", JsonContent(payload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var article = body["article"]!;
        Assert.Equal(title, article["title"]?.GetValue<string>());
        Assert.False(string.IsNullOrEmpty(article["slug"]?.GetValue<string>()));
        Assert.Equal("Test description", article["description"]?.GetValue<string>());
        Assert.Equal("Test body content", article["body"]?.GetValue<string>());

        var tags = article["tagList"]!.AsArray();
        Assert.Equal(2, tags.Count);
        Assert.Equal(tag1, tags[0]?.GetValue<string>());
        Assert.Equal(tag2, tags[1]?.GetValue<string>());

        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T", article["createdAt"]?.GetValue<string>());
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T", article["updatedAt"]?.GetValue<string>());
        Assert.False(article["favorited"]?.GetValue<bool>());
        Assert.Equal(0, article["favoritesCount"]?.GetValue<int>());
        Assert.Equal(username, article["author"]?["username"]?.GetValue<string>());
    }

    // ── TC02: Create article without tagList ──

    [Fact]
    public async Task CreateArticleWithoutTagList_Returns201WithEmptyTagArray()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();

        var payload = new
        {
            article = new
            {
                title = $"NoTag Article {uid}",
                description = "Desc",
                body = "Body"
            }
        };

        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PostAsync("/api/articles", JsonContent(payload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var article = body["article"]!;
        var tags = article["tagList"]!.AsArray();
        Assert.Empty(tags);
    }

    // ── TC03: Create article without authentication ──

    [Fact]
    public async Task CreateArticleWithoutAuth_Returns401()
    {
        var payload = new
        {
            article = new
            {
                title = "No Auth Article",
                description = "test",
                body = "test"
            }
        };

        var response = await _client.PostAsync("/api/articles", JsonContent(payload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var tokenErrors = body["errors"]?["token"]?.AsArray();
        Assert.NotNull(tokenErrors);
        Assert.Contains(tokenErrors, e => e?.GetValue<string>() == "is missing");
    }

    // ── TC04: Create article with empty title ──

    [Fact]
    public async Task CreateArticleWithEmptyTitle_Returns422()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();

        var payload = new
        {
            article = new
            {
                title = "",
                description = "test",
                body = "test"
            }
        };

        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PostAsync("/api/articles", JsonContent(payload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var titleErrors = body["errors"]?["title"]?.AsArray();
        Assert.NotNull(titleErrors);
        Assert.Contains(titleErrors, e => e?.GetValue<string>() == "can't be blank");
    }

    // ── TC05: Create article with empty description ──

    [Fact]
    public async Task CreateArticleWithEmptyDescription_Returns422()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();

        var payload = new
        {
            article = new
            {
                title = $"ErrDesc {uid}",
                description = "",
                body = "test"
            }
        };

        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PostAsync("/api/articles", JsonContent(payload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var descErrors = body["errors"]?["description"]?.AsArray();
        Assert.NotNull(descErrors);
        Assert.Contains(descErrors, e => e?.GetValue<string>() == "can't be blank");
    }

    // ── TC06: Create article with empty body ──

    [Fact]
    public async Task CreateArticleWithEmptyBody_Returns422()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();

        var payload = new
        {
            article = new
            {
                title = $"ErrBody {uid}",
                description = "test",
                body = ""
            }
        };

        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PostAsync("/api/articles", JsonContent(payload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var bodyErrors = body["errors"]?["body"]?.AsArray();
        Assert.NotNull(bodyErrors);
        Assert.Contains(bodyErrors, e => e?.GetValue<string>() == "can't be blank");
    }

    // ── TC07: Create two articles with duplicate title ──

    [Fact]
    public async Task CreateArticleWithDuplicateTitle_Returns201WithUniqueSlug()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();
        var dupTitle = $"Dup Title {uid}";

        var payload1 = new { article = new { title = dupTitle, description = "first", body = "first" } };
        var payload2 = new { article = new { title = dupTitle, description = "second", body = "second" } };

        using var authClient = AuthenticatedClient(token);
        var response1 = await authClient.PostAsync("/api/articles", JsonContent(payload1));
        var body1 = JsonNode.Parse(await response1.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        var slug1 = body1["article"]!["slug"]!.GetValue<string>();

        var response2 = await authClient.PostAsync("/api/articles", JsonContent(payload2));
        var body2 = JsonNode.Parse(await response2.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
        var slug2 = body2["article"]!["slug"]!.GetValue<string>();

        Assert.NotEqual(slug1, slug2);
    }

    // ── TC08: Update article body only ──

    [Fact]
    public async Task UpdateArticleBodyOnly_Returns200WithOtherFieldsPreserved()
    {
        var (_, _, username, token) = await RegisterFreshUserAsync();
        var uid = Uid();
        var tag1 = $"d_{uid}";
        var tag2 = $"t_{uid}";

        var (slug, origTitle, origDesc, _, _) = await CreateArticleAsync(
            token, $"Test Article {uid}", "Test description", "Test body content", new[] { tag1, tag2 });

        var updatePayload = new { article = new { body = "Updated body content" } };
        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PutAsync($"/api/articles/{slug}", JsonContent(updatePayload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var article = body["article"]!;
        Assert.Equal("Updated body content", article["body"]?.GetValue<string>());
        Assert.Equal(origTitle, article["title"]?.GetValue<string>());
        Assert.Equal(slug, article["slug"]?.GetValue<string>());
        Assert.Equal(origDesc, article["description"]?.GetValue<string>());

        var tags = article["tagList"]!.AsArray();
        Assert.Equal(2, tags.Count);
        Assert.Equal(tag1, tags[0]?.GetValue<string>());
        Assert.Equal(tag2, tags[1]?.GetValue<string>());

        Assert.Equal(username, article["author"]?["username"]?.GetValue<string>());
    }

    // ── TC09: Update article without tagList preserves tags ──

    [Fact]
    public async Task UpdateArticleWithoutTagList_PreservesExistingTags()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();
        var tag1 = $"d_{uid}";
        var tag2 = $"t_{uid}";

        var (slug, _, _, _, _) = await CreateArticleAsync(
            token, $"TagPreserve {uid}", "Desc", "Body", new[] { tag1, tag2 });

        var updatePayload = new { article = new { body = "Body without touching tags" } };
        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PutAsync($"/api/articles/{slug}", JsonContent(updatePayload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var article = body["article"]!;
        Assert.Equal("Body without touching tags", article["body"]?.GetValue<string>());

        var tags = article["tagList"]!.AsArray();
        Assert.Equal(2, tags.Count);
        Assert.Contains(tags, t => t?.GetValue<string>() == tag1);
        Assert.Contains(tags, t => t?.GetValue<string>() == tag2);
    }

    // ── TC10: Update article with empty tagList removes all tags ──

    [Fact]
    public async Task UpdateArticleWithEmptyTagList_RemovesAllTags()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();
        var tag1 = $"d_{uid}";
        var tag2 = $"t_{uid}";

        var (slug, _, _, _, _) = await CreateArticleAsync(
            token, $"TagRemove {uid}", "Desc", "Body", new[] { tag1, tag2 });

        var updatePayload = new { article = new { tagList = Array.Empty<string>() } };
        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PutAsync($"/api/articles/{slug}", JsonContent(updatePayload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var article = body["article"]!;
        var tags = article["tagList"]!.AsArray();
        Assert.Empty(tags);
    }

    // ── TC11: Update article with null tagList is rejected ──

    [Fact]
    public async Task UpdateArticleWithNullTagList_Returns422()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();

        var (slug, _, _, _, _) = await CreateArticleAsync(
            token, $"NullTag {uid}", "Desc", "Body", new[] { $"tag_{uid}" });

        // Use raw JSON to send tagList: null (System.Text.Json won't serialize null in anonymous objects the same way)
        var json = $"{{\"article\":{{\"tagList\":null}}}}";
        using var authClient = AuthenticatedClient(token);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await authClient.PutAsync($"/api/articles/{slug}", content);
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ── TC12: Update persistence verified via GET ──

    [Fact]
    public async Task UpdatePersistence_VerifiedViaGet()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();

        var (slug, _, _, _, _) = await CreateArticleAsync(
            token, $"Persist {uid}", "Original desc", "Original body");

        var updatePayload = new { article = new { body = "Updated body content" } };
        using var authClient = AuthenticatedClient(token);
        var updateResponse = await authClient.PutAsync($"/api/articles/{slug}", JsonContent(updatePayload));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var (getResponse, getBody) = await GetArticleAsync(slug);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var article = getBody["article"]!;
        Assert.Equal("Updated body content", article["body"]?.GetValue<string>());
        Assert.Equal("Original desc", article["description"]?.GetValue<string>());
        Assert.Equal($"Persist {uid}", article["title"]?.GetValue<string>());
    }

    // ── TC13: Update article without authentication ──

    [Fact]
    public async Task UpdateArticleWithoutAuth_Returns401()
    {
        var payload = new { article = new { body = "test" } };
        var response = await _client.PutAsync("/api/articles/some-slug", JsonContent(payload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var tokenErrors = body["errors"]?["token"]?.AsArray();
        Assert.NotNull(tokenErrors);
        Assert.Contains(tokenErrors, e => e?.GetValue<string>() == "is missing");
    }

    // ── TC14: Non-owner attempts to update article ──

    [Fact]
    public async Task NonOwnerUpdateArticle_Returns403()
    {
        var (_, _, _, ownerToken) = await RegisterFreshUserAsync();
        var (_, _, _, nonOwnerToken) = await RegisterFreshUserAsync();
        var uid = Uid();

        var (slug, _, _, _, _) = await CreateArticleAsync(
            ownerToken, $"OwnerArt {uid}", "Desc", "Body");

        var updatePayload = new { article = new { body = "hijacked" } };
        using var nonOwnerClient = AuthenticatedClient(nonOwnerToken);
        var response = await nonOwnerClient.PutAsync($"/api/articles/{slug}", JsonContent(updatePayload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var articleErrors = body["errors"]?["article"]?.AsArray();
        Assert.NotNull(articleErrors);
        Assert.Contains(articleErrors, e => e?.GetValue<string>() == "forbidden");
    }

    // ── TC15: Update article with unknown slug ──

    [Fact]
    public async Task UpdateArticleWithUnknownSlug_Returns404()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();

        var updatePayload = new { article = new { body = "test" } };
        using var authClient = AuthenticatedClient(token);
        var response = await authClient.PutAsync($"/api/articles/unknown-slug-{uid}", JsonContent(updatePayload));
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var articleErrors = body["errors"]?["article"]?.AsArray();
        Assert.NotNull(articleErrors);
        Assert.Contains(articleErrors, e => e?.GetValue<string>() == "not found");
    }

    // ── TC16: Delete article by owner ──

    [Fact]
    public async Task DeleteArticleByOwner_Returns204()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();

        var (slug, _, _, _, _) = await CreateArticleAsync(
            token, $"DeleteArt {uid}", "Desc", "Body");

        using var authClient = AuthenticatedClient(token);
        var response = await authClient.DeleteAsync($"/api/articles/{slug}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ── TC17: Get article after deletion returns 404 ──

    [Fact]
    public async Task GetArticleAfterDeletion_Returns404()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();

        var (slug, _, _, _, _) = await CreateArticleAsync(
            token, $"GoneArt {uid}", "Desc", "Body");

        using var authClient = AuthenticatedClient(token);
        await authClient.DeleteAsync($"/api/articles/{slug}");

        var (getResponse, getBody) = await GetArticleAsync(slug);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var articleErrors = getBody["errors"]?["article"]?.AsArray();
        Assert.NotNull(articleErrors);
        Assert.Contains(articleErrors, e => e?.GetValue<string>() == "not found");
    }

    // ── TC18: Delete article without authentication ──

    [Fact]
    public async Task DeleteArticleWithoutAuth_Returns401()
    {
        var response = await _client.DeleteAsync("/api/articles/some-slug");
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var tokenErrors = body["errors"]?["token"]?.AsArray();
        Assert.NotNull(tokenErrors);
        Assert.Contains(tokenErrors, e => e?.GetValue<string>() == "is missing");
    }

    // ── TC19: Non-owner attempts to delete article ──

    [Fact]
    public async Task NonOwnerDeleteArticle_Returns403()
    {
        var (_, _, _, ownerToken) = await RegisterFreshUserAsync();
        var (_, _, _, nonOwnerToken) = await RegisterFreshUserAsync();
        var uid = Uid();

        var (slug, _, _, _, _) = await CreateArticleAsync(
            ownerToken, $"OwnerDelArt {uid}", "Desc", "Body");

        using var nonOwnerClient = AuthenticatedClient(nonOwnerToken);
        var response = await nonOwnerClient.DeleteAsync($"/api/articles/{slug}");
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var articleErrors = body["errors"]?["article"]?.AsArray();
        Assert.NotNull(articleErrors);
        Assert.Contains(articleErrors, e => e?.GetValue<string>() == "forbidden");
    }

    // ── TC20: Delete article with unknown slug ──

    [Fact]
    public async Task DeleteArticleWithUnknownSlug_Returns404()
    {
        var (_, _, _, token) = await RegisterFreshUserAsync();
        var uid = Uid();

        using var authClient = AuthenticatedClient(token);
        var response = await authClient.DeleteAsync($"/api/articles/unknown-slug-{uid}");
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var articleErrors = body["errors"]?["article"]?.AsArray();
        Assert.NotNull(articleErrors);
        Assert.Contains(articleErrors, e => e?.GetValue<string>() == "not found");
    }

    // ── TC21: Get article with unknown slug ──

    [Fact]
    public async Task GetArticleWithUnknownSlug_Returns404()
    {
        var uid = Uid();
        var (response, body) = await GetArticleAsync($"unknown-slug-{uid}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var articleErrors = body["errors"]?["article"]?.AsArray();
        Assert.NotNull(articleErrors);
        Assert.Contains(articleErrors, e => e?.GetValue<string>() == "not found");
    }
}
