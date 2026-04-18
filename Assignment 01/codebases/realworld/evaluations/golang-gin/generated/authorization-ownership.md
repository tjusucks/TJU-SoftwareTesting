# Authorization Ownership - Black-Box Test Suite

## Feature Summary

This suite validates ownership-based authorization for article updates.

- User A creates an article.
- User B (authenticated but non-owner) attempts to update User A's article.
- The operation must be rejected with status 403.
- The original article content must remain unchanged after the forbidden attempt.

## Endpoint Under Test

| Endpoint              | Method | Purpose                                   |
| --------------------- | ------ | ----------------------------------------- |
| `/api/articles`       | POST   | Create article as owner (User A)          |
| `/api/articles/:slug` | PUT    | Non-owner update attempt (User B)         |
| `/api/articles/:slug` | GET    | Verify resource unchanged after rejection |

## Acceptance Criteria Covered

- non-owner update attempt on article returns 403
- forbidden operation does not mutate the original article
