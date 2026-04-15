---
name: article-lifecycle
source: realworld
category: backend
complexity: medium
recommended_role: primary
references:
  - upstream/specs/api/bruno/articles/02-create-article-with-tags.bru
  - upstream/specs/api/bruno/articles/10-update-article-body.bru
  - upstream/specs/api/bruno/articles/12-update-article-without-taglist-tags-should-be-preserved.bru
  - upstream/specs/api/bruno/articles/13-update-article-remove-all-tags-with-empty-array.bru
  - upstream/specs/api/bruno/articles/15-update-article-taglist-null-should-be-rejected.bru
  - upstream/specs/api/bruno/articles/16-delete-article.bru
  - upstream/specs/api/bruno/articles/17-verify-deletion.bru
  - upstream/specs/api/bruno/errors-articles/01-create-article-no-auth.bru
  - upstream/specs/api/bruno/errors-articles/09-create-article-empty-title.bru
  - upstream/specs/api/bruno/errors-articles/10-create-article-empty-description.bru
  - upstream/specs/api/bruno/errors-articles/11-create-article-empty-body.bru
  - upstream/specs/api/bruno/errors-articles/12-duplicate-titles-are-allowed-each-gets-a-unique-slug.bru
---

# Article Lifecycle

## Purpose

This feature slice covers authenticated article creation, update, deletion, and key validation semantics for article fields and tags.

## Main user-visible behavior

An authenticated user can create an article with title, description, body, and tags; later update parts of it; and finally delete it. Changes must persist and be reflected when the article is retrieved again.

## Inputs

- title
- description
- body
- optional tag list
- authentication token
- target article slug for update/delete operations

## Core rules

### Creation

- creating an article requires authentication
- creating an article succeeds with status 201 when valid input is provided
- the returned article includes title, slug, description, body, tag list, timestamps, favorite state, favorite count, and author username
- duplicate titles are allowed, but each created article must still get a unique slug

### Validation

- empty title is rejected
- empty description is rejected
- empty body is rejected
- unauthenticated create attempts are rejected

### Update semantics

- updating article body succeeds for the article owner
- after update, unchanged fields remain unchanged unless explicitly modified
- omitting tagList during update preserves the existing tags
- setting tagList to an empty array removes all tags
- setting tagList to null is rejected
- update persistence must be observable when the article is fetched again

### Deletion

- deleting an existing owned article succeeds
- after deletion, later retrieval of that article should fail or indicate the article no longer exists

## Black-box test dimensions

- valid create flow
- field-level validation failures
- authentication required for create/update/delete
- duplicate title handling with unique slug generation
- partial update behavior
- tag preservation when omitted
- tag removal with empty list
- rejection of null tag list
- persistence after update
- persistence of deletion

## Acceptance criteria

- valid article creation returns 201 and the created article payload
- unauthenticated create is rejected
- empty title, description, and body are rejected
- duplicate titles are allowed, but slugs remain unique
- updating only body changes body while preserving other fields
- omitting tagList on update preserves existing tags
- empty tagList removes all tags
- null tagList is rejected
- deleting the article removes it from later retrieval

## Why this is a strong benchmark feature

This slice is richer than simple auth flows because it combines CRUD behavior, validation, persistence, partial update semantics, and edge-case handling in one feature family.
