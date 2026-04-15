---
name: settings-null-fields
source: realworld
category: backend
complexity: medium
recommended_role: primary
references:
  - upstream/specs/api/bruno/auth/06-update-user-bio-to-empty-string-should-normalize-to-null.bru
  - upstream/specs/api/bruno/auth/07-verify-empty-string-normalization-persisted.bru
  - upstream/specs/api/bruno/auth/08-restore-bio-then-set-to-null.bru
  - upstream/specs/api/bruno/auth/09-update-user-bio-to-null-should-accept-for-nullable-field.bru
  - upstream/specs/api/bruno/auth/10-verify-null-bio-persisted.bru
  - upstream/specs/api/bruno/auth/12-update-user-image.bru
  - upstream/specs/api/bruno/auth/14-update-image-to-empty-string-should-normalize-to-null.bru
  - upstream/specs/api/bruno/auth/15-verify-image-empty-string-normalization-persisted.bru
  - upstream/specs/api/bruno/auth/16-set-image-then-update-to-null-should-accept-for-nullable-field.bru
  - upstream/specs/api/bruno/auth/18-verify-null-image-persisted.bru
  - upstream/specs/api/bruno/errors-auth/12-update-email-to-empty-string-should-reject.bru
  - upstream/specs/api/bruno/errors-auth/13-update-username-to-empty-string-should-reject.bru
  - upstream/specs/api/bruno/errors-auth/14-update-email-to-null-should-reject.bru
  - upstream/specs/api/bruno/errors-auth/15-update-username-to-null-should-reject.bru
---

# Settings Null Fields

## Purpose

This feature slice covers profile update semantics for nullable and non-nullable user fields, with emphasis on empty-string normalization and null handling.

## Main user-visible behavior

An authenticated user can update profile fields. Some profile fields are nullable and accept null-like states, while others are required identity fields and must reject empty or null values.

## Field classes

### Nullable fields

- bio
- image

### Non-nullable fields

- username
- email

## Core rules

### Nullable field behavior

- updating bio to an empty string succeeds and normalizes to null
- updating bio to null succeeds
- updating image to an empty string succeeds and normalizes to null
- updating image to null succeeds
- normalization and null assignments must persist when the user profile is fetched again

### Non-nullable field behavior

- updating email to an empty string is rejected
- updating username to an empty string is rejected
- updating email to null is rejected
- updating username to null is rejected

## Black-box test dimensions

- nullable vs non-nullable field distinction
- normalization of empty string to null
- explicit null acceptance for nullable fields
- rejection of invalid null/empty values on required fields
- persistence of normalized values across later fetches

## Acceptance criteria

- bio empty string normalizes to null
- bio null is accepted
- image empty string normalizes to null
- image null is accepted
- normalized/null values persist across later reads
- email empty string is rejected
- username empty string is rejected
- email null is rejected
- username null is rejected

## Why this is a strong benchmark feature

This slice is subtle and valuable because it tests semantics that are easy to implement incorrectly but remain fully observable through the external contract.
