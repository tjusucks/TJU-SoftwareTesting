// Package tests contains black-box tests for the RealWorld API article-lifecycle feature.
// These tests are designed to test only externally visible behavior without
// relying on internal implementation details.
package tests

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"net/url"
	_ "os"
	"strings"
	"testing"
	"time"
)

// // Configuration from environment variables
// func getTestHost() string {
// 	host := os.Getenv("TEST_HOST")
// 	if host == "" {
// 		host = "http://localhost:8080"
// 	}
// 	return host
// }

// // Generate unique identifier for test isolation
// func generateUID() string {
// 	return fmt.Sprintf("test_%d", time.Now().UnixNano())
// }

// ========== Request/Response Types ==========

type Article struct {
	Title        string   `json:"title"`
	Slug         string   `json:"slug"`
	Description  string   `json:"description"`
	Body         string   `json:"body"`
	TagList      []string `json:"tagList"`
	CreatedAt    string   `json:"createdAt"`
	UpdatedAt    string   `json:"updatedAt"`
	Favorited    bool     `json:"favorited"`
	FavoritesCount int    `json:"favoritesCount"`
	Author       Author   `json:"author"`
}

type Author struct {
	Username string `json:"username"`
	Bio      string `json:"bio"`
	Image    string `json:"image"`
}

type ArticleResponse struct {
	Article Article `json:"article"`
}

type ArticleCreateRequest struct {
	Article struct {
		Title       string   `json:"title"`
		Description string   `json:"description"`
		Body        string   `json:"body"`
		TagList     []string `json:"tagList,omitempty"`
	} `json:"article"`
}

type ArticleUpdateRequest struct {
	Article struct {
		Title       string   `json:"title,omitempty"`
		Description string   `json:"description,omitempty"`
		Body        string   `json:"body,omitempty"`
		TagList     []string `json:"tagList,omitempty"`
	} `json:"article"`
}

// type ErrorResponse struct {
// 	Errors map[string]interface{} `json:"errors"`
// }

// func getErrorString(errResp ErrorResponse, key string) string {
// 	if errResp.Errors == nil {
// 		return ""
// 	}
// 	value, ok := errResp.Errors[key]
// 	if !ok {
// 		return ""
// 	}
// 	switch v := value.(type) {
// 	case string:
// 		return v
// 	case []interface{}:
// 		if len(v) > 0 {
// 			if s, ok := v[0].(string); ok {
// 				return s
// 			}
// 		}
// 	}
// 	return ""
// }

// ========== Test Helper Functions ==========

type TestUser struct {
	Username string
	Email    string
	Password string
	Token    string
}

func registerUser(t *testing.T, uid string) TestUser {
	host := getTestHost()
	user := TestUser{
		Username: fmt.Sprintf("user_%s", uid),
		Email:    fmt.Sprintf("%s@test.com", uid),
		Password: "password123",
	}

	reqBody := map[string]interface{}{
		"user": map[string]string{
			"username": user.Username,
			"email":    user.Email,
			"password": user.Password,
		},
	}

	bodyBytes, err := json.Marshal(reqBody)
	if err != nil {
		t.Fatalf("Failed to marshal request body: %v", err)
	}

	endpoint, err := url.JoinPath(host, "/api/users")
	if err != nil {
		t.Fatalf("Failed to construct URL: %v", err)
	}

	resp, err := http.Post(endpoint, "application/json", bytes.NewReader(bodyBytes))
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusCreated {
		t.Fatalf("Registration failed with status %d", resp.StatusCode)
	}

	var userResp struct {
		User struct {
			Token string `json:"token"`
		} `json:"user"`
	}

	if err := json.NewDecoder(resp.Body).Decode(&userResp); err != nil {
		t.Fatalf("Failed to decode response: %v", err)
	}

	user.Token = userResp.User.Token
	return user
}

func createArticle(t *testing.T, token, title, description, body string, tags []string) (string, Article) {
	host := getTestHost()

	reqBody := ArticleCreateRequest{}
	reqBody.Article.Title = title
	reqBody.Article.Description = description
	reqBody.Article.Body = body
	if len(tags) > 0 {
		reqBody.Article.TagList = tags
	}

	bodyBytes, err := json.Marshal(reqBody)
	if err != nil {
		t.Fatalf("Failed to marshal request body: %v", err)
	}

	endpoint, err := url.JoinPath(host, "/api/articles")
	if err != nil {
		t.Fatalf("Failed to construct URL: %v", err)
	}

	req, err := http.NewRequest("POST", endpoint, bytes.NewReader(bodyBytes))
	if err != nil {
		t.Fatalf("Failed to create request: %v", err)
	}
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", fmt.Sprintf("Token %s", token))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusCreated {
		t.Fatalf("Create article failed with status %d", resp.StatusCode)
	}

	var articleResp ArticleResponse
	if err := json.NewDecoder(resp.Body).Decode(&articleResp); err != nil {
		t.Fatalf("Failed to decode response: %v", err)
	}

	return articleResp.Article.Slug, articleResp.Article
}

func getArticle(t *testing.T, slug string) Article {
	host := getTestHost()

	endpoint, err := url.JoinPath(host, "/api/articles", slug)
	if err != nil {
		t.Fatalf("Failed to construct URL: %v", err)
	}

	resp, err := http.Get(endpoint)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		t.Fatalf("Get article failed with status %d", resp.StatusCode)
	}

	var articleResp ArticleResponse
	if err := json.NewDecoder(resp.Body).Decode(&articleResp); err != nil {
		t.Fatalf("Failed to decode response: %v", err)
	}

	return articleResp.Article
}

func updateArticle(t *testing.T, token, slug string, updateReq ArticleUpdateRequest) Article {
	host := getTestHost()

	bodyBytes, err := json.Marshal(updateReq)
	if err != nil {
		t.Fatalf("Failed to marshal request body: %v", err)
	}

	endpoint, err := url.JoinPath(host, "/api/articles", slug)
	if err != nil {
		t.Fatalf("Failed to construct URL: %v", err)
	}

	req, err := http.NewRequest("PUT", endpoint, bytes.NewReader(bodyBytes))
	if err != nil {
		t.Fatalf("Failed to create request: %v", err)
	}
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", fmt.Sprintf("Token %s", token))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		t.Fatalf("Update article failed with status %d", resp.StatusCode)
	}

	var articleResp ArticleResponse
	if err := json.NewDecoder(resp.Body).Decode(&articleResp); err != nil {
		t.Fatalf("Failed to decode response: %v", err)
	}

	return articleResp.Article
}

func deleteArticle(t *testing.T, token, slug string) {
	host := getTestHost()

	endpoint, err := url.JoinPath(host, "/api/articles", slug)
	if err != nil {
		t.Fatalf("Failed to construct URL: %v", err)
	}

	req, err := http.NewRequest("DELETE", endpoint, nil)
	if err != nil {
		t.Fatalf("Failed to create request: %v", err)
	}
	req.Header.Set("Authorization", fmt.Sprintf("Token %s", token))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusNoContent {
		t.Fatalf("Delete article failed with status %d", resp.StatusCode)
	}
}

// ========== TC01: Create Article with Valid Input ==========

func TestCreateArticleSuccess(t *testing.T) {
	host := getTestHost()
	uid := generateUID()

	// Register and get token
	user := registerUser(t, uid)

	// Create article
	reqBody := ArticleCreateRequest{}
	reqBody.Article.Title = fmt.Sprintf("Test Article %s", uid)
	reqBody.Article.Description = "Test description"
	reqBody.Article.Body = "Test body content"

	bodyBytes, err := json.Marshal(reqBody)
	if err != nil {
		t.Fatalf("Failed to marshal request body: %v", err)
	}

	endpoint, err := url.JoinPath(host, "/api/articles")
	if err != nil {
		t.Fatalf("Failed to construct URL: %v", err)
	}

	req, err := http.NewRequest("POST", endpoint, bytes.NewReader(bodyBytes))
	if err != nil {
		t.Fatalf("Failed to create request: %v", err)
	}
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", fmt.Sprintf("Token %s", user.Token))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Assert status code is 201
	if resp.StatusCode != http.StatusCreated {
		t.Errorf("Expected status 201, got %d", resp.StatusCode)
	}

	var articleResp ArticleResponse
	if err := json.NewDecoder(resp.Body).Decode(&articleResp); err != nil {
		t.Fatalf("Failed to decode response: %v", err)
	}

	// Assert title matches
	if articleResp.Article.Title != reqBody.Article.Title {
		t.Errorf("Expected title %q, got %q", reqBody.Article.Title, articleResp.Article.Title)
	}

	// Assert slug is non-empty
	if articleResp.Article.Slug == "" {
		t.Error("Expected slug to be non-empty")
	}

	// Assert description matches
	if articleResp.Article.Description != reqBody.Article.Description {
		t.Errorf("Expected description %q, got %q", reqBody.Article.Description, articleResp.Article.Description)
	}

	// Assert body matches
	if articleResp.Article.Body != reqBody.Article.Body {
		t.Errorf("Expected body %q, got %q", reqBody.Article.Body, articleResp.Article.Body)
	}

	// Assert tagList is present (empty array is acceptable)
	if articleResp.Article.TagList == nil {
		t.Error("Expected tagList to be present")
	}

	// Assert timestamps are present
	if articleResp.Article.CreatedAt == "" {
		t.Error("Expected createdAt to be non-empty")
	}
	if articleResp.Article.UpdatedAt == "" {
		t.Error("Expected updatedAt to be non-empty")
	}

	// Assert favorited is false
	if articleResp.Article.Favorited != false {
		t.Errorf("Expected favorited to be false, got %v", articleResp.Article.Favorited)
	}

	// Assert favoritesCount is 0
	if articleResp.Article.FavoritesCount != 0 {
		t.Errorf("Expected favoritesCount to be 0, got %d", articleResp.Article.FavoritesCount)
	}

	// Assert author username matches
	if articleResp.Article.Author.Username != user.Username {
		t.Errorf("Expected author username %q, got %q", user.Username, articleResp.Article.Author.Username)
	}
}

// ========== TC02: Create Article with Tags ==========

func TestCreateArticleWithTags(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	tags := []string{fmt.Sprintf("tag1_%s", uid), fmt.Sprintf("tag2_%s", uid)}
	slug, article := createArticle(t, user.Token, fmt.Sprintf("Test Article %s", uid), "Test description", "Test body", tags)

	// Assert tagList contains provided tags
	if len(article.TagList) != len(tags) {
		t.Errorf("Expected %d tags, got %d", len(tags), len(article.TagList))
	}

	for _, tag := range tags {
		found := false
		for _, articleTag := range article.TagList {
			if articleTag == tag {
				found = true
				break
			}
		}
		if !found {
			t.Errorf("Expected tag %q in tagList", tag)
		}
	}

	// Cleanup
	deleteArticle(t, user.Token, slug)
}

// ========== TC03: Create Article Without Authentication ==========

func TestCreateArticleNoAuth(t *testing.T) {
	host := getTestHost()

	reqBody := ArticleCreateRequest{}
	reqBody.Article.Title = "Test Article"
	reqBody.Article.Description = "Test description"
	reqBody.Article.Body = "Test body"

	bodyBytes, _ := json.Marshal(reqBody)

	endpoint, _ := url.JoinPath(host, "/api/articles")

	resp, err := http.Post(endpoint, "application/json", bytes.NewReader(bodyBytes))
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 401 for unauthenticated request
	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("Expected status 401, got %d", resp.StatusCode)
	}
}

// ========== TC04: Create Article with Empty Title ==========

func TestCreateArticleEmptyTitle(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	host := getTestHost()
	reqBody := ArticleCreateRequest{}
	reqBody.Article.Title = ""
	reqBody.Article.Description = "test"
	reqBody.Article.Body = "test"

	bodyBytes, _ := json.Marshal(reqBody)

	endpoint, _ := url.JoinPath(host, "/api/articles")

	req, _ := http.NewRequest("POST", endpoint, bytes.NewReader(bodyBytes))
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", fmt.Sprintf("Token %s", user.Token))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 422
	if resp.StatusCode != http.StatusUnprocessableEntity {
		t.Errorf("Expected status 422, got %d", resp.StatusCode)
	}

	var errResp ErrorResponse
	json.NewDecoder(resp.Body).Decode(&errResp)

	titleErr := getErrorString(errResp, "title")
	if titleErr == "" {
		t.Error("Expected title validation error")
	}
}

// ========== TC05: Create Article with Empty Description ==========

func TestCreateArticleEmptyDescription(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	host := getTestHost()
	reqBody := ArticleCreateRequest{}
	reqBody.Article.Title = "test"
	reqBody.Article.Description = ""
	reqBody.Article.Body = "test"

	bodyBytes, _ := json.Marshal(reqBody)

	endpoint, _ := url.JoinPath(host, "/api/articles")

	req, _ := http.NewRequest("POST", endpoint, bytes.NewReader(bodyBytes))
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", fmt.Sprintf("Token %s", user.Token))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 422
	if resp.StatusCode != http.StatusUnprocessableEntity {
		t.Errorf("Expected status 422, got %d", resp.StatusCode)
	}

	var errResp ErrorResponse
	json.NewDecoder(resp.Body).Decode(&errResp)

	descErr := getErrorString(errResp, "description")
	if descErr == "" {
		t.Error("Expected description validation error")
	}
}

// ========== TC06: Create Article with Empty Body ==========

func TestCreateArticleEmptyBody(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	host := getTestHost()
	reqBody := ArticleCreateRequest{}
	reqBody.Article.Title = "test"
	reqBody.Article.Description = "test"
	reqBody.Article.Body = ""

	bodyBytes, _ := json.Marshal(reqBody)

	endpoint, _ := url.JoinPath(host, "/api/articles")

	req, _ := http.NewRequest("POST", endpoint, bytes.NewReader(bodyBytes))
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", fmt.Sprintf("Token %s", user.Token))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 422
	if resp.StatusCode != http.StatusUnprocessableEntity {
		t.Errorf("Expected status 422, got %d", resp.StatusCode)
	}

	var errResp ErrorResponse
	json.NewDecoder(resp.Body).Decode(&errResp)

	bodyErr := getErrorString(errResp, "body")
	if bodyErr == "" {
		t.Error("Expected body validation error")
	}
}

// ========== TC07: Create Duplicate Title Articles ==========

func TestCreateDuplicateTitle(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	title := fmt.Sprintf("Dup Title %s", uid)

	// Create first article
	slug1, _ := createArticle(t, user.Token, title, "first", "first", nil)

	// Create second article with same title
	slug2, _ := createArticle(t, user.Token, title, "second", "second", nil)

	// Assert slugs are different
	if slug1 == slug2 {
		t.Errorf("Expected different slugs for duplicate titles, got %q and %q", slug1, slug2)
	}

	// Cleanup
	deleteArticle(t, user.Token, slug1)
	deleteArticle(t, user.Token, slug2)
}

// ========== TC08: Update Article Body Only ==========

func TestUpdateArticleBodyOnly(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	// Create article with all fields
	originalTitle := fmt.Sprintf("Test Article %s", uid)
	slug, original := createArticle(t, user.Token, originalTitle, "Original description", "Original body", []string{"tag1", "tag2"})

	// Update only body
	updateReq := ArticleUpdateRequest{}
	updateReq.Article.Body = "Updated body content"

	updated := updateArticle(t, user.Token, slug, updateReq)

	// Assert body is updated
	if updated.Body != "Updated body content" {
		t.Errorf("Expected body to be updated, got %q", updated.Body)
	}

	// Assert other fields are preserved
	if updated.Title != original.Title {
		t.Errorf("Expected title to be preserved, got %q", updated.Title)
	}
	if updated.Description != original.Description {
		t.Errorf("Expected description to be preserved, got %q", updated.Description)
	}
	if len(updated.TagList) != len(original.TagList) {
		t.Errorf("Expected tags to be preserved, got %d tags", len(updated.TagList))
	}

	// Cleanup
	deleteArticle(t, user.Token, slug)
}

// ========== TC09: Update Article Without TagList (Preserve Tags) ==========

func TestUpdateArticleWithoutTagList(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	tags := []string{"preserved_tag1", "preserved_tag2"}
	slug, original := createArticle(t, user.Token, fmt.Sprintf("Test Article %s", uid), "Description", "Body", tags)

	// Update without tagList
	updateReq := ArticleUpdateRequest{}
	updateReq.Article.Body = "Body without touching tags"

	updated := updateArticle(t, user.Token, slug, updateReq)

	// Assert tags are preserved
	if len(updated.TagList) != len(original.TagList) {
		t.Errorf("Expected %d tags, got %d", len(original.TagList), len(updated.TagList))
	}

	for _, tag := range original.TagList {
		found := false
		for _, updatedTag := range updated.TagList {
			if updatedTag == tag {
				found = true
				break
			}
		}
		if !found {
			t.Errorf("Expected tag %q to be preserved", tag)
		}
	}

	// Cleanup
	deleteArticle(t, user.Token, slug)
}

// ========== TC10: Update Article with Empty TagList (Remove Tags) ==========

func TestUpdateArticleEmptyTagList(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	tags := []string{"tag1", "tag2"}
	slug, _ := createArticle(t, user.Token, fmt.Sprintf("Test Article %s", uid), "Description", "Body", tags)

	// Update with empty tagList
	updateReq := ArticleUpdateRequest{}
	updateReq.Article.TagList = []string{}

	updated := updateArticle(t, user.Token, slug, updateReq)

	// Assert all tags are removed
	if len(updated.TagList) != 0 {
		t.Errorf("Expected 0 tags, got %d", len(updated.TagList))
	}

	// Cleanup
	deleteArticle(t, user.Token, slug)
}

// ========== TC11: Update Article with Null TagList (Reject) ==========

func TestUpdateArticleNullTagList(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	slug, _ := createArticle(t, user.Token, fmt.Sprintf("Test Article %s", uid), "Description", "Body", []string{"tag1"})

	host := getTestHost()
	// Update with null tagList - send as explicit null
	updateReq := map[string]interface{}{
		"article": map[string]interface{}{
			"tagList": nil,
		},
	}

	bodyBytes, _ := json.Marshal(updateReq)

	endpoint, _ := url.JoinPath(host, "/api/articles", slug)

	req, _ := http.NewRequest("PUT", endpoint, bytes.NewReader(bodyBytes))
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", fmt.Sprintf("Token %s", user.Token))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 422
	if resp.StatusCode != http.StatusUnprocessableEntity {
		t.Errorf("Expected status 422, got %d", resp.StatusCode)
	}

	// Cleanup
	deleteArticle(t, user.Token, slug)
}

// ========== TC12: Verify Update Persistence ==========

func TestUpdatePersistence(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	slug, _ := createArticle(t, user.Token, fmt.Sprintf("Test Article %s", uid), "Description", "Original body", nil)

	// Update the article
	updateReq := ArticleUpdateRequest{}
	updateReq.Article.Body = "Persisted body content"
	updateArticle(t, user.Token, slug, updateReq)

	// Fetch the article again
	fetched := getArticle(t, slug)

	// Assert the update persisted
	if fetched.Body != "Persisted body content" {
		t.Errorf("Expected body to be persisted, got %q", fetched.Body)
	}

	// Cleanup
	deleteArticle(t, user.Token, slug)
}

// ========== TC13: Delete Article ==========

func TestDeleteArticle(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	slug, _ := createArticle(t, user.Token, fmt.Sprintf("Test Article %s", uid), "Description", "Body", nil)

	// Delete the article
	deleteArticle(t, user.Token, slug)

	// Verify deletion by attempting to get the article
	host := getTestHost()
	endpoint, _ := url.JoinPath(host, "/api/articles", slug)

	resp, err := http.Get(endpoint)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 404
	if resp.StatusCode != http.StatusNotFound {
		t.Errorf("Expected status 404 after deletion, got %d", resp.StatusCode)
	}
}

// ========== TC14: Verify Deletion Persistence ==========

func TestDeleteArticlePersistence(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	slug, _ := createArticle(t, user.Token, fmt.Sprintf("Test Article %s", uid), "Description", "Body", nil)

	// Delete the article
	deleteArticle(t, user.Token, slug)

	// Attempt to get the deleted article multiple times to verify persistence
	for i := 0; i < 3; i++ {
		host := getTestHost()
		endpoint, _ := url.JoinPath(host, "/api/articles", slug)

		resp, err := http.Get(endpoint)
		if err != nil {
			t.Fatalf("Failed to send request: %v", err)
		}
		resp.Body.Close()

		if resp.StatusCode != http.StatusNotFound {
			t.Errorf("Attempt %d: Expected status 404, got %d", i+1, resp.StatusCode)
		}

		time.Sleep(100 * time.Millisecond) // Small delay between checks
	}
}

// ========== Additional Test: Update Without Auth ==========

func TestUpdateArticleNoAuth(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	slug, _ := createArticle(t, user.Token, fmt.Sprintf("Test Article %s", uid), "Description", "Body", nil)

	host := getTestHost()
	updateReq := ArticleUpdateRequest{}
	updateReq.Article.Body = "Updated body"

	bodyBytes, _ := json.Marshal(updateReq)

	endpoint, _ := url.JoinPath(host, "/api/articles", slug)

	req, _ := http.NewRequest("PUT", endpoint, bytes.NewReader(bodyBytes))
	req.Header.Set("Content-Type", "application/json")
	// No Authorization header

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 401
	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("Expected status 401, got %d", resp.StatusCode)
	}

	// Cleanup
	deleteArticle(t, user.Token, slug)
}

// ========== Additional Test: Delete Without Auth ==========

func TestDeleteArticleNoAuth(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	slug, _ := createArticle(t, user.Token, fmt.Sprintf("Test Article %s", uid), "Description", "Body", nil)

	host := getTestHost()
	endpoint, _ := url.JoinPath(host, "/api/articles", slug)

	req, _ := http.NewRequest("DELETE", endpoint, nil)
	// No Authorization header

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 401
	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("Expected status 401, got %d", resp.StatusCode)
	}

	// Cleanup
	deleteArticle(t, user.Token, slug)
}

// ========== Additional Test: Get Non-Existent Article ==========

func TestGetNonExistentArticle(t *testing.T) {
	host := getTestHost()
	slug := fmt.Sprintf("nonexistent-%s", generateUID())

	endpoint, _ := url.JoinPath(host, "/api/articles", slug)

	resp, err := http.Get(endpoint)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 404
	if resp.StatusCode != http.StatusNotFound {
		t.Errorf("Expected status 404, got %d", resp.StatusCode)
	}
}

// ========== Additional Test: Update Non-Existent Article ==========

func TestUpdateNonExistentArticle(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	host := getTestHost()
	slug := fmt.Sprintf("nonexistent-%s", uid)

	updateReq := ArticleUpdateRequest{}
	updateReq.Article.Body = "Updated body"

	bodyBytes, _ := json.Marshal(updateReq)

	endpoint, _ := url.JoinPath(host, "/api/articles", slug)

	req, _ := http.NewRequest("PUT", endpoint, bytes.NewReader(bodyBytes))
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", fmt.Sprintf("Token %s", user.Token))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 404
	if resp.StatusCode != http.StatusNotFound {
		t.Errorf("Expected status 404, got %d", resp.StatusCode)
	}
}

// ========== Additional Test: Delete Non-Existent Article ==========

func TestDeleteNonExistentArticle(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	host := getTestHost()
	slug := fmt.Sprintf("nonexistent-%s", uid)

	endpoint, _ := url.JoinPath(host, "/api/articles", slug)

	req, _ := http.NewRequest("DELETE", endpoint, nil)
	req.Header.Set("Authorization", fmt.Sprintf("Token %s", user.Token))

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 404
	if resp.StatusCode != http.StatusNotFound {
		t.Errorf("Expected status 404, got %d", resp.StatusCode)
	}
}

// ========== Additional Test: Slug Format ==========

func TestSlugFormat(t *testing.T) {
	uid := generateUID()
	user := registerUser(t, uid)

	title := fmt.Sprintf("Test Article %s", uid)
	slug, _ := createArticle(t, user.Token, title, "Description", "Body", nil)

	// Slug should be URL-safe (lowercase, hyphens)
	if strings.Contains(slug, " ") {
		t.Errorf("Slug should not contain spaces: %q", slug)
	}

	// Slug should be derived from title
	if !strings.Contains(strings.ToLower(title), strings.ToLower(strings.Split(slug, "-")[0])) {
		t.Logf("Warning: slug %q does not appear to be derived from title %q", slug, title)
	}

	// Cleanup
	deleteArticle(t, user.Token, slug)
}