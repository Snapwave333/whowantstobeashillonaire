# API Reference (Overview)

Base URL (dev): http://localhost:5000

## Endpoints
- GET /api/health — service health
- GET /api/questions — fetch question set (query: difficulty, category)
- POST /api/questions/generate — trigger AI generation
- GET /api/admin/stats — basic metrics (protected)

## Errors
- 400 Bad Request — invalid parameters
- 500 Internal Server Error — unexpected failure (see backend logs)

## Notes
- Authentication not required for local dev; add as needed for production.
