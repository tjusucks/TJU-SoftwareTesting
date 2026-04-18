package tests

import (
    "bytes"
    "encoding/json"
    "fmt"
    "net/http"
    "net/url"
    "os"
    "testing"
    "time"
)

func getAuthzTestHost() string {
    host := os.Getenv("TEST_HOST")
    if host == "" {
        host = "http://localhost:8080"
    }
    return host
}

func authzUID() string {
    return fmt.Sprintf("authz_%d", time.Now().UnixNano())
}

type registerReq struct {
    User struct {
        Username string `json:"username"`
        Email    string `json:"email"`
        Password string `json:"password"`
    } `json:"user"`
}

type userResp struct {
    User struct {
        Email    string `json:"email"`
        Username string `json:"username"`
        Token    string `json:"token"`
    } `json:"user"`
}

type createArticleReq struct {
    Article struct {
        Title       string   `json:"title"`
        Description string   `json:"description"`
        Body        string   `json:"body"`
        TagList     []string `json:"tagList,omitempty"`
    } `json:"article"`
}

type updateArticleReq struct {
    Article struct {
        Title       string `json:"title,omitempty"`
        Description string `json:"description,omitempty"`
        Body        string `json:"body,omitempty"`
    } `json:"article"`
}

type articleResp struct {
    Article struct {
        Slug string `json:"slug"`
        Body string `json:"body"`
    } `json:"article"`
}

func mustRegisterUser(t *testing.T, host, username, email, password string) string {
    t.Helper()

    req := registerReq{}
    req.User.Username = username
    req.User.Email = email
    req.User.Password = password

    body, err := json.Marshal(req)
    if err != nil {
        t.Fatalf("marshal register request failed: %v", err)
    }

    endpoint, err := url.JoinPath(host, "/api/users")
    if err != nil {
        t.Fatalf("build register URL failed: %v", err)
    }

    resp, err := http.Post(endpoint, "application/json", bytes.NewReader(body))
    if err != nil {
        t.Fatalf("register request failed: %v", err)
    }
    defer resp.Body.Close()

    if resp.StatusCode != http.StatusCreated {
        t.Fatalf("register expected 201, got %d", resp.StatusCode)
    }

    var ur userResp
    if err := json.NewDecoder(resp.Body).Decode(&ur); err != nil {
        t.Fatalf("decode register response failed: %v", err)
    }
    if ur.User.Token == "" {
        t.Fatal("register returned empty token")
    }
    return ur.User.Token
}

func mustCreateArticle(t *testing.T, host, token, title, description, body string) (slug string, originalBody string) {
    t.Helper()

    req := createArticleReq{}
    req.Article.Title = title
    req.Article.Description = description
    req.Article.Body = body
    req.Article.TagList = []string{"authz", "ownership"}

    payload, err := json.Marshal(req)
    if err != nil {
        t.Fatalf("marshal create article request failed: %v", err)
    }

    endpoint, err := url.JoinPath(host, "/api/articles")
    if err != nil {
        t.Fatalf("build create article URL failed: %v", err)
    }

    httpReq, err := http.NewRequest(http.MethodPost, endpoint, bytes.NewReader(payload))
    if err != nil {
        t.Fatalf("build create article request failed: %v", err)
    }
    httpReq.Header.Set("Content-Type", "application/json")
    httpReq.Header.Set("Authorization", "Token "+token)

    resp, err := http.DefaultClient.Do(httpReq)
    if err != nil {
        t.Fatalf("create article request failed: %v", err)
    }
    defer resp.Body.Close()

    if resp.StatusCode != http.StatusCreated {
        t.Fatalf("create article expected 201, got %d", resp.StatusCode)
    }

    var ar articleResp
    if err := json.NewDecoder(resp.Body).Decode(&ar); err != nil {
        t.Fatalf("decode create article response failed: %v", err)
    }
    if ar.Article.Slug == "" {
        t.Fatal("create article returned empty slug")
    }
    return ar.Article.Slug, ar.Article.Body
}

func updateArticleAsUser(t *testing.T, host, token, slug, body string) int {
    t.Helper()

    req := updateArticleReq{}
    req.Article.Body = body

    payload, err := json.Marshal(req)
    if err != nil {
        t.Fatalf("marshal update article request failed: %v", err)
    }

    endpoint, err := url.JoinPath(host, "/api/articles", slug)
    if err != nil {
        t.Fatalf("build update article URL failed: %v", err)
    }

    httpReq, err := http.NewRequest(http.MethodPut, endpoint, bytes.NewReader(payload))
    if err != nil {
        t.Fatalf("build update article request failed: %v", err)
    }
    httpReq.Header.Set("Content-Type", "application/json")
    httpReq.Header.Set("Authorization", "Token "+token)

    resp, err := http.DefaultClient.Do(httpReq)
    if err != nil {
        t.Fatalf("update article request failed: %v", err)
    }
    defer resp.Body.Close()

    return resp.StatusCode
}

func mustGetArticleBody(t *testing.T, host, slug string) string {
    t.Helper()

    endpoint, err := url.JoinPath(host, "/api/articles", slug)
    if err != nil {
        t.Fatalf("build get article URL failed: %v", err)
    }

    resp, err := http.Get(endpoint)
    if err != nil {
        t.Fatalf("get article request failed: %v", err)
    }
    defer resp.Body.Close()

    if resp.StatusCode != http.StatusOK {
        t.Fatalf("get article expected 200, got %d", resp.StatusCode)
    }

    var ar articleResp
    if err := json.NewDecoder(resp.Body).Decode(&ar); err != nil {
        t.Fatalf("decode get article response failed: %v", err)
    }

    return ar.Article.Body
}

func TestAuthorizationOwnership_NonOwnerUpdateMustBe403(t *testing.T) {
    host := getAuthzTestHost()
    uid := authzUID()

    // User A creates article
    userAName := "userA_" + uid
    userAEmail := uid + "_a@test.com"
    userAPass := "password123"
    tokenA := mustRegisterUser(t, host, userAName, userAEmail, userAPass)

    // User B is authenticated non-owner
    userBName := "userB_" + uid
    userBEmail := uid + "_b@test.com"
    userBPass := "password123"
    tokenB := mustRegisterUser(t, host, userBName, userBEmail, userBPass)

    originalBody := "original-body-" + uid
    slug, createdBody := mustCreateArticle(t, host, tokenA, "title-"+uid, "desc-"+uid, originalBody)
    if createdBody != originalBody {
        t.Fatalf("created body mismatch: want %q, got %q", originalBody, createdBody)
    }

    // Non-owner update must be forbidden.
    status := updateArticleAsUser(t, host, tokenB, slug, "hacked-body-"+uid)
    if status != http.StatusForbidden {
        t.Fatalf("non-owner update expected 403, got %d", status)
    }

    // Forbidden attempt must not mutate resource.
    bodyAfter := mustGetArticleBody(t, host, slug)
    if bodyAfter != originalBody {
        t.Fatalf("article body changed after forbidden update, want %q, got %q", originalBody, bodyAfter)
    }
}