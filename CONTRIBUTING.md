# Contributing

Thank you for considering contributing to Who Wants to Be a Shillionaire!

## Getting Started
- Fork the repo and create your branch from `main`.
- Run the app locally (see README Quick Start).
- Write clear, atomic commits.

## Development
- Frontend: Node 18, `frontend/`
- Desktop: Node 18, `desktop-app/`
- Backend: Python 3.9+, `backend/`
- Tests: add/update tests where applicable.

## Coding Standards
- Use linters/formatters configured in each package.
- Keep functions small and well-named.
- Add comments only for non-obvious logic and constraints.

## Pull Requests
- Use the PR template.
- Link related issues.
- Include screenshots for UI changes.
- Ensure CI passes.

### Conventional Commits
- Format commit messages as: `<type>(optional scope): <short summary>`
- Common types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`.
- Examples:
  - `feat(frontend): add lifeline UI component`
  - `fix(backend): handle empty question set gracefully`
  - `ci: upload coverage to codecov`

PR titles should follow the same convention. This enables automated changelogs and passes the Semantic PR check.

## Reporting Bugs / Requesting Features
- Use the issue templates.
- Provide steps, environment, and expected vs actual behavior.

## Releases
- Large binaries are never committed. Use GitHub Releases to distribute `.exe` and `.zip`.

## Code of Conduct
- See `CODE_OF_CONDUCT.md`.
