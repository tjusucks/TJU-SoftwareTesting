---
name: auth-login-smoke
source: realworld
category: backend
complexity: low
recommended_role: smoke
references:
  - upstream/specs/api/bruno/auth/01-register.bru
  - upstream/specs/api/bruno/auth/02-login.bru
  - upstream/specs/api/bruno/errors-auth/07-login-empty-email.bru
  - upstream/specs/api/bruno/errors-auth/09-login-wrong-password.bru
---

# Auth Login Smoke

## Purpose

This feature slice covers the minimum authentication flow needed to verify that a RealWorld implementation is reachable and behaves correctly for basic user registration and login.

## Why this slice exists

Use this slice mainly as:

- a smoke test for environment validation
- a warm-up benchmark task
- a simple baseline for black-box test generation

Do not rely on this slice alone as the main quality benchmark because its behavior is too simple compared with richer RealWorld features.

## Main user-visible behavior

A user can register with a username, email, and password, then log in with valid credentials to obtain authenticated user information.

## Inputs

### Registration

- username
- email
- password

### Login

- email
- password

## Expected behavior

### Registration success

- registering a new user succeeds
- the response contains the created username and email
- nullable profile fields such as bio and image are null by default
- an authentication token is returned

### Login success

- logging in with the correct email and password succeeds
- the response contains the correct username and email
- bio and image are null for a newly registered user
- a non-empty authentication token is returned

### Login validation and error handling

- login with an empty email is rejected with status 422
- the error response includes an email validation message indicating the field cannot be blank
- login with the wrong password is rejected with status 401
- the error response indicates invalid credentials

## Black-box test dimensions

- valid registration
- valid login
- missing required input
- invalid credentials
- response field presence and type checks
- token presence on successful auth

## Acceptance criteria

- a newly registered user can log in successfully
- successful auth returns user identity fields and a token
- empty email on login returns 422 with a validation error
- wrong password returns 401 with an invalid-credentials error

## Notes for evaluation

This feature is intentionally small and should be treated as a smoke or calibration slice, not the main benchmark feature.
