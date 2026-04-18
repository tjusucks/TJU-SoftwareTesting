# Authorization Ownership - Black-Box Test Suite

## Feature Summary

This suite validates ownership authorization with both article and comment resources, including side-effect checks after forbidden operations.

Covered behavior:

- non-owner cannot update another user's article
- article owner cannot delete another user's comment
- comment owner can delete own comment even on another user's article
- forbidden or unauthenticated deletion attempts do not mutate existing comments

## Endpoints Under Test

| Endpoint                         | Method | Purpose                            |
| -------------------------------- | ------ | ---------------------------------- |
| /api/users                       | POST   | register users A, B, C             |
| /api/articles                    | POST   | create article as owner            |
| /api/articles/:slug              | PUT    | verify non-owner update forbidden  |
| /api/articles/:slug/comments     | POST   | create comment                     |
| /api/articles/:slug/comments     | GET    | verify persistence/non-persistence |
| /api/articles/:slug/comments/:id | DELETE | verify ownership and auth checks   |

## Scenarios

1. Non-owner update article must be 403 and article body unchanged
2. Article owner deleting another user's comment must be 403 and comment remains
3. Comment owner deleting own comment on another user's article must succeed
4. Forbidden comment delete attempt must not remove or mutate comments
5. Unauthenticated comment delete must be 401 and comment remains

## Acceptance Criteria Covered

- authenticated but unauthorized operations return 403
- unauthenticated delete returns 401
- failed operations are non-destructive
- ownership is based on comment author for comment deletion, not article owner
