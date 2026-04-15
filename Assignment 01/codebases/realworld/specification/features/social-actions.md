---
name: social-actions
source: realworld
category: frontend-e2e
complexity: medium
recommended_role: primary
references:
  - upstream/specs/e2e/social.spec.ts
---

# Social Actions

## Purpose

This feature slice covers profile and social interactions that are visible from the user interface, including follow/unfollow behavior, profile viewing, favorited articles, and feed behavior for followed users.

## Main user-visible behavior

A signed-in user can interact with other users and articles through social actions. These actions affect buttons, profile pages, favorites views, and the personalized feed.

## Core rules

### Follow and unfollow

- a user can follow another user
- after following, the visible button changes to an unfollow state
- a user can unfollow a followed user
- after unfollowing, the visible button changes back to a follow state

### Own profile vs other profile

- viewing your own profile shows your username and an Edit Profile Settings control
- viewing your own profile does not show a Follow button for yourself
- viewing another user's profile shows that user's identity and a Follow button
- another user's profile can display their articles

### Favorited articles

- a user can favorite an article from the article view
- favorited articles should appear in the user's favorited tab on the profile page

### Feed behavior

- after following another user, that user's articles should appear in Your Feed

## Black-box test dimensions

- visible button state transitions
- self-profile vs other-profile behavior
- social state persistence across navigation
- profile tabs and feed filtering
- article visibility after social actions

## Acceptance criteria

- follow changes visible control from Follow to Unfollow
- unfollow changes visible control from Unfollow to Follow
- own profile shows Edit Profile Settings and not Follow
- other user profile shows Follow and article previews
- favorited articles appear in the Favorited tab
- followed users' articles appear in Your Feed

## Why this is a strong benchmark feature

This slice exercises user-visible state transitions and cross-page consistency, making it a useful complement to API-focused backend slices.
