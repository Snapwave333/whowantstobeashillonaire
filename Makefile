.PHONY: help install dev build deploy clean test

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-15s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

install: ## Install all dependencies
	@echo "Installing backend dependencies..."
	cd backend && pip install -r requirements.txt
	@echo "Installing frontend dependencies..."
	cd frontend && npm install

dev: ## Start development servers
	@echo "Starting development environment..."
	@echo "Backend will run on http://localhost:5000"
	@echo "Frontend will run on http://localhost:3000"
	docker-compose -f docker-compose.dev.yml up

build: ## Build production images
	@echo "Building production Docker images..."
	docker-compose build

deploy: ## Deploy to production
	@echo "Deploying to production..."
	docker-compose up -d --build
	@echo "Initializing database..."
	docker-compose exec backend python seed_database.py

test: ## Run tests
	@echo "Running backend tests..."
	cd backend && python test_api.py
	@echo "Running frontend tests..."
	cd frontend && npm test -- --watchAll=false

clean: ## Clean up containers and images
	@echo "Cleaning up Docker containers and images..."
	docker-compose down -v
	docker system prune -f

logs: ## View application logs
	docker-compose logs -f

backup: ## Backup database
	@echo "Creating database backup..."
	docker-compose exec backend sqlite3 instance/trivia_prod.db ".backup backup_$(shell date +%Y%m%d_%H%M%S).db"

restore: ## Restore database (usage: make restore BACKUP=backup_file.db)
	@echo "Restoring database from $(BACKUP)..."
	docker-compose exec backend sqlite3 instance/trivia_prod.db ".restore $(BACKUP)"

health: ## Check application health
	@echo "Checking backend health..."
	curl -f http://localhost:5000/api/health || echo "Backend unhealthy"
	@echo "Checking frontend health..."
	curl -f http://localhost:80/health || echo "Frontend unhealthy"