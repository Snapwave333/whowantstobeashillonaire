#!/usr/bin/env python3
"""
Database seeding script for the Who Wants to Be a Millionaire trivia game.
This script populates the database with initial data including categories,
difficulty levels, sample trivia questions, and an admin user.
"""

import os
import sys
from datetime import datetime
from werkzeug.security import generate_password_hash

# Add the backend directory to the Python path
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from app import create_app
from database import db
from models.models import Category, DifficultyLevel, TriviaQuestion, AdminUser

def seed_categories():
    """Seed the categories table with initial data."""
    categories = [
        {'name': 'General Knowledge', 'description': 'Mixed topics and general trivia'},
        {'name': 'Science', 'description': 'Physics, Chemistry, Biology, and Earth Sciences'},
        {'name': 'History', 'description': 'World history, ancient civilizations, and historical events'},
        {'name': 'Geography', 'description': 'Countries, capitals, landmarks, and physical geography'},
        {'name': 'Sports', 'description': 'Various sports, athletes, and sporting events'},
        {'name': 'Entertainment', 'description': 'Movies, TV shows, music, and celebrities'},
        {'name': 'Literature', 'description': 'Books, authors, and literary works'},
        {'name': 'Technology', 'description': 'Computers, internet, and modern technology'},
        {'name': 'Art', 'description': 'Paintings, sculptures, and famous artists'},
        {'name': 'Mathematics', 'description': 'Numbers, equations, and mathematical concepts'}
    ]
    
    for cat_data in categories:
        existing = Category.query.filter_by(name=cat_data['name']).first()
        if not existing:
            category = Category(**cat_data)
            db.session.add(category)
    
    db.session.commit()
    print("✓ Categories seeded successfully")

def seed_difficulty_levels():
    """Seed the difficulty levels table with initial data."""
    difficulties = [
        {'level_order': 1, 'level_name': 'Easy', 'prize_amount': 1000},
        {'level_order': 2, 'level_name': 'Medium', 'prize_amount': 5000},
        {'level_order': 3, 'level_name': 'Hard', 'prize_amount': 25000},
        {'level_order': 4, 'level_name': 'Expert', 'prize_amount': 100000},
        {'level_order': 5, 'level_name': 'Genius', 'prize_amount': 1000000}
    ]
    
    for diff_data in difficulties:
        existing = DifficultyLevel.query.filter_by(level_name=diff_data['level_name']).first()
        if not existing:
            difficulty = DifficultyLevel(**diff_data)
            db.session.add(difficulty)
    
    db.session.commit()
    print("✓ Difficulty levels seeded successfully")

def seed_trivia_questions():
    """Seed the trivia questions table with sample questions."""
    
    # Get category and difficulty IDs
    general_cat = Category.query.filter_by(name='General Knowledge').first()
    science_cat = Category.query.filter_by(name='Science').first()
    history_cat = Category.query.filter_by(name='History').first()
    geography_cat = Category.query.filter_by(name='Geography').first()
    sports_cat = Category.query.filter_by(name='Sports').first()
    entertainment_cat = Category.query.filter_by(name='Entertainment').first()
    
    easy_diff = DifficultyLevel.query.filter_by(level_name='Easy').first()
    medium_diff = DifficultyLevel.query.filter_by(level_name='Medium').first()
    hard_diff = DifficultyLevel.query.filter_by(level_name='Hard').first()
    expert_diff = DifficultyLevel.query.filter_by(level_name='Expert').first()
    
    sample_questions = [
        # Easy Questions
        {
            'question_text': 'What is the capital of France?',
            'correct_answer': 'Paris',
            'incorrect_answer_1': 'London',
            'incorrect_answer_2': 'Berlin',
            'incorrect_answer_3': 'Madrid',
            'explanation': 'Paris has been the capital of France since 987 AD.',
            'category_id': geography_cat.id,
            'difficulty_id': easy_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'How many legs does a spider have?',
            'correct_answer': '8',
            'incorrect_answer_1': '6',
            'incorrect_answer_2': '10',
            'incorrect_answer_3': '12',
            'explanation': 'All spiders are arachnids and have 8 legs.',
            'category_id': science_cat.id,
            'difficulty_id': easy_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'What color do you get when you mix red and blue?',
            'correct_answer': 'Purple',
            'incorrect_answer_1': 'Green',
            'incorrect_answer_2': 'Orange',
            'incorrect_answer_3': 'Yellow',
            'explanation': 'Red and blue are primary colors that combine to make purple.',
            'category_id': general_cat.id,
            'difficulty_id': easy_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'Which planet is known as the Red Planet?',
            'correct_answer': 'Mars',
            'incorrect_answer_1': 'Venus',
            'incorrect_answer_2': 'Jupiter',
            'incorrect_answer_3': 'Saturn',
            'explanation': 'Mars appears red due to iron oxide (rust) on its surface.',
            'category_id': science_cat.id,
            'difficulty_id': easy_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'How many minutes are in an hour?',
            'correct_answer': '60',
            'incorrect_answer_1': '50',
            'incorrect_answer_2': '70',
            'incorrect_answer_3': '100',
            'explanation': 'There are 60 minutes in one hour.',
            'category_id': general_cat.id,
            'difficulty_id': easy_diff.id,
            'source': 'seed'
        },
        
        # Medium Questions
        {
            'question_text': 'Who wrote the novel "Pride and Prejudice"?',
            'correct_answer': 'Jane Austen',
            'incorrect_answer_1': 'Charlotte Brontë',
            'incorrect_answer_2': 'Emily Dickinson',
            'incorrect_answer_3': 'Virginia Woolf',
            'explanation': 'Jane Austen published Pride and Prejudice in 1813.',
            'category_id': general_cat.id,
            'difficulty_id': medium_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'What is the chemical symbol for gold?',
            'correct_answer': 'Au',
            'incorrect_answer_1': 'Go',
            'incorrect_answer_2': 'Gd',
            'incorrect_answer_3': 'Ag',
            'explanation': 'Au comes from the Latin word "aurum" meaning gold.',
            'category_id': science_cat.id,
            'difficulty_id': medium_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'In which year did World War II end?',
            'correct_answer': '1945',
            'incorrect_answer_1': '1944',
            'incorrect_answer_2': '1946',
            'incorrect_answer_3': '1943',
            'explanation': 'World War II ended in 1945 with the surrender of Japan.',
            'category_id': history_cat.id,
            'difficulty_id': medium_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'Which country has the most natural lakes?',
            'correct_answer': 'Canada',
            'incorrect_answer_1': 'Russia',
            'incorrect_answer_2': 'United States',
            'incorrect_answer_3': 'Finland',
            'explanation': 'Canada has over 2 million lakes, more than any other country.',
            'category_id': geography_cat.id,
            'difficulty_id': medium_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'How many players are on a basketball team on the court at one time?',
            'correct_answer': '5',
            'incorrect_answer_1': '6',
            'incorrect_answer_2': '7',
            'incorrect_answer_3': '4',
            'explanation': 'Each basketball team has 5 players on the court during play.',
            'category_id': sports_cat.id,
            'difficulty_id': medium_diff.id,
            'source': 'seed'
        },
        
        # Hard Questions
        {
            'question_text': 'What is the smallest country in the world?',
            'correct_answer': 'Vatican City',
            'incorrect_answer_1': 'Monaco',
            'incorrect_answer_2': 'San Marino',
            'incorrect_answer_3': 'Liechtenstein',
            'explanation': 'Vatican City is only 0.17 square miles (0.44 square kilometers).',
            'category_id': geography_cat.id,
            'difficulty_id': hard_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'Who directed the movie "Pulp Fiction"?',
            'correct_answer': 'Quentin Tarantino',
            'incorrect_answer_1': 'Martin Scorsese',
            'incorrect_answer_2': 'Steven Spielberg',
            'incorrect_answer_3': 'Francis Ford Coppola',
            'explanation': 'Quentin Tarantino wrote and directed Pulp Fiction in 1994.',
            'category_id': entertainment_cat.id,
            'difficulty_id': hard_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'What is the hardest natural substance on Earth?',
            'correct_answer': 'Diamond',
            'incorrect_answer_1': 'Quartz',
            'incorrect_answer_2': 'Titanium',
            'incorrect_answer_3': 'Graphite',
            'explanation': 'Diamond rates 10 on the Mohs hardness scale.',
            'category_id': science_cat.id,
            'difficulty_id': hard_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'Which ancient wonder of the world was located in Alexandria?',
            'correct_answer': 'The Lighthouse of Alexandria',
            'incorrect_answer_1': 'The Colossus of Rhodes',
            'incorrect_answer_2': 'The Hanging Gardens of Babylon',
            'incorrect_answer_3': 'The Temple of Artemis',
            'explanation': 'The Lighthouse of Alexandria was one of the Seven Wonders of the Ancient World.',
            'category_id': history_cat.id,
            'difficulty_id': hard_diff.id,
            'source': 'seed'
        },
        
        # Expert Questions
        {
            'question_text': 'What is the rarest blood type?',
            'correct_answer': 'Rh-null',
            'incorrect_answer_1': 'AB-negative',
            'incorrect_answer_2': 'O-negative',
            'incorrect_answer_3': 'Duffy-negative',
            'explanation': 'Rh-null blood type is extremely rare, found in fewer than 50 people worldwide.',
            'category_id': science_cat.id,
            'difficulty_id': expert_diff.id,
            'source': 'seed'
        },
        {
            'question_text': 'Which composer wrote "The Four Seasons"?',
            'correct_answer': 'Antonio Vivaldi',
            'incorrect_answer_1': 'Johann Sebastian Bach',
            'incorrect_answer_2': 'Wolfgang Amadeus Mozart',
            'incorrect_answer_3': 'Ludwig van Beethoven',
            'explanation': 'Antonio Vivaldi composed "The Four Seasons" around 1720.',
            'category_id': entertainment_cat.id,
            'difficulty_id': expert_diff.id,
            'source': 'seed'
        }
    ]
    
    for question_data in sample_questions:
        # Check if question already exists
        existing = TriviaQuestion.query.filter_by(
            question_text=question_data['question_text']
        ).first()
        
        if not existing:
            question = TriviaQuestion(**question_data)
            db.session.add(question)
    
    db.session.commit()
    print(f"✓ {len(sample_questions)} trivia questions seeded successfully")

def seed_admin_user():
    """Create a default admin user."""
    admin_data = {
        'username': 'admin',
        'password_hash': generate_password_hash('admin123'),
        'email': 'admin@triviagame.com'
    }
    
    existing = AdminUser.query.filter_by(username=admin_data['username']).first()
    if not existing:
        admin = AdminUser(**admin_data)
        db.session.add(admin)
        db.session.commit()
        print("✓ Default admin user created (username: admin, password: admin123)")
    else:
        print("✓ Admin user already exists")

def main():
    """Main seeding function."""
    print("🌱 Starting database seeding...")
    
    app = create_app()
    with app.app_context():
        # Create all tables
        db.create_all()
        print("✓ Database tables created")
        
        # Seed data in order
        seed_categories()
        seed_difficulty_levels()
        seed_trivia_questions()
        seed_admin_user()
        
        print("\n🎉 Database seeding completed successfully!")
        print("\nDatabase Statistics:")
        print(f"  - Categories: {Category.query.count()}")
        print(f"  - Difficulty Levels: {DifficultyLevel.query.count()}")
        print(f"  - Trivia Questions: {TriviaQuestion.query.count()}")
        print(f"  - Admin Users: {AdminUser.query.count()}")
        
        print("\nDefault Admin Credentials:")
        print("  Username: admin")
        print("  Password: admin123")
        print("  (Please change these in production!)")

if __name__ == '__main__':
    main()