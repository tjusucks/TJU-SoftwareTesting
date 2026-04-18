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

func mustRegisterUser(t *testing.T, host, username, email, password string) string {
    t.Helper()

    reqBody := map[string]any{
        "user": map[string]any{
            "username": username,
            "email":    email,
            "password": password,
        },
    }

    body, err := json.Marshal(reqBody)
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

    var ur map[string]any
    if err := json.NewDecoder(resp.Body).Decode(&ur); err != nil {
        t.Fatalf("decode register response failed: %v", err)
    }

    userObj, ok := ur["user"].(map[string]any)
    if !ok {
        t.Fatal("register response missing user object")
    }

    token, ok := userObj["token"].(string)
    if !ok || token == "" {
        t.Fatal("register returned empty token")
    }
    return token
}

func mustCreateArticle(t *testing.T, host, token, title, description, body string) (string, string) {
    t.Helper()

    reqBody := map[string]any{
        "article": map[string]any{
            "title":       title,
            "description": description,
            "body":        body,
            "tagList":     []string{"authz", "ownership"},
        },
    }

    payload, err := json.Marshal(reqBody)
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

    var ar map[string]any
    if err := json.NewDecoder(resp.Body).Decode(&ar); err != nil {
        t.Fatalf("decode create article response failed: %v", err)
    }

    articleObj, ok := ar["article"].(map[string]any)
    if !ok {
        t.Fatal("create article response missing article object")
    }

    slug, ok := articleObj["slug"].(string)
    if !ok || slug == "" {
        t.Fatal("create article returned empty slug")
    }

    createdBody, ok := articleObj["body"].(string)
    if !ok {
        t.Fatal("create article response missing body")
    }

    return slug, createdBody
}

func updateArticleAsUser(t *testing.T, host, token, slug, body string) int {
    t.Helper()

    reqBody := map[string]any{
        "article": map[string]any{
            "body": body,
        },
    }

    payload, err := json.Marshal(reqBody)
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

    var ar map[string]any
    if err := json.NewDecoder(resp.Body).Decode(&ar); err != nil {
        t.Fatalf("decode get article response failed: %v", err)
    }

    articleObj, ok := ar["article"].(map[string]any)
    if !ok {
        t.Fatal("get article response missing article object")
    }

    body, ok := articleObj["body"].(string)
    if !ok {
        t.Fatal("get article response missing body")
    }
    return body
}

func mustCreateComment(t *testing.T, host, token, slug, body string) uint {
    t.Helper()

    reqBody := map[string]any{
        "comment": map[string]any{
            "body": body,
        },
    }

    payload, err := json.Marshal(reqBody)
    if err != nil {
        t.Fatalf("marshal create comment request failed: %v", err)
    }

    endpoint, err := url.JoinPath(host, "/api/articles", slug, "comments")
    if err != nil {
        t.Fatalf("build create comment URL failed: %v", err)
    }

    httpReq, err := http.NewRequest(http.MethodPost, endpoint, bytes.NewReader(payload))
    if err != nil {
        t.Fatalf("build create comment request failed: %v", err)
    }
    httpReq.Header.Set("Content-Type", "application/json")
    httpReq.Header.Set("Authorization", "Token "+token)

    resp, err := http.DefaultClient.Do(httpReq)
    if err != nil {
        t.Fatalf("create comment request failed: %v", err)
    }
    defer resp.Body.Close()

    if resp.StatusCode != http.StatusCreated {
        t.Fatalf("create comment expected 201, got %d", resp.StatusCode)
    }

    var cr map[string]any
    if err := json.NewDecoder(resp.Body).Decode(&cr); err != nil {
        t.Fatalf("decode create comment response failed: %v", err)
    }

    commentObj, ok := cr["comment"].(map[string]any)
    if !ok {
        t.Fatal("create comment response missing comment object")
    }

    idNum, ok := commentObj["id"].(float64)
    if !ok || idNum <= 0 {
        t.Fatal("create comment returned invalid id")
    }
    return uint(idNum)
}

func deleteCommentAsUserStatus(t *testing.T, host, token, slug string, id uint) int {
    t.Helper()

    endpoint, err := url.JoinPath(host, "/api/articles", slug, "comments", fmt.Sprintf("%d", id))
    if err != nil {
        t.Fatalf("build delete comment URL failed: %v", err)
    }

    httpReq, err := http.NewRequest(http.MethodDelete, endpoint, nil)
    if err != nil {
        t.Fatalf("build delete comment request failed: %v", err)
    }
    httpReq.Header.Set("Authorization", "Token "+token)

    resp, err := http.DefaultClient.Do(httpReq)
    if err != nil {
        t.Fatalf("delete comment request failed: %v", err)
    }
    defer resp.Body.Close()

    return resp.StatusCode
}

func deleteCommentWithoutAuthStatus(t *testing.T, host, slug string, id uint) int {
    t.Helper()

    endpoint, err := url.JoinPath(host, "/api/articles", slug, "comments", fmt.Sprintf("%d", id))
    if err != nil {
        t.Fatalf("build delete comment URL failed: %v", err)
    }

    httpReq, err := http.NewRequest(http.MethodDelete, endpoint, nil)
    if err != nil {
        t.Fatalf("build delete comment request failed: %v", err)
    }

    resp, err := http.DefaultClient.Do(httpReq)
    if err != nil {
        t.Fatalf("delete comment request failed: %v", err)
    }
    defer resp.Body.Close()

    return resp.StatusCode
}

func mustListComments(t *testing.T, host, slug string) []map[string]any {
    t.Helper()

    endpoint, err := url.JoinPath(host, "/api/articles", slug, "comments")
    if err != nil {
        t.Fatalf("build list comments URL failed: %v", err)
    }

    resp, err := http.Get(endpoint)
    if err != nil {
        t.Fatalf("list comments request failed: %v", err)
    }
    defer resp.Body.Close()

    if resp.StatusCode != http.StatusOK {
        t.Fatalf("list comments expected 200, got %d", resp.StatusCode)
    }

    var lr map[string]any
    if err := json.NewDecoder(resp.Body).Decode(&lr); err != nil {
        t.Fatalf("decode list comments response failed: %v", err)
    }

    raw, ok := lr["comments"].([]any)
    if !ok {
        t.Fatal("list comments response missing comments")
    }

    out := make([]map[string]any, 0, len(raw))
    for _, one := range raw {
        obj, ok := one.(map[string]any)
        if ok {
            out = append(out, obj)
        }
    }
    return out
}

func commentsContainsID(comments []map[string]any, id uint) bool {
    for _, c := range comments {
        idNum, ok := c["id"].(float64)
        if ok && uint(idNum) == id {
            return true
        }
    }
    return false
}

func TestAuthorizationOwnership_NonOwnerUpdateMustBe403(t *testing.T) {
    host := getAuthzTestHost()
    uid := authzUID()

    userAName := "userA_" + uid
    userAEmail := uid + "_a@test.com"
    tokenA := mustRegisterUser(t, host, userAName, userAEmail, "password123")

    userBName := "userB_" + uid
    userBEmail := uid + "_b@test.com"
    tokenB := mustRegisterUser(t, host, userBName, userBEmail, "password123")

    originalBody := "original-body-" + uid
    slug, createdBody := mustCreateArticle(t, host, tokenA, "title-"+uid, "desc-"+uid, originalBody)
    if createdBody != originalBody {
        t.Fatalf("created body mismatch: want %q, got %q", originalBody, createdBody)
    }

    status := updateArticleAsUser(t, host, tokenB, slug, "hacked-body-"+uid)
    if status != http.StatusForbidden {
        t.Fatalf("non-owner update expected 403, got %d", status)
    }

    bodyAfter := mustGetArticleBody(t, host, slug)
    if bodyAfter != originalBody {
        t.Fatalf("article body changed after forbidden update, want %q, got %q", originalBody, bodyAfter)
    }
}

func TestAuthorizationOwnership_ArticleOwnerCannotDeleteOthersComment(t *testing.T) {
    host := getAuthzTestHost()
    uid := authzUID()

    tokenA := mustRegisterUser(t, host, "owner_"+uid, uid+"_owner@test.com", "password123")
    tokenB := mustRegisterUser(t, host, "commenter_"+uid, uid+"_commenter@test.com", "password123")

    slug, _ := mustCreateArticle(t, host, tokenA, "article-"+uid, "desc-"+uid, "body-"+uid)
    commentID := mustCreateComment(t, host, tokenB, slug, "comment-by-b-"+uid)

    status := deleteCommentAsUserStatus(t, host, tokenA, slug, commentID)
    if status != http.StatusForbidden {
        t.Fatalf("article owner deleting other's comment expected 403, got %d", status)
    }

    comments := mustListComments(t, host, slug)
    if !commentsContainsID(comments, commentID) {
        t.Fatalf("comment %d disappeared after forbidden delete", commentID)
    }
}

func TestAuthorizationOwnership_CommentOwnerCanDeleteOwnCommentOnOthersArticle(t *testing.T) {
    host := getAuthzTestHost()
    uid := authzUID()

    tokenA := mustRegisterUser(t, host, "owner2_"+uid, uid+"_owner2@test.com", "password123")
    tokenB := mustRegisterUser(t, host, "commenter2_"+uid, uid+"_commenter2@test.com", "password123")

    slug, _ := mustCreateArticle(t, host, tokenA, "article2-"+uid, "desc2-"+uid, "body2-"+uid)
    commentID := mustCreateComment(t, host, tokenB, slug, "comment-by-b2-"+uid)

    status := deleteCommentAsUserStatus(t, host, tokenB, slug, commentID)
    if status != http.StatusOK {
        t.Fatalf("comment owner delete own comment expected 200, got %d", status)
    }

    comments := mustListComments(t, host, slug)
    if commentsContainsID(comments, commentID) {
        t.Fatalf("comment %d still exists after successful owner delete", commentID)
    }
}

func TestAuthorizationOwnership_NonDestructiveAfterForbiddenCommentDelete(t *testing.T) {
    host := getAuthzTestHost()
    uid := authzUID()

    tokenA := mustRegisterUser(t, host, "owner3_"+uid, uid+"_owner3@test.com", "password123")
    tokenB := mustRegisterUser(t, host, "commenter3_"+uid, uid+"_commenter3@test.com", "password123")
    tokenC := mustRegisterUser(t, host, "intruder3_"+uid, uid+"_intruder3@test.com", "password123")

    slug, _ := mustCreateArticle(t, host, tokenA, "article3-"+uid, "desc3-"+uid, "body3-"+uid)
    commentID := mustCreateComment(t, host, tokenB, slug, "comment-by-b3-"+uid)

    before := mustListComments(t, host, slug)
    status := deleteCommentAsUserStatus(t, host, tokenC, slug, commentID)
    if status != http.StatusForbidden {
        t.Fatalf("non-owner delete comment expected 403, got %d", status)
    }
    after := mustListComments(t, host, slug)

    if !commentsContainsID(after, commentID) {
        t.Fatalf("comment %d disappeared after forbidden delete", commentID)
    }
    if len(after) < len(before) {
        t.Fatalf("comment list shrank after forbidden delete, before=%d after=%d", len(before), len(after))
    }
}

func TestAuthorizationOwnership_UnauthenticatedDeleteCommentMustBe401(t *testing.T) {
    host := getAuthzTestHost()
    uid := authzUID()

    tokenA := mustRegisterUser(t, host, "owner4_"+uid, uid+"_owner4@test.com", "password123")
    tokenB := mustRegisterUser(t, host, "commenter4_"+uid, uid+"_commenter4@test.com", "password123")

    slug, _ := mustCreateArticle(t, host, tokenA, "article4-"+uid, "desc4-"+uid, "body4-"+uid)
    commentID := mustCreateComment(t, host, tokenB, slug, "comment-by-b4-"+uid)

    status := deleteCommentWithoutAuthStatus(t, host, slug, commentID)
    if status != http.StatusUnauthorized {
        t.Fatalf("unauthenticated delete comment expected 401, got %d", status)
    }

    comments := mustListComments(t, host, slug)
    if !commentsContainsID(comments, commentID) {
        t.Fatalf("comment %d disappeared after unauthenticated delete", commentID)
    }
}