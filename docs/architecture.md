# Architecture Overview

## High-Level
- Desktop (Electron) hosts the game UI and orchestrates between frontend and backend.
- Frontend (React) provides web UI; also bundled into Electron.
- Backend (Flask) serves APIs and handles AI question generation and storage.
- Database: SQLite for development.

## Data Flow
1. Desktop/Frontend requests a question set from Backend.
2. Backend loads/generates questions (Gemini + fallbacks).
3. Responses are rendered in the Contestant and Game Master UIs.

## Services
- AI Generator: backend/utils/ai_generator.py
- Routes: backend/routes/
- Models: backend/models/

## Build & Deploy
- CI/CD GitHub Actions: tests, builds, Pages deploy, Docker build, Releases.
- Desktop packaged via Electron Forge.
