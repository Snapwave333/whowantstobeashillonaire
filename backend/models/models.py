from datetime import datetime
from flask_sqlalchemy import SQLAlchemy
from sqlalchemy import func
import json
import random

# Import db from database module to avoid circular imports
from database import db

class Category(db.Model):
    """Category model for organizing trivia questions."""
    __tablename__ = 'categories'
    
    id = db.Column(db.Integer, primary_key=True)
    name = db.Column(db.String(100), nullable=False, unique=True)
    description = db.Column(db.Text)
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    
    # Relationship
    questions = db.relationship('TriviaQuestion', backref='category', lazy=True)
    
    def to_dict(self):
        return {
            'id': self.id,
            'name': self.name,
            'description': self.description,
            'question_count': len(self.questions)
        }

class DifficultyLevel(db.Model):
    """Difficulty level model for question progression."""
    __tablename__ = 'difficulty_levels'
    
    id = db.Column(db.Integer, primary_key=True)
    level_name = db.Column(db.String(50), nullable=False, unique=True)
    level_order = db.Column(db.Integer, nullable=False, unique=True)
    prize_amount = db.Column(db.Integer, default=0)
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    
    # Relationship
    questions = db.relationship('TriviaQuestion', backref='difficulty', lazy=True)
    
    def to_dict(self):
        return {
            'id': self.id,
            'level_name': self.level_name,
            'level_order': self.level_order,
            'prize_amount': self.prize_amount
        }

class TriviaQuestion(db.Model):
    """Main trivia question model."""
    __tablename__ = 'trivia_questions'
    
    id = db.Column(db.Integer, primary_key=True)
    question_text = db.Column(db.Text, nullable=False)
    correct_answer = db.Column(db.Text, nullable=False)
    incorrect_answer_1 = db.Column(db.Text, nullable=False)
    incorrect_answer_2 = db.Column(db.Text, nullable=False)
    incorrect_answer_3 = db.Column(db.Text, nullable=False)
    explanation = db.Column(db.Text)
    category_id = db.Column(db.Integer, db.ForeignKey('categories.id'))
    difficulty_id = db.Column(db.Integer, db.ForeignKey('difficulty_levels.id'))
    source = db.Column(db.String(100), default='manual')
    is_active = db.Column(db.Boolean, default=True)
    times_used = db.Column(db.Integer, default=0)
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    updated_at = db.Column(db.DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    
    def get_shuffled_answers(self):
        """Return all answers in random order with correct answer marked."""
        answers = [
            {'text': self.correct_answer, 'is_correct': True},
            {'text': self.incorrect_answer_1, 'is_correct': False},
            {'text': self.incorrect_answer_2, 'is_correct': False},
            {'text': self.incorrect_answer_3, 'is_correct': False}
        ]
        random.shuffle(answers)
        return answers
    
    def get_all_answers(self):
        """Return all answers as a list."""
        return [
            self.correct_answer,
            self.incorrect_answer_1,
            self.incorrect_answer_2,
            self.incorrect_answer_3
        ]
    
    def is_answer_correct(self, answer):
        """Check if provided answer is correct."""
        return answer.strip().lower() == self.correct_answer.strip().lower()
    
    def to_dict(self, include_correct=False):
        """Convert to dictionary for API responses."""
        data = {
            'id': self.id,
            'question_text': self.question_text,
            'answers': self.get_shuffled_answers() if include_correct else [ans['text'] for ans in self.get_shuffled_answers()],
            'category': self.category.name if self.category else None,
            'difficulty': self.difficulty.level_name if self.difficulty else None,
            'explanation': self.explanation,
            'times_used': self.times_used
        }
        
        if include_correct:
            data['correct_answer'] = self.correct_answer
            
        return data
    
    @classmethod
    def get_random_question(cls, difficulty_level=None, category_id=None, exclude_ids=None):
        """Get a random question with optional filters."""
        query = cls.query.filter(cls.is_active == True)
        
        if difficulty_level:
            query = query.join(DifficultyLevel).filter(DifficultyLevel.level_order == difficulty_level)
        
        if category_id:
            query = query.filter(cls.category_id == category_id)
        
        if exclude_ids:
            query = query.filter(~cls.id.in_(exclude_ids))
        
        return query.order_by(func.random()).first()

class GameSession(db.Model):
    """Game session model for tracking gameplay."""
    __tablename__ = 'game_sessions'
    
    id = db.Column(db.Integer, primary_key=True)
    session_token = db.Column(db.String(255), unique=True, nullable=False)
    current_question_level = db.Column(db.Integer, default=1)
    total_winnings = db.Column(db.Integer, default=0)
    questions_answered = db.Column(db.Integer, default=0)
    is_active = db.Column(db.Boolean, default=True)
    lifelines_used = db.Column(db.Text, default='[]')  # JSON array
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    ended_at = db.Column(db.DateTime)
    
    # Relationship
    session_questions = db.relationship('SessionQuestion', backref='session', lazy=True, cascade='all, delete-orphan')
    
    def get_lifelines_used(self):
        """Get list of used lifelines."""
        try:
            return json.loads(self.lifelines_used)
        except:
            return []
    
    def use_lifeline(self, lifeline_type):
        """Mark a lifeline as used."""
        lifelines = self.get_lifelines_used()
        if lifeline_type not in lifelines:
            lifelines.append(lifeline_type)
            self.lifelines_used = json.dumps(lifelines)
    
    def get_used_question_ids(self):
        """Get list of question IDs already used in this session."""
        return [sq.question_id for sq in self.session_questions]
    
    def end_session(self):
        """End the game session."""
        self.is_active = False
        self.ended_at = datetime.utcnow()
    
    def to_dict(self):
        return {
            'id': self.id,
            'session_token': self.session_token,
            'current_question_level': self.current_question_level,
            'total_winnings': self.total_winnings,
            'questions_answered': self.questions_answered,
            'is_active': self.is_active,
            'lifelines_used': self.get_lifelines_used(),
            'created_at': self.created_at.isoformat() if self.created_at else None,
            'ended_at': self.ended_at.isoformat() if self.ended_at else None
        }

class SessionQuestion(db.Model):
    """Track questions used in each session."""
    __tablename__ = 'session_questions'
    
    id = db.Column(db.Integer, primary_key=True)
    session_id = db.Column(db.Integer, db.ForeignKey('game_sessions.id'), nullable=False)
    question_id = db.Column(db.Integer, db.ForeignKey('trivia_questions.id'), nullable=False)
    question_order = db.Column(db.Integer, nullable=False)
    user_answer = db.Column(db.Text)
    is_correct = db.Column(db.Boolean)
    answered_at = db.Column(db.DateTime)
    
    # Relationships
    question = db.relationship('TriviaQuestion', backref='session_uses')
    
    __table_args__ = (db.UniqueConstraint('session_id', 'question_id'),)
    
    def to_dict(self):
        return {
            'id': self.id,
            'question_id': self.question_id,
            'question_order': self.question_order,
            'user_answer': self.user_answer,
            'is_correct': self.is_correct,
            'answered_at': self.answered_at.isoformat() if self.answered_at else None,
            'question': self.question.to_dict() if self.question else None
        }

class AdminUser(db.Model):
    """Admin user model for question management."""
    __tablename__ = 'admin_users'
    
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(50), unique=True, nullable=False)
    password_hash = db.Column(db.String(255), nullable=False)
    email = db.Column(db.String(100), unique=True)
    is_active = db.Column(db.Boolean, default=True)
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    
    def to_dict(self):
        return {
            'id': self.id,
            'username': self.username,
            'email': self.email,
            'is_active': self.is_active,
            'created_at': self.created_at.isoformat() if self.created_at else None
        }