"""
AI Question Generator for Trivia Game
Uses OpenAI API to generate trivia questions with specified categories and difficulties.
"""

import os
import json
import openai
from typing import Dict, List, Optional
from models.models import db, TriviaQuestion, Category, DifficultyLevel

class AIQuestionGenerator:
    """Generate trivia questions using OpenAI API."""
    
    def __init__(self, api_key: Optional[str] = None):
        """Initialize the AI question generator."""
        self.api_key = api_key or os.getenv('OPENAI_API_KEY')
        if self.api_key:
            openai.api_key = self.api_key
        else:
            print("Warning: OpenAI API key not found. AI generation will not work.")
    
    def generate_question(self, category: str, difficulty: str, count: int = 1) -> List[Dict]:
        """
        Generate trivia questions using OpenAI API.
        
        Args:
            category: The category for the questions
            difficulty: The difficulty level (Easy, Medium, Hard, Expert, Genius)
            count: Number of questions to generate
            
        Returns:
            List of question dictionaries
        """
        if not self.api_key:
            raise ValueError("OpenAI API key is required for AI generation")
        
        # Define difficulty descriptions
        difficulty_descriptions = {
            'Easy': 'basic knowledge that most people would know',
            'Medium': 'moderate difficulty requiring some specific knowledge',
            'Hard': 'challenging questions requiring specialized knowledge',
            'Expert': 'very difficult questions for subject matter experts',
            'Genius': 'extremely challenging questions that would stump most experts'
        }
        
        difficulty_desc = difficulty_descriptions.get(difficulty, 'moderate difficulty')
        
        prompt = f"""
Generate {count} trivia question(s) for the category "{category}" with {difficulty_desc}.

For each question, provide:
1. A clear, well-written question
2. One correct answer
3. Three plausible but incorrect answers
4. A brief explanation of why the correct answer is right

Format the response as a JSON array with this structure:
[
  {{
    "question_text": "Your question here?",
    "correct_answer": "Correct answer",
    "incorrect_answer_1": "Wrong answer 1",
    "incorrect_answer_2": "Wrong answer 2", 
    "incorrect_answer_3": "Wrong answer 3",
    "explanation": "Brief explanation of the correct answer"
  }}
]

Requirements:
- Questions should be factual and verifiable
- Incorrect answers should be plausible but clearly wrong
- Avoid questions that could be offensive or controversial
- Make sure the difficulty matches the "{difficulty}" level
- Keep questions concise but clear
- Explanations should be 1-2 sentences maximum
"""

        try:
            response = openai.ChatCompletion.create(
                model="gpt-3.5-turbo",
                messages=[
                    {"role": "system", "content": "You are a trivia question generator. Generate high-quality, accurate trivia questions in the requested format."},
                    {"role": "user", "content": prompt}
                ],
                max_tokens=1500,
                temperature=0.7
            )
            
            content = response.choices[0].message.content.strip()
            
            # Try to parse the JSON response
            try:
                questions = json.loads(content)
                if not isinstance(questions, list):
                    questions = [questions]
                return questions
            except json.JSONDecodeError:
                # If JSON parsing fails, try to extract JSON from the response
                import re
                json_match = re.search(r'\[.*\]', content, re.DOTALL)
                if json_match:
                    questions = json.loads(json_match.group())
                    return questions
                else:
                    raise ValueError("Could not parse AI response as JSON")
                    
        except Exception as e:
            print(f"Error generating questions with AI: {str(e)}")
            return []
    
    def generate_and_save_questions(self, category_name: str, difficulty_name: str, count: int = 5) -> int:
        """
        Generate questions and save them to the database.
        
        Args:
            category_name: Name of the category
            difficulty_name: Name of the difficulty level
            count: Number of questions to generate
            
        Returns:
            Number of questions successfully saved
        """
        # Get category and difficulty from database
        category = Category.query.filter_by(name=category_name).first()
        difficulty = DifficultyLevel.query.filter_by(name=difficulty_name).first()
        
        if not category:
            print(f"Category '{category_name}' not found in database")
            return 0
        
        if not difficulty:
            print(f"Difficulty '{difficulty_name}' not found in database")
            return 0
        
        # Generate questions
        questions_data = self.generate_question(category_name, difficulty_name, count)
        
        if not questions_data:
            print("No questions generated")
            return 0
        
        saved_count = 0
        
        for q_data in questions_data:
            try:
                # Validate required fields
                required_fields = ['question_text', 'correct_answer', 'incorrect_answer_1', 
                                 'incorrect_answer_2', 'incorrect_answer_3']
                
                if not all(field in q_data for field in required_fields):
                    print(f"Skipping question due to missing fields: {q_data}")
                    continue
                
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
                    difficulty_id=difficulty.id,
                    source='ai_generated'
                )
                
                db.session.add(question)
                saved_count += 1
                
            except Exception as e:
                print(f"Error saving question: {str(e)}")
                continue
        
        try:
            db.session.commit()
            print(f"Successfully saved {saved_count} AI-generated questions")
            return saved_count
        except Exception as e:
            db.session.rollback()
            print(f"Error committing questions to database: {str(e)}")
            return 0
    
    def bulk_generate_questions(self, generation_plan: List[Dict]) -> Dict[str, int]:
        """
        Generate multiple sets of questions based on a plan.
        
        Args:
            generation_plan: List of dicts with 'category', 'difficulty', and 'count' keys
            
        Returns:
            Dictionary with results for each category/difficulty combination
        """
        results = {}
        
        for plan in generation_plan:
            category = plan.get('category')
            difficulty = plan.get('difficulty')
            count = plan.get('count', 5)
            
            key = f"{category}_{difficulty}"
            print(f"Generating {count} {difficulty} questions for {category}...")
            
            saved = self.generate_and_save_questions(category, difficulty, count)
            results[key] = saved
        
        return results

# Example usage and CLI interface
if __name__ == '__main__':
    import sys
    import argparse
    
    # Add the backend directory to the Python path
    sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    
    from app import app
    
    parser = argparse.ArgumentParser(description='Generate trivia questions using AI')
    parser.add_argument('--category', required=True, help='Category name')
    parser.add_argument('--difficulty', required=True, help='Difficulty level')
    parser.add_argument('--count', type=int, default=5, help='Number of questions to generate')
    parser.add_argument('--api-key', help='OpenAI API key (or set OPENAI_API_KEY env var)')
    
    args = parser.parse_args()
    
    with app.app_context():
        generator = AIQuestionGenerator(api_key=args.api_key)
        saved = generator.generate_and_save_questions(
            args.category, 
            args.difficulty, 
            args.count
        )
        print(f"Generated and saved {saved} questions")