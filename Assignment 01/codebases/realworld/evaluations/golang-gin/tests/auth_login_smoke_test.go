// Package tests contains black-box tests for the RealWorld API auth-login-smoke feature.
// These tests are designed to test only externally visible behavior without
// relying on internal implementation details.
package tests

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"net/url"
	"os"
	"strings"
	"testing"
	"time"
)

// Configuration from environment variables
func getTestHost() string {
	host := os.Getenv("TEST_HOST")
	if host == "" {
		host = "http://localhost:8080"
	}
	return host
}

// Generate unique identifier for test isolation
func generateUID() string {
	return fmt.Sprintf("test_%d", time.Now().UnixNano())
}

// ========== Request/Response Types ==========

type UserRegistrationRequest struct {
	User struct {
		Username string `json:"username"`
		Email    string `json:"email"`
		Password string `json:"password"`
	} `json:"user"`
}

type UserLoginRequest struct {
	User struct {
		Email    string `json:"email"`
		Password string `json:"password"`
	} `json:"user"`
}

type UserResponse struct {
	User struct {
		Email    string `json:"email"`
		Username string `json:"username"`
		Bio      string `json:"bio"`
		Image    string `json:"image"`
		Token    string `json:"token"`
	} `json:"user"`
}

type ErrorResponse struct {
	Errors map[string]interface{} `json:"errors"`
}

func getErrorString(errResp ErrorResponse, key string) string {
	if errResp.Errors == nil {
		return ""
	}
	value, ok := errResp.Errors[key]
	if !ok {
		return ""
	}
	str, ok := value.(string)
	if !ok {
		return ""
	}
	return str
}

// ========== Test: Successful Registration ==========

func TestRegisterSuccess(t *testing.T) {
	host := getTestHost()
	uid := generateUID()

	reqBody := UserRegistrationRequest{}
	reqBody.User.Username = fmt.Sprintf("user_%s", uid)
	reqBody.User.Email = fmt.Sprintf("%s@test.com", uid)
	reqBody.User.Password = "password123"

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

	// Assert status code is 201
	if resp.StatusCode != http.StatusCreated {
		t.Errorf("Expected status 201, got %d", resp.StatusCode)
	}

	var userResp UserResponse
	if err := json.NewDecoder(resp.Body).Decode(&userResp); err != nil {
		t.Fatalf("Failed to decode response: %v", err)
	}

	// Assert username matches
	if userResp.User.Username != reqBody.User.Username {
		t.Errorf("Expected username %q, got %q", reqBody.User.Username, userResp.User.Username)
	}

	// Assert email matches
	if userResp.User.Email != reqBody.User.Email {
		t.Errorf("Expected email %q, got %q", reqBody.User.Email, userResp.User.Email)
	}

	// Current golang-gin baseline returns empty strings for bio and image
	if userResp.User.Bio != "" {
		t.Errorf("Expected bio to be empty string, got %q", userResp.User.Bio)
	}

	if userResp.User.Image != "" {
		t.Errorf("Expected image to be empty string, got %q", userResp.User.Image)
	}

	// Assert token is non-empty
	if userResp.User.Token == "" {
		t.Error("Expected token to be non-empty")
	}
}

// ========== Test: Successful Login ==========

func TestLoginSuccess(t *testing.T) {
	host := getTestHost()
	uid := generateUID()

	// First, register a user
	regReq := UserRegistrationRequest{}
	regReq.User.Username = fmt.Sprintf("user_%s", uid)
	regReq.User.Email = fmt.Sprintf("%s@test.com", uid)
	regReq.User.Password = "password123"

	regBytes, _ := json.Marshal(regReq)
	regEndpoint, _ := url.JoinPath(host, "/api/users")

	resp, err := http.Post(regEndpoint, "application/json", bytes.NewReader(regBytes))
	if err != nil {
		t.Fatalf("Failed to register user: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusCreated {
		t.Fatalf("Registration failed with status %d", resp.StatusCode)
	}

	// Now, login with the same credentials
	loginReq := UserLoginRequest{}
	loginReq.User.Email = regReq.User.Email
	loginReq.User.Password = regReq.User.Password

	loginBytes, _ := json.Marshal(loginReq)
	loginEndpoint, _ := url.JoinPath(host, "/api/users/login")

	loginResp, err := http.Post(loginEndpoint, "application/json", bytes.NewReader(loginBytes))
	if err != nil {
		t.Fatalf("Failed to send login request: %v", err)
	}
	defer loginResp.Body.Close()

	// Assert status code is 200
	if loginResp.StatusCode != http.StatusOK {
		t.Errorf("Expected status 200, got %d", loginResp.StatusCode)
	}

	var userResp UserResponse
	if err := json.NewDecoder(loginResp.Body).Decode(&userResp); err != nil {
		t.Fatalf("Failed to decode response: %v", err)
	}

	// Assert username matches
	if userResp.User.Username != regReq.User.Username {
		t.Errorf("Expected username %q, got %q", regReq.User.Username, userResp.User.Username)
	}

	// Assert email matches
	if userResp.User.Email != regReq.User.Email {
		t.Errorf("Expected email %q, got %q", regReq.User.Email, userResp.User.Email)
	}

	// Assert bio is null for new user
	if userResp.User.Bio != "" {
		t.Errorf("Expected bio to be null, got %v", userResp.User.Bio)
	}

	// Assert image is null for new user
	if userResp.User.Image != "" {
		t.Errorf("Expected image to be null, got %v", userResp.User.Image)
	}

	// Assert token is present and non-empty
	if userResp.User.Token == "" {
		t.Error("Expected token to be non-empty")
	}

	// Assert token type is string
	if !isValidJWT(userResp.User.Token) {
		t.Log("Warning: Token does not appear to be a valid JWT format")
	}
}

// ========== Test: Login with Empty Email ==========

func TestLoginEmptyEmail(t *testing.T) {
	host := getTestHost()

	loginReq := UserLoginRequest{}
	loginReq.User.Email = ""
	loginReq.User.Password = "password123"

	loginBytes, _ := json.Marshal(loginReq)
	loginEndpoint, _ := url.JoinPath(host, "/api/users/login")

	resp, err := http.Post(loginEndpoint, "application/json", bytes.NewReader(loginBytes))
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Assert status code is 422
	if resp.StatusCode != http.StatusUnprocessableEntity {
		t.Errorf("Expected status 422, got %d", resp.StatusCode)
	}

	var errResp ErrorResponse
	if err := json.NewDecoder(resp.Body).Decode(&errResp); err != nil {
		t.Fatalf("Failed to decode error response: %v", err)
	}

	// Current golang-gin baseline returns validator errors as string values in errors map
	emailErr := getErrorString(errResp, "Email")
	if emailErr == "" {
		t.Error("Expected Email validation error to be present")
	} else {
		expectedMsg := "{key: required}"
		if emailErr != expectedMsg {
			t.Errorf("Expected Email error to be %q, got %q", expectedMsg, emailErr)
		}
	}
}

// ========== Test: Login with Wrong Password ==========

func TestLoginWrongPassword(t *testing.T) {
	host := getTestHost()
	uid := generateUID()

	// First, register a user
	regReq := UserRegistrationRequest{}
	regReq.User.Username = fmt.Sprintf("user_%s", uid)
	regReq.User.Email = fmt.Sprintf("%s@test.com", uid)
	regReq.User.Password = "password123"

	regBytes, _ := json.Marshal(regReq)
	regEndpoint, _ := url.JoinPath(host, "/api/users")

	resp, err := http.Post(regEndpoint, "application/json", bytes.NewReader(regBytes))
	if err != nil {
		t.Fatalf("Failed to register user: %v", err)
	}
	resp.Body.Close()

	// Now, attempt login with wrong password
	loginReq := UserLoginRequest{}
	loginReq.User.Email = regReq.User.Email
	loginReq.User.Password = "wrongpassword"

	loginBytes, _ := json.Marshal(loginReq)
	loginEndpoint, _ := url.JoinPath(host, "/api/users/login")

	loginResp, err := http.Post(loginEndpoint, "application/json", bytes.NewReader(loginBytes))
	if err != nil {
		t.Fatalf("Failed to send login request: %v", err)
	}
	defer loginResp.Body.Close()

	// Assert status code is 401
	if loginResp.StatusCode != http.StatusUnauthorized {
		t.Errorf("Expected status 401, got %d", loginResp.StatusCode)
	}

	var errResp ErrorResponse
	if err := json.NewDecoder(loginResp.Body).Decode(&errResp); err != nil {
		t.Fatalf("Failed to decode error response: %v", err)
	}

	// Current golang-gin baseline reports login failures under errors.login
	loginErr := getErrorString(errResp, "login")
	if loginErr == "" {
		t.Error("Expected login error to be present")
	} else {
		expectedMsg := "Not Registered email or invalid password"
		if loginErr != expectedMsg {
			t.Errorf("Expected login error to be %q, got %q", expectedMsg, loginErr)
		}
	}
}

// ========== Test: Login with Non-Existent User ==========

func TestLoginNonExistentUser(t *testing.T) {
	host := getTestHost()
	uid := generateUID()

	loginReq := UserLoginRequest{}
	loginReq.User.Email = fmt.Sprintf("nonexistent_%s@test.com", uid)
	loginReq.User.Password = "password123"

	loginBytes, _ := json.Marshal(loginReq)
	loginEndpoint, _ := url.JoinPath(host, "/api/users/login")

	resp, err := http.Post(loginEndpoint, "application/json", bytes.NewReader(loginBytes))
	if err != nil {
		t.Fatalf("Failed to send request: %v", err)
	}
	defer resp.Body.Close()

	// Should return 401 for non-existent user (same as wrong password for security)
	if resp.StatusCode != http.StatusUnauthorized {
		t.Errorf("Expected status 401 for non-existent user, got %d", resp.StatusCode)
	}
}

// ========== Test: Token is Valid JWT Format ==========

func TestTokenFormat(t *testing.T) {
	host := getTestHost()
	uid := generateUID()

	// Register a user
	regReq := UserRegistrationRequest{}
	regReq.User.Username = fmt.Sprintf("user_%s", uid)
	regReq.User.Email = fmt.Sprintf("%s@test.com", uid)
	regReq.User.Password = "password123"

	regBytes, _ := json.Marshal(regReq)
	regEndpoint, _ := url.JoinPath(host, "/api/users")

	resp, err := http.Post(regEndpoint, "application/json", bytes.NewReader(regBytes))
	if err != nil {
		t.Fatalf("Failed to register user: %v", err)
	}
	defer resp.Body.Close()

	var userResp UserResponse
	json.NewDecoder(resp.Body).Decode(&userResp)

	// Verify token format (JWT has 3 parts separated by dots)
	if !isValidJWT(userResp.User.Token) {
		t.Errorf("Token does not appear to be a valid JWT format: %s", userResp.User.Token[:min(20, len(userResp.User.Token))]+"...")
	}
}

// ========== Helper Functions ==========

func isValidJWT(token string) bool {
	parts := strings.Split(token, ".")
	return len(parts) == 3 && len(parts[0]) > 0 && len(parts[1]) > 0 && len(parts[2]) > 0
}

func min(a, b int) int {
	if a < b {
		return a
	}
	return b
}
