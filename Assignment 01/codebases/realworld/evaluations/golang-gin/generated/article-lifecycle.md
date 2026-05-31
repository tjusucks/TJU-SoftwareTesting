# Black-Box Test Generation Report: article-lifecycle

## Feature Summary

This feature covers authenticated article creation, update, deletion, and validation semantics for article fields and tags in the RealWorld API.

## Requirements Extracted

### Creation
- Creating an article requires authentication
- Creating an article succeeds with status 201 when valid input is provided
- The returned article includes: title, slug, description, body, tag list, timestamps, favorite state, favorite count, and author username
- Duplicate titles are allowed, but each created article must get a unique slug

### Validation
- Empty title is rejected (422)
- Empty description is rejected (422)
- Empty body is rejected (422)
- Unauthenticated create attempts are rejected (401)

### Update Semantics
- Updating article body succeeds for the article owner (200)
- After update, unchanged fields remain unchanged unless explicitly modified
- Omitting tagList during update preserves the existing tags
- Setting tagList to an empty array removes all tags
- Setting tagList to null is rejected (422)
- Update persistence must be observable when the article is fetched again

### Deletion
- Deleting an existing owned article succeeds (204)
- After deletion, later retrieval of that article should fail or indicate the article no longer exists (404)

## Test Design Strategy

Used black-box testing techniques:
- **Equivalence Partitioning**: Valid inputs, empty inputs, missing inputs, null inputs
- **Boundary Value Analysis**: Empty strings, null values, array boundaries
- **State Transition Testing**: Creation -> Update -> Deletion lifecycle
- **Decision Table Testing**: TagList handling (omit, empty array, null)
- **Error Guessing**: Authentication failures, resource not found

## Test Scenarios

| ID | Scenario | Requirement Reference |
|----|----------|----------------------|
| TC01 | Create article with valid input | Creation |
| TC02 | Create article with tags | Creation |
| TC03 | Create article without authentication | Validation |
| TC04 | Create article with empty title | Validation |
| TC05 | Create article with empty description | Validation |
| TC06 | Create article with empty body | Validation |
| TC07 | Create duplicate title articles | Creation |
| TC08 | Update article body only | Update Semantics |
| TC09 | Update article without tagList (preserve tags) | Update Semantics |
| TC10 | Update article with empty tagList (remove tags) | Update Semantics |
| TC11 | Update article with null tagList (reject) | Update Semantics |
| TC12 | Verify update persistence | Update Semantics |
| TC13 | Delete article | Deletion |
| TC14 | Verify deletion persistence | Deletion |

## Detailed Test Cases

### TC01: Create Article with Valid Input
- **Preconditions**: User is authenticated with valid token
- **Test Data**: title="Test Article", description="Test description", body="Test body"
- **Steps**: POST /api/articles with valid article payload
- **Expected Result**: Status 201, response contains article with title, slug, description, body, tagList, timestamps, favorited=false, favoritesCount=0, author.username

### TC02: Create Article with Tags
- **Preconditions**: User is authenticated with valid token
- **Test Data**: title="Test Article", tagList=["tag1", "tag2"]
- **Steps**: POST /api/articles with tagList
- **Expected Result**: Status 201, tagList contains provided tags

### TC03: Create Article Without Authentication
- **Preconditions**: None
- **Test Data**: Valid article payload, no Authorization header
- **Steps**: POST /api/articles without auth header
- **Expected Result**: Status 401 (unauthorized)

### TC04: Create Article with Empty Title
- **Preconditions**: User is authenticated
- **Test Data**: title="", description="test", body="test"
- **Steps**: POST /api/articles with empty title
- **Expected Result**: Status 422, errors.title contains "can't be blank"

### TC05: Create Article with Empty Description
- **Preconditions**: User is authenticated
- **Test Data**: title="test", description="", body="test"
- **Steps**: POST /api/articles with empty description
- **Expected Result**: Status 422, errors.description contains "can't be blank"

### TC06: Create Article with Empty Body
- **Preconditions**: User is authenticated
- **Test Data**: title="test", description="test", body=""
- **Steps**: POST /api/articles with empty body
- **Expected Result**: Status 422, errors.body contains "can't be blank"

### TC07: Create Duplicate Title Articles
- **Preconditions**: User is authenticated
- **Test Data**: Same title used twice
- **Steps**: Create article with title X, then create another article with same title X
- **Expected Result**: Both succeed with 201, each gets unique slug

### TC08: Update Article Body Only
- **Preconditions**: Article exists and user owns it
- **Test Data**: article.body="Updated body"
- **Steps**: PUT /api/articles/{slug} with only body field
- **Expected Result**: Status 200, body is updated, other fields unchanged

### TC09: Update Article Without TagList (Preserve Tags)
- **Preconditions**: Article exists with tags
- **Test Data**: article.body="New body", no tagList field
- **Steps**: PUT /api/articles/{slug} without tagList
- **Expected Result**: Status 200, existing tags preserved

### TC10: Update Article with Empty TagList (Remove Tags)
- **Preconditions**: Article exists with tags
- **Test Data**: article.tagList=[]
- **Steps**: PUT /api/articles/{slug} with empty tagList
- **Expected Result**: Status 200, all tags removed

### TC11: Update Article with Null TagList (Reject)
- **Preconditions**: Article exists
- **Test Data**: article.tagList=null
- **Steps**: PUT /api/articles/{slug} with null tagList
- **Expected Result**: Status 422

### TC12: Verify Update Persistence
- **Preconditions**: Article was updated
- **Test Data**: Previously updated article slug
- **Steps**: GET /api/articles/{slug}
- **Expected Result**: Returns updated article with new values

### TC13: Delete Article
- **Preconditions**: Article exists and user owns it
- **Test Data**: Existing article slug
- **Steps**: DELETE /api/articles/{slug}
- **Expected Result**: Status 204

### TC14: Verify Deletion Persistence
- **Preconditions**: Article was deleted
- **Test Data**: Deleted article slug
- **Steps**: GET /api/articles/{slug}
- **Expected Result**: Status 404

## Coverage Summary

| Requirement | Covered By |
|-------------|------------|
| Create requires auth | TC03 |
| Create success 201 | TC01, TC02 |
| Response includes all fields | TC01 |
| Duplicate titles with unique slugs | TC07 |
| Empty title rejected | TC04 |
| Empty description rejected | TC05 |
| Empty body rejected | TC06 |
| Update body success | TC08 |
| Unchanged fields preserved | TC08 |
| tagList omitted preserves tags | TC09 |
| tagList empty removes tags | TC10 |
| tagList null rejected | TC11 |
| Update persistence | TC12 |
| Delete success | TC13 |
| Deletion persistence | TC14 |

## Ambiguities / Missing Information / Assumptions

1. **Ownership**: Specification mentions "for the article owner" but doesn't specify behavior for non-owners. Assumed: 403 Forbidden for non-owners.
2. **Error response format**: Assumed standard RealWorld format: `{"errors": {"field": ["message"]}}`
3. **Slug generation**: Assumed URL-safe slug generated from title with unique suffix for duplicates.
4. **API Base URL**: Assumed configurable via TEST_HOST environment variable, defaulting to http://localhost:8080
5. **Authentication**: Assumed Bearer token format "Token {token}" based on API specs.

## Generated Files

- `article_lifecycle_test.go` - Main test file with all test cases
- `helpers.go` - Helper functions for API calls and test utilities