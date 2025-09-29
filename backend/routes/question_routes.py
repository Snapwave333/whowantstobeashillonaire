from flask import Blueprint, request, jsonify
from models.models import db, TriviaQuestion, Category, DifficultyLevel
from sqlalchemy import func

question_bp = Blueprint('questions', __name__)

@question_bp.route('/random', methods=['GET'])
def get_random_question():
    """Get a random trivia question with optional filters."""
    try:
        # Get query parameters
        category_id = request.args.get('category_id', type=int)
        difficulty_level = request.args.get('difficulty_level', type=int)
        include_answer = request.args.get('include_answer', 'false').lower() == 'true'
        
        # Build query
        query = TriviaQuestion.query.filter(TriviaQuestion.is_active == True)
        
        if category_id:
            query = query.filter(TriviaQuestion.category_id == category_id)
        
        if difficulty_level:
            query = query.join(DifficultyLevel).filter(DifficultyLevel.level_order == difficulty_level)
        
        # Get random question
        question = query.order_by(func.random()).first()
        
        if not question:
            return jsonify({
                'success': False,
                'error': 'No questions found matching criteria'
            }), 404
        
        # Update usage count
        question.times_used += 1
        db.session.commit()
        
        return jsonify({
            'success': True,
            'question': question.to_dict(include_correct=include_answer)
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@question_bp.route('/validate', methods=['POST'])
def validate_answer():
    """Validate a user's answer to a question."""
    try:
        data = request.get_json()
        question_id = data.get('question_id')
        user_answer = data.get('answer')
        
        if not all([question_id, user_answer]):
            return jsonify({
                'success': False,
                'error': 'Missing required fields: question_id, answer'
            }), 400
        
        # Find the question
        question = TriviaQuestion.query.get(question_id)
        if not question:
            return jsonify({
                'success': False,
                'error': 'Question not found'
            }), 404
        
        # Check if answer is correct
        is_correct = question.is_answer_correct(user_answer)
        
        return jsonify({
            'success': True,
            'is_correct': is_correct,
            'correct_answer': question.correct_answer,
            'explanation': question.explanation,
            'user_answer': user_answer
        }), 200
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@question_bp.route('/categories', methods=['GET'])
def get_categories():
    """Get all available categories."""
    try:
        categories = Category.query.all()
        
        return jsonify({
            'success': True,
            'categories': [category.to_dict() for category in categories]
        }), 200
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@question_bp.route('/difficulties', methods=['GET'])
def get_difficulties():
    """Get all difficulty levels."""
    try:
        difficulties = DifficultyLevel.query.order_by(DifficultyLevel.level_order).all()
        
        return jsonify({
            'success': True,
            'difficulties': [difficulty.to_dict() for difficulty in difficulties]
        }), 200
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@question_bp.route('/stats', methods=['GET'])
def get_question_stats():
    """Get statistics about questions in the database."""
    try:
        total_questions = TriviaQuestion.query.filter(TriviaQuestion.is_active == True).count()
        
        # Questions by category
        category_stats = db.session.query(
            Category.name,
            func.count(TriviaQuestion.id).label('count')
        ).join(TriviaQuestion).filter(TriviaQuestion.is_active == True).group_by(Category.name).all()
        
        # Questions by difficulty
        difficulty_stats = db.session.query(
            DifficultyLevel.level_name,
            DifficultyLevel.level_order,
            func.count(TriviaQuestion.id).label('count')
        ).join(TriviaQuestion).filter(TriviaQuestion.is_active == True).group_by(
            DifficultyLevel.level_name, DifficultyLevel.level_order
        ).order_by(DifficultyLevel.level_order).all()
        
        return jsonify({
            'success': True,
            'stats': {
                'total_questions': total_questions,
                'by_category': [{'name': name, 'count': count} for name, count in category_stats],
                'by_difficulty': [{'level': name, 'order': order, 'count': count} for name, order, count in difficulty_stats]
            }
        }), 200
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@question_bp.route('/<int:question_id>', methods=['GET'])
def get_question_by_id(question_id):
    """Get a specific question by ID."""
    try:
        include_answer = request.args.get('include_answer', 'false').lower() == 'true'
        
        question = TriviaQuestion.query.get(question_id)
        if not question:
            return jsonify({
                'success': False,
                'error': 'Question not found'
            }), 404
        
        return jsonify({
            'success': True,
            'question': question.to_dict(include_correct=include_answer)
        }), 200
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500