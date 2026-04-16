# Auth Login Smoke - Black-Box Test Suite

## Feature Summary

This test suite covers the minimum authentication flow for the RealWorld API:

1. **User Registration** - Create a new user with username, email, and password
2. **User Login** - Authenticate with valid email and password
3. **Login Error Handling** - Validate error responses for invalid inputs

## API Endpoints Under Test

| Endpoint           | Method | Purpose             |
| ------------------ | ------ | ------------------- |
| `/api/users`       | POST   | Register a new user |
| `/api/users/login` | POST   | Authenticate user   |

## Test Scenarios

### Scenario 1: Successful User Registration

**Given** the API server is running
**When** a POST request is sent to `/api/users` with valid user data
**Then** the response status is 201
**And** the response contains user object with:

- `username` matches the registered username
- `email` matches the registered email
- `bio` is null
- `image` is null
- `token` is a non-empty string

### Scenario 2: Successful User Login

**Given** a user is already registered
**When** a POST request is sent to `/api/users/login` with correct credentials
**Then** the response status is 200
**And** the response contains user object with:

- `username` matches the registered username
- `email` matches the registered email
- `bio` is null
- `image` is null
- `token` is a non-empty string

### Scenario 3: Login with Empty Email

**Given** the API server is running
**When** a POST request is sent to `/api/users/login` with empty email
**Then** the response status is 422
**And** the response body contains error message:

- `errors.email[0]` equals "can't be blank"

### Scenario 4: Login with Wrong Password

**Given** a user is already registered
**When** a POST request is sent to `/api/users/login` with incorrect password
**Then** the response status is 401
**And** the response body contains error message:

- `errors.credentials[0]` equals "invalid"

## Request/Response Schemas

### Registration Request

```json
{
  "user": {
    "username": "string",
    "email": "string",
    "password": "string"
  }
}
```

### Login Request

```json
{
  "user": {
    "email": "string",
    "password": "string"
  }
}
```

### Success Response (200/201)

```json
{
  "user": {
    "email": "string",
    "username": "string",
    "bio": "string | null",
    "image": "string | null",
    "token": "string"
  }
}
```

### Error Response (422/401)

```json
{
  "errors": {
    "email": ["can't be blank"],
    "credentials": ["invalid"]
  }
}
```

## Test Design Notes

- Tests are designed to be independent of implementation details
- Each test generates unique user identifiers using timestamps to avoid collisions
- Tests use only the external API contract as defined in the RealWorld spec
- No internal source code or database access is required
- Tests can be run against any RealWorld-compatible implementation
