"""
RSS Feed Importer for Trivia Game
Imports trivia questions from RSS feeds and other external sources.
"""

import feedparser
import requests
import re
from typing import List, Dict, Optional
from datetime import datetime
from models.models import db, TriviaQuestion, Category, DifficultyLevel

class RSSImporter:
    """Import trivia questions from RSS feeds and external sources."""
    
    def __init__(self):
        """Initialize the RSS importer."""
        self.session = requests.Session()
        self.session.headers.update({
            'User-Agent': 'TriviaGame-RSS-Importer/1.0'
        })
    
    def fetch_rss_feed(self, url: str) -> Optional[feedparser.FeedParserDict]:
        """
        Fetch and parse an RSS feed.
        
        Args:
            url: RSS feed URL
            
        Returns:
            Parsed feed data or None if failed
        """
        try:
            response = self.session.get(url, timeout=30)
            response.raise_for_status()
            
            feed = feedparser.parse(response.content)
            
            if feed.bozo:
                print(f"Warning: Feed may have parsing issues: {feed.bozo_exception}")
            
            return feed
            
        except Exception as e:
            print(f"Error fetching RSS feed {url}: {str(e)}")
            return None
    
    def parse_trivia_from_feed(self, feed: feedparser.FeedParserDict, 
                              category_name: str = 'General Knowledge',
                              difficulty_name: str = 'Medium') -> List[Dict]:
        """
        Parse trivia questions from RSS feed entries.
        This is a basic implementation that looks for question patterns.
        
        Args:
            feed: Parsed RSS feed
            category_name: Default category for imported questions
            difficulty_name: Default difficulty for imported questions
            
        Returns:
            List of question dictionaries
        """
        questions = []
        
        # Common question patterns to look for
        question_patterns = [
            r'Q:\s*(.+?)\s*A:\s*(.+?)(?=\s*Q:|$)',  # Q: ... A: ... format
            r'Question:\s*(.+?)\s*Answer:\s*(.+?)(?=\s*Question:|$)',  # Question: ... Answer: ... format
            r'(\d+\.\s*.+?\?)\s*(.+?)(?=\s*\d+\.|$)',  # Numbered questions
        ]
        
        for entry in feed.entries:
            title = getattr(entry, 'title', '')
            description = getattr(entry, 'description', '')
            content = getattr(entry, 'content', [{}])
            
            # Combine all text content
            text_content = f"{title} {description}"
            if content and isinstance(content, list) and content[0].get('value'):
                text_content += f" {content[0]['value']}"
            
            # Clean HTML tags
            text_content = re.sub(r'<[^>]+>', '', text_content)
            text_content = re.sub(r'\s+', ' ', text_content).strip()
            
            # Try to extract questions using patterns
            for pattern in question_patterns:
                matches = re.findall(pattern, text_content, re.IGNORECASE | re.DOTALL)
                
                for match in matches:
                    if len(match) >= 2:
                        question_text = match[0].strip()
                        answer_text = match[1].strip()
                        
                        # Basic validation
                        if (len(question_text) > 10 and len(answer_text) > 1 and
                            question_text.endswith('?')):
                            
                            # Create a basic question structure
                            # Note: This creates placeholder incorrect answers
                            # In a real implementation, you'd want better logic here
                            question_data = {
                                'question_text': question_text,
                                'correct_answer': answer_text,
                                'incorrect_answer_1': 'Option A',
                                'incorrect_answer_2': 'Option B', 
                                'incorrect_answer_3': 'Option C',
                                'explanation': f'Imported from RSS feed: {feed.feed.get("title", "Unknown")}',
                                'category_name': category_name,
                                'difficulty_name': difficulty_name,
                                'source': f'rss:{feed.feed.get("link", "unknown")}'
                            }
                            
                            questions.append(question_data)
        
        return questions
    
    def import_from_opentdb_api(self, amount: int = 10, category: int = None, 
                               difficulty: str = None) -> List[Dict]:
        """
        Import questions from Open Trivia Database API.
        
        Args:
            amount: Number of questions to fetch (max 50)
            category: Category ID from OpenTDB
            difficulty: Difficulty level (easy, medium, hard)
            
        Returns:
            List of question dictionaries
        """
        url = "https://opentdb.com/api.php"
        params = {
            'amount': min(amount, 50),
            'type': 'multiple'  # Multiple choice questions
        }
        
        if category:
            params['category'] = category
        if difficulty:
            params['difficulty'] = difficulty
        
        try:
            response = self.session.get(url, params=params, timeout=30)
            response.raise_for_status()
            
            data = response.json()
            
            if data.get('response_code') != 0:
                print(f"OpenTDB API error: {data.get('response_code')}")
                return []
            
            questions = []
            
            for item in data.get('results', []):
                # Decode HTML entities
                import html
                
                question_data = {
                    'question_text': html.unescape(item['question']),
                    'correct_answer': html.unescape(item['correct_answer']),
                    'incorrect_answer_1': html.unescape(item['incorrect_answers'][0]),
                    'incorrect_answer_2': html.unescape(item['incorrect_answers'][1]),
                    'incorrect_answer_3': html.unescape(item['incorrect_answers'][2]),
                    'explanation': f'Imported from Open Trivia Database',
                    'category_name': html.unescape(item['category']),
                    'difficulty_name': item['difficulty'].capitalize(),
                    'source': 'opentdb'
                }
                
                questions.append(question_data)
            
            return questions
            
        except Exception as e:
            print(f"Error importing from OpenTDB: {str(e)}")
            return []
    
    def save_questions_to_db(self, questions: List[Dict]) -> int:
        """
        Save imported questions to the database.
        
        Args:
            questions: List of question dictionaries
            
        Returns:
            Number of questions successfully saved
        """
        saved_count = 0
        
        for q_data in questions:
            try:
                # Get or create category
                category_name = q_data.get('category_name', 'General Knowledge')
                category = Category.query.filter_by(name=category_name).first()
                
                if not category:
                    # Create new category
                    category = Category(
                        name=category_name,
                        description=f'Auto-created from import: {category_name}'
                    )
                    db.session.add(category)
                    db.session.flush()  # Get the ID
                
                # Get difficulty level
                difficulty_name = q_data.get('difficulty_name', 'Medium')
                difficulty = DifficultyLevel.query.filter_by(name=difficulty_name).first()
                
                if not difficulty:
                    print(f"Difficulty '{difficulty_name}' not found, using Medium")
                    difficulty = DifficultyLevel.query.filter_by(name='Medium').first()
                
                # Check if question already exists
                existing = TriviaQuestion.query.filter_by(
                    question_text=q_data['question_text']
                ).first()
                
                if existing:
                    print(f"Question already exists: {q_data['question_text'][:50]}...")
                    continue
                
                # Create new question
                question = TriviaQuestion(
                    question_text=q_data['question_text'],
                    correct_answer=q_data['correct_answer'],
                    incorrect_answer_1=q_data['incorrect_answer_1'],
                    incorrect_answer_2=q_data['incorrect_answer_2'],
                    incorrect_answer_3=q_data['incorrect_answer_3'],
                    explanation=q_data.get('explanation'),
                    category_id=category.id,
                    difficulty_id=difficulty.id if difficulty else None,
                    source=q_data.get('source', 'imported')
                )
                
                db.session.add(question)
                saved_count += 1
                
            except Exception as e:
                print(f"Error saving question: {str(e)}")
                continue
        
        try:
            db.session.commit()
            print(f"Successfully saved {saved_count} imported questions")
            return saved_count
        except Exception as e:
            db.session.rollback()
            print(f"Error committing questions to database: {str(e)}")
            return 0
    
    def import_from_rss_url(self, url: str, category_name: str = 'General Knowledge',
                           difficulty_name: str = 'Medium') -> int:
        """
        Import questions from an RSS feed URL.
        
        Args:
            url: RSS feed URL
            category_name: Category for imported questions
            difficulty_name: Difficulty for imported questions
            
        Returns:
            Number of questions successfully imported
        """
        feed = self.fetch_rss_feed(url)
        if not feed:
            return 0
        
        questions = self.parse_trivia_from_feed(feed, category_name, difficulty_name)
        if not questions:
            print("No questions found in RSS feed")
            return 0
        
        return self.save_questions_to_db(questions)
    
    def import_from_opentdb(self, amount: int = 10, category: int = None,
                           difficulty: str = None) -> int:
        """
        Import questions from Open Trivia Database.
        
        Args:
            amount: Number of questions to import
            category: OpenTDB category ID
            difficulty: Difficulty level
            
        Returns:
            Number of questions successfully imported
        """
        questions = self.import_from_opentdb_api(amount, category, difficulty)
        if not questions:
            return 0
        
        return self.save_questions_to_db(questions)

# Example usage and CLI interface
if __name__ == '__main__':
    import sys
    import argparse
    
    # Add the backend directory to the Python path
    sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    
    from app import app
    
    parser = argparse.ArgumentParser(description='Import trivia questions from external sources')
    parser.add_argument('--source', choices=['rss', 'opentdb'], required=True,
                       help='Import source')
    parser.add_argument('--url', help='RSS feed URL (for RSS import)')
    parser.add_argument('--amount', type=int, default=10,
                       help='Number of questions to import (for OpenTDB)')
    parser.add_argument('--category', help='Category name or OpenTDB category ID')
    parser.add_argument('--difficulty', help='Difficulty level')
    
    args = parser.parse_args()
    
    with app.app_context():
        importer = RSSImporter()
        
        if args.source == 'rss':
            if not args.url:
                print("RSS URL is required for RSS import")
                sys.exit(1)
            
            imported = importer.import_from_rss_url(
                args.url,
                args.category or 'General Knowledge',
                args.difficulty or 'Medium'
            )
            
        elif args.source == 'opentdb':
            category_id = None
            if args.category and args.category.isdigit():
                category_id = int(args.category)
            
            imported = importer.import_from_opentdb(
                args.amount,
                category_id,
                args.difficulty
            )
        
        print(f"Successfully imported {imported} questions")