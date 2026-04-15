---
name: comment-lifecycle
source: realworld
category: backend
complexity: medium
recommended_role: primary
references:
  - upstream/specs/api/bruno/comments/03-create-comment.bru
  - upstream/specs/api/bruno/comments/04-list-comments.bru
  - upstream/specs/api/bruno/comments/05-list-comments-without-auth.bru
  - upstream/specs/api/bruno/comments/06-delete-comment.bru
  - upstream/specs/api/bruno/comments/07-verify-deletion.bru
  - upstream/specs/api/bruno/comments/08-selective-deletion-create-two-comments-delete-one-verify-the-other-remains.bru
  - upstream/specs/api/bruno/comments/12-verify-only-the-second-comment-remains.bru
  - upstream/specs/api/bruno/errors-comments/01-post-comment-no-auth.bru
  - upstream/specs/api/bruno/errors-comments/02-delete-comment-no-auth.bru
---

# Comment Lifecycle

## Purpose

This feature slice covers comment creation, listing, deletion, and selective deletion behavior for article comments.

## Main user-visible behavior

An authenticated user can add comments to an article. Comments can be listed on the article and deleted later. Listing is visible even without authentication. When one of multiple comments is deleted, unrelated comments must remain.

## Inputs

- target article slug
- comment body
- authentication token for create/delete
- comment identifier for deletion

## Core rules

### Comment creation

- posting a comment requires authentication
- valid comment creation succeeds with status 201
- the returned comment includes an integer id, body, timestamps, and author username

### Listing comments

- comments for an article can be listed successfully
- comment listing works without authentication

### Deletion

- deleting a comment requires authentication
- deleting an existing owned comment succeeds
- after deletion, the deleted comment no longer appears in later listing
- when two comments exist and only one is deleted, the other comment remains visible

## Black-box test dimensions

- valid create flow
- visible structure of returned comment payload
- unauthenticated create rejection
- unauthenticated delete rejection
- list visibility with and without auth
- deletion persistence
- selective deletion correctness

## Acceptance criteria

- valid comment creation returns 201 and a comment payload with id, body, timestamps, and author
- comment list can be retrieved successfully
- comment list is accessible without auth
- unauthenticated create is rejected
- unauthenticated delete is rejected
- deleted comments do not appear in later listings
- if two comments exist, deleting one leaves the other intact

## Why this is a strong benchmark feature

This slice tests nested-resource behavior, state transitions, persistence, and selective side effects, making it stronger than a simple one-request auth flow.
