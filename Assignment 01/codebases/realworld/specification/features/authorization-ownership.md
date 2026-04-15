---
name: authorization-ownership
source: realworld
category: backend
complexity: medium
recommended_role: primary
references:
  - upstream/specs/api/bruno/errors-authorization/01-register-user-a.bru
  - upstream/specs/api/bruno/errors-authorization/02-register-user-b.bru
  - upstream/specs/api/bruno/errors-authorization/03-user-a-creates-article.bru
  - upstream/specs/api/bruno/errors-authorization/04-user-b-tries-to-delete-403.bru
  - upstream/specs/api/bruno/errors-authorization/05-user-b-tries-to-update-403.bru
  - upstream/specs/api/bruno/errors-authorization/06-user-a-creates-a-comment-on-the-article.bru
  - upstream/specs/api/bruno/errors-authorization/07-user-b-tries-to-delete-a-s-comment-403.bru
  - upstream/specs/api/bruno/errors-authorization/08-verify-comment-survived-the-failed-delete.bru
  - upstream/specs/api/bruno/errors-authorization/09-cleanup-user-a-deletes-article.bru
---

# Authorization Ownership

## Purpose

This feature slice covers ownership-based authorization rules for article and comment modification and deletion.

## Main user-visible behavior

A resource owner can modify or delete their own content. A different authenticated user must not be able to update or delete someone else’s article or comment.

## Actors

- user A: resource owner
- user B: authenticated non-owner

## Core rules

### Article ownership

- user A can create an article
- user B cannot delete user A's article
- user B cannot update user A's article
- forbidden operations return status 403
- failed forbidden operations do not mutate or remove the original article

### Comment ownership

- user A can create a comment on the article
- user B cannot delete user A's comment
- forbidden comment deletion returns status 403
- after the failed delete attempt, the original comment still exists

## Black-box test dimensions

- cross-user setup
- authenticated but unauthorized actions
- forbidden status code behavior
- resource persistence after failed modification
- distinction between unauthenticated failure and authenticated-but-forbidden failure

## Acceptance criteria

- non-owner delete attempt on article returns 403
- non-owner update attempt on article returns 403
- non-owner delete attempt on comment returns 403
- after forbidden operations, the original article and comment remain intact

## Why this is a strong benchmark feature

This slice is valuable because it tests negative cases, role-sensitive behavior, and non-destructive guarantees after a failed action. These are highly informative for black-box test quality.
