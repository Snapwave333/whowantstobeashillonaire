# Who Wants to Be a Millionaire - Trivia Game Backend

A comprehensive backend system for a "Who Wants to Be a Millionaire" style trivia game built with Python Flask, SQLAlchemy, and modern web technologies.

## 🚀 Features

- **Complete Game Flow**: Sequential questions with increasing difficulty
- **Admin Panel**: Full CRUD operations for questions and categories
- **AI Question Generation**: Automatic question creation using OpenAI API
- **RSS Import**: Import questions from external RSS feeds and Open Trivia Database
- **Lifelines Support**: 50/50 lifeline implementation with extensibility for more
- **Session Management**: Track game progress and prevent question repeats
- **JWT Authentication**: Secure admin endpoints
- **RESTful API**: Clean, documented API endpoints
- **Database Flexibility**: SQLite for development, easily configurable for production

## 📋 Requirements

- Python 3.8+
- Flask 2.3+
- SQLAlchemy 2.0+
- OpenAI API key (optional, for AI question generation)

## 🛠️ Installation & Setup

### 1. Clone and Navigate
```bash
cd backend
```

### 2. Create Virtual Environment
```bash
python -m venv venv

# Windows
venv\Scripts\activate

# macOS/Linux
source venv/bin/activate
```

### 3. Install Dependencies
```bash
pip install -r requirements.txt
```

### 4. Environment Configuration
Create a `.env` file in the backend directory:
```env
SECRET_KEY=your-secret-key-here
JWT_SECRET_KEY=your-jwt-secret-key
OPENAI_API_KEY=your-openai-api-key-optional
FLASK_ENV=development
DATABASE_URL=sqlite:///trivia_game.db
```

### 5. Initialize Database
```bash
# Create database and seed with sample data
python seed_database.py
```

### 6. Run the Application
```bash
python app.py
```

The API will be available at `http://localhost:5000`

## 📊 Database Schema

### Tables Overview
- **categories**: Question categories (Science, History, etc.)
- **difficulty_levels**: 5 difficulty levels (Easy to Genius)
- **trivia_questions**: Main questions table with answers
- **game_sessions**: Track individual game sessions
- **session_questions**: Track which questions were used in each session
- **admin_users**: Admin authentication

### Key Relationships
- Questions belong to categories and difficulty levels
- Game sessions track used questions to prevent repeats
- Admin users can manage all content

## 🔌 API Endpoints

### Game Endpoints (`/api/game`)

#### Start New Game
```http
POST /api/game/start
Content-Type: application/json

{
  "player_name": "John Doe"
}
```

#### Get Next Question
```http
GET /api/game/question/{session_token}
```

#### Submit Answer
```http
POST /api/game/answer
Content-Type: application/json

{
  "session_token": "abc123",
  "question_id": 1,
  "selected_answer": "Paris"
}
```

#### Use 50/50 Lifeline
```http
POST /api/game/lifeline/fifty-fifty
Content-Type: application/json

{
  "session_token": "abc123",
  "question_id": 1
}
```

#### Get Session Info
```http
GET /api/game/session/{session_token}
```

#### Quit Game
```http
POST /api/game/quit
Content-Type: application/json

{
  "session_token": "abc123"
}
```

### Question Endpoints (`/api/questions`)

#### Get Random Question
```http
GET /api/questions/random?category=Science&difficulty=Medium
```

#### Validate Answer
```http
POST /api/questions/validate
Content-Type: application/json

{
  "question_id": 1,
  "answer": "Paris"
}
```

#### Get Categories
```http
GET /api/questions/categories
```

#### Get Difficulties
```http
GET /api/questions/difficulties
```

#### Get Question Stats
```http
GET /api/questions/stats
```

### Admin Endpoints (`/api/admin`)

#### Admin Login
```http
POST /api/admin/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

#### Create Question (Requires JWT)
```http
POST /api/admin/questions
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "question_text": "What is the capital of France?",
  "correct_answer": "Paris",
  "incorrect_answer_1": "London",
  "incorrect_answer_2": "Berlin",
  "incorrect_answer_3": "Madrid",
  "explanation": "Paris has been the capital of France since 987 AD.",
  "category_id": 1,
  "difficulty_id": 1
}
```

#### Update Question (Requires JWT)
```http
PUT /api/admin/questions/{question_id}
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "question_text": "Updated question text"
}
```

#### Delete Question (Requires JWT)
```http
DELETE /api/admin/questions/{question_id}
Authorization: Bearer {jwt_token}
```

#### List Questions (Requires JWT)
```http
GET /api/admin/questions?page=1&per_page=20&category_id=1
Authorization: Bearer {jwt_token}
```

#### Admin Dashboard (Requires JWT)
```http
GET /api/admin/dashboard
Authorization: Bearer {jwt_token}
```

## 🤖 AI Question Generation

Generate questions using OpenAI API:

```bash
# Generate 5 Science questions at Medium difficulty
python utils/ai_generator.py --category "Science" --difficulty "Medium" --count 5
```

### Programmatic Usage
```python
from utils.ai_generator import AIQuestionGenerator

generator = AIQuestionGenerator()
saved_count = generator.generate_and_save_questions("Science", "Hard", 10)
print(f"Generated {saved_count} questions")
```

## 📡 RSS Import

Import questions from external sources:

### From Open Trivia Database
```bash
python utils/rss_importer.py --source opentdb --amount 20 --difficulty medium
```

### From RSS Feed
```bash
python utils/rss_importer.py --source rss --url "https://example.com/trivia.rss" --category "General Knowledge"
```

### Programmatic Usage
```python
from utils.rss_importer import RSSImporter

importer = RSSImporter()
imported_count = importer.import_from_opentdb(amount=50, difficulty='easy')
print(f"Imported {imported_count} questions")
```

## 🎮 Game Flow

1. **Start Game**: Create session with player name
2. **Question Sequence**: Questions increase in difficulty (Easy → Medium → Hard → Expert → Genius)
3. **Answer Validation**: Submit answers and get immediate feedback
4. **Lifelines**: Use 50/50 to eliminate two wrong answers
5. **Session Tracking**: No question repeats within a session
6. **Game End**: Quit anytime or complete all levels

## 🔧 Configuration

### Environment Variables
- `SECRET_KEY`: Flask secret key for sessions
- `JWT_SECRET_KEY`: JWT token signing key
- `OPENAI_API_KEY`: OpenAI API key for AI generation
- `DATABASE_URL`: Database connection string
- `FLASK_ENV`: Environment (development/production)

### Database Configuration
The system uses SQLite by default but can be configured for PostgreSQL, MySQL, or other databases by updating the `DATABASE_URL`.

## 🧪 Testing

Run the test suite:
```bash
pytest
```

Test specific endpoints:
```bash
# Test game flow
curl -X POST http://localhost:5000/api/game/start -H "Content-Type: application/json" -d '{"player_name":"Test Player"}'

# Test question fetch
curl http://localhost:5000/api/questions/random
```

## 📈 Production Deployment

### Using Gunicorn
```bash
pip install gunicorn
gunicorn -w 4 -b 0.0.0.0:5000 app:app
```

### Environment Setup
1. Set `FLASK_ENV=production`
2. Use a production database (PostgreSQL recommended)
3. Set strong secret keys
4. Configure CORS for your frontend domain
5. Set up proper logging and monitoring

### Database Migration
For production, consider using Flask-Migrate:
```bash
pip install Flask-Migrate
flask db init
flask db migrate -m "Initial migration"
flask db upgrade
```

## 🔒 Security Features

- **JWT Authentication**: Secure admin endpoints
- **Password Hashing**: Bcrypt for admin passwords
- **Input Validation**: Comprehensive request validation
- **CORS Configuration**: Configurable cross-origin requests
- **SQL Injection Protection**: SQLAlchemy ORM prevents SQL injection
- **Rate Limiting**: Ready for rate limiting implementation

## 🎯 Default Admin Credentials

**Username**: `admin`  
**Password**: `admin123`

⚠️ **Important**: Change these credentials in production!

## 📝 Sample Data

The seeding script includes:
- **16 sample questions** across different categories and difficulties
- **10 categories** (Science, History, Geography, etc.)
- **5 difficulty levels** (Easy to Genius)
- **Default admin user**

## 🚀 Extensibility

The system is designed for easy extension:

### Adding New Lifelines
1. Create new endpoint in `game_routes.py`
2. Add lifeline logic to question model
3. Update frontend to use new lifeline

### Adding New Question Sources
1. Create new importer in `utils/` directory
2. Follow the pattern of existing importers
3. Add CLI interface for easy usage

### Adding New Features
- User registration and profiles
- Question difficulty scoring
- Multiplayer support
- Question reporting system
- Advanced analytics

## 🐛 Troubleshooting

### Common Issues

**Database not found**
```bash
python seed_database.py
```

**Import errors**
```bash
pip install -r requirements.txt
```

**OpenAI API errors**
- Check your API key in `.env`
- Ensure you have API credits
- Verify internet connection

**CORS errors**
- Update `CORS_ORIGINS` in config.py
- Check frontend URL configuration

## 📚 API Response Examples

### Successful Question Fetch
```json
{
  "success": true,
  "question": {
    "id": 1,
    "question_text": "What is the capital of France?",
    "answers": ["Paris", "London", "Berlin", "Madrid"],
    "category": "Geography",
    "difficulty": "Easy"
  }
}
```

### Answer Validation
```json
{
  "success": true,
  "correct": true,
  "explanation": "Paris has been the capital of France since 987 AD.",
  "correct_answer": "Paris"
}
```

### Error Response
```json
{
  "success": false,
  "error": "Question not found"
}
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Built with ❤️ for trivia enthusiasts everywhere!**