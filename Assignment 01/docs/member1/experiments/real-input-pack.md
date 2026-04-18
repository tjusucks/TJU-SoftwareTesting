# Real Input Pack (RealWorld + TodoMVC)

## Source references

- RealWorld API spec:
  - https://github.com/realworld-apps/realworld
  - https://raw.githubusercontent.com/realworld-apps/realworld/main/specs/api/openapi.yml
- RealWorld Node/Express implementation context:
  - https://github.com/gothinkster/node-express-realworld-example-app
- TodoMVC app specification:
  - https://github.com/tastejs/todomvc
  - https://raw.githubusercontent.com/tastejs/todomvc/master/app-spec.md

## RequirementInputV1

```yaml
project_name: "realworld-node-express-plus-todomvc"
feature_name: "core api and ui behavior black-box testing"
actors:
  - "anonymous user"
  - "authenticated user"
  - "article author"
preconditions:
  - "realworld api server is reachable"
  - "test data can be created and cleaned"
  - "for protected endpoints, valid token must be provided"
business_rules:
  - "registration must enforce unique username and email"
  - "protected resources require authorization token"
  - "only owner can update or delete own article/comment"
  - "todo app routing supports all, active, completed states"
  - "todo edits save on blur/enter and cancel on escape"
input_constraints:
  - "pagination limit >= 1 and offset >= 0"
  - "required request bodies must include user/article/comment wrapper"
  - "todo title is trimmed and cannot be empty on create/update"
error_conditions:
  - "401 unauthorized when token missing or invalid"
  - "403 forbidden for non-owner mutation"
  - "404 not found for missing article/profile/comment"
  - "422 validation error for malformed or missing required fields"
  - "409 conflict on duplicate user identity data"
requirement_items:
  - id: "R1"
    text: "POST /users registers user and returns token; duplicate username or email must fail with conflict or validation error."
    priority: "high"
  - id: "R2"
    text: "POST /users/login authenticates existing user; invalid credentials must fail and not return token."
    priority: "high"
  - id: "R3"
    text: "GET/PUT /user require authorization token; missing token returns unauthorized; update should persist supported fields."
    priority: "high"
  - id: "R4"
    text: "POST /articles creates article with required title, description, and body; malformed payload should fail validation."
    priority: "high"
  - id: "R5"
    text: "PUT/DELETE /articles/{slug} allow only article owner; non-owner gets forbidden; missing slug returns not found."
    priority: "high"
  - id: "R6"
    text: "POST/DELETE /profiles/{username}/follow and /articles/{slug}/favorite require auth and must toggle follow/favorite state correctly."
    priority: "medium"
  - id: "R7"
    text: "GET /articles supports filters (tag/author/favorited) and pagination (limit, offset) with valid boundaries."
    priority: "medium"
  - id: "R8"
    text: "POST /articles/{slug}/comments creates comment for existing article; DELETE comment enforces ownership and returns proper errors."
    priority: "high"
  - id: "R9"
    text: "TodoMVC: creating a todo trims input and ignores empty text; Enter creates item and clears input."
    priority: "medium"
  - id: "R10"
    text: "TodoMVC: editing saves on blur/enter, escape cancels, empty edited title deletes item."
    priority: "medium"
  - id: "R11"
    text: "TodoMVC: mark-all checkbox synchronizes with item states; clear-completed removes completed items and updates UI state."
    priority: "medium"
  - id: "R12"
    text: "TodoMVC routing supports #/, #/active, #/completed and updates filtered list and selected state consistently."
    priority: "medium"
```
