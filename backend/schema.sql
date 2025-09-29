-- Who Wants to Be a Millionaire Trivia Game Database Schema
-- SQLite compatible schema

-- Categories table for organizing questions
CREATE TABLE categories (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Difficulty levels table
CREATE TABLE difficulty_levels (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    level_name VARCHAR(50) NOT NULL UNIQUE,
    level_order INTEGER NOT NULL UNIQUE,
    prize_amount INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Main trivia questions table
CREATE TABLE trivia_questions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    question_text TEXT NOT NULL,
    correct_answer TEXT NOT NULL,
    incorrect_answer_1 TEXT NOT NULL,
    incorrect_answer_2 TEXT NOT NULL,
    incorrect_answer_3 TEXT NOT NULL,
    explanation TEXT,
    category_id INTEGER,
    difficulty_id INTEGER,
    source VARCHAR(100) DEFAULT 'manual',
    is_active BOOLEAN DEFAULT 1,
    times_used INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES categories(id),
    FOREIGN KEY (difficulty_id) REFERENCES difficulty_levels(id)
);

-- Game sessions table for tracking gameplay
CREATE TABLE game_sessions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_token VARCHAR(255) UNIQUE NOT NULL,
    current_question_level INTEGER DEFAULT 1,
    total_winnings INTEGER DEFAULT 0,
    questions_answered INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT 1,
    lifelines_used TEXT DEFAULT '[]', -- JSON array of used lifelines
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP NULL
);

-- Track which questions have been used in each session
CREATE TABLE session_questions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id INTEGER NOT NULL,
    question_id INTEGER NOT NULL,
    question_order INTEGER NOT NULL,
    user_answer TEXT,
    is_correct BOOLEAN,
    answered_at TIMESTAMP,
    FOREIGN KEY (session_id) REFERENCES game_sessions(id),
    FOREIGN KEY (question_id) REFERENCES trivia_questions(id),
    UNIQUE(session_id, question_id)
);

-- Admin users table for question management
CREATE TABLE admin_users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    email VARCHAR(100) UNIQUE,
    is_active BOOLEAN DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for better performance
CREATE INDEX idx_trivia_questions_category ON trivia_questions(category_id);
CREATE INDEX idx_trivia_questions_difficulty ON trivia_questions(difficulty_id);
CREATE INDEX idx_trivia_questions_active ON trivia_questions(is_active);
CREATE INDEX idx_game_sessions_token ON game_sessions(session_token);
CREATE INDEX idx_game_sessions_active ON game_sessions(is_active);
CREATE INDEX idx_session_questions_session ON session_questions(session_id);

-- Insert default difficulty levels (Who Wants to Be a Millionaire style)
INSERT INTO difficulty_levels (level_name, level_order, prize_amount) VALUES
('Easy', 1, 100),
('Easy', 2, 200),
('Easy', 3, 300),
('Easy', 4, 500),
('Easy', 5, 1000),
('Medium', 6, 2000),
('Medium', 7, 4000),
('Medium', 8, 8000),
('Medium', 9, 16000),
('Medium', 10, 32000),
('Hard', 11, 64000),
('Hard', 12, 125000),
('Hard', 13, 250000),
('Hard', 14, 500000),
('Expert', 15, 1000000);

-- Insert default categories
INSERT INTO categories (name, description) VALUES
('General Knowledge', 'Mixed topics and general trivia'),
('Science', 'Physics, Chemistry, Biology, and Earth Sciences'),
('History', 'World history, events, and historical figures'),
('Geography', 'Countries, capitals, landmarks, and physical geography'),
('Entertainment', 'Movies, TV shows, music, and celebrities'),
('Sports', 'Various sports, athletes, and sporting events'),
('Literature', 'Books, authors, and literary works'),
('Technology', 'Computers, internet, and modern technology'),
('Art & Culture', 'Fine arts, museums, and cultural topics'),
('Food & Drink', 'Cuisine, cooking, and beverages');