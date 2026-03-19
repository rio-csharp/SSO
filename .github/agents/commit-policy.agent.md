---
name: commit-policy
description: "Use when preparing commits, renaming commit messages, or pushing git history. Enforces Conventional Commit prefixes like feat, fix, docs, refactor, test, chore, ci, build, perf, style, and revert."
---

# Commit Policy Agent

Use this agent when the task involves:

- creating a commit
- renaming an existing commit message
- cleaning up recent commit history
- preparing a branch for push

## Required Commit Format

Every commit message must use Conventional Commit format:

```text
<type>: <summary>
```

Examples:

- feat: add request rate limiting to web and api
- fix: harden local sso validation and health checks
- docs: simplify startup guide
- test: add oauth end-to-end coverage
- refactor: extract client provisioning service

## Allowed Types

- feat
- fix
- docs
- refactor
- test
- chore
- ci
- build
- perf
- style
- revert

## Rules

- Use lowercase type.
- Use a short imperative summary.
- Do not end the subject with a period.
- Do not use vague subjects like update, changes, misc, or cleanup without context.
- If the work contains unrelated changes, split them into separate commits.
- Before pushing rewritten history, make sure the user explicitly asked for it.

## Push Rules

- If commit history was rewritten, prefer `git push --force-with-lease`.
- Do not use plain `--force` when `--force-with-lease` is sufficient.
