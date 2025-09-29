from flask import Blueprint, request, jsonify
from models.models import db, GameSession, TriviaQuestion, SessionQuestion, DifficultyLevel
from datetime import datetime
import uuid
import random

game_bp = Blueprint('game', __name__)

@game_bp.route('/start', methods=['POST'])
def start_game():
    """Start a new game session."""
    try:
        # Generate unique session token
        session_token = str(uuid.uuid4())
        
        # Create new game session
        session = GameSession(
            session_token=session_token,
            current_question_level=1,
            total_winnings=0,
            questions_answered=0,
            is_active=True
        )
        
        db.session.add(session)
        db.session.commit()
        
        return jsonify({
            'success': True,
            'session_token': session_token,
            'message': 'Game session started successfully',
            'session': session.to_dict()
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@game_bp.route('/question/<session_token>', methods=['GET'])
def get_question(session_token):
    """Get the next question for the current session."""
    try:
        # Find active session
        session = GameSession.query.filter_by(
            session_token=session_token,
            is_active=True
        ).first()
        
        if not session:
            return jsonify({
                'success': False,
                'error': 'Invalid or expired session'
            }), 404
        
        # Get difficulty level for current question
        difficulty_level = session.current_question_level
        
        # Get list of already used questions
        used_question_ids = session.get_used_question_ids()
        
        # Find a random question for this difficulty level
        question = TriviaQuestion.get_random_question(
            difficulty_level=difficulty_level,
            exclude_ids=used_question_ids
        )
        
        if not question:
            return jsonify({
                'success': False,
                'error': 'No more questions available for this difficulty level'
            }), 404
        
        # Get prize amount for current level
        difficulty = DifficultyLevel.query.filter_by(level_order=difficulty_level).first()
        prize_amount = difficulty.prize_amount if difficulty else 0
        
        # Create session question record
        session_question = SessionQuestion(
            session_id=session.id,
            question_id=question.id,
            question_order=session.current_question_level
        )
        
        db.session.add(session_question)
        
        # Update question usage count
        question.times_used += 1
        
        db.session.commit()
        
        # Return question with shuffled answers (without revealing correct answer)
        return jsonify({
            'success': True,
            'question': {
                'id': question.id,
                'text': question.question_text,
                'answers': [ans['text'] for ans in question.get_shuffled_answers()],
                'category': question.category.name if question.category else None,
                'difficulty': question.difficulty.level_name if question.difficulty else None,
                'level': session.current_question_level,
                'prize_amount': prize_amount
            },
            'session': {
                'current_level': session.current_question_level,
                'total_winnings': session.total_winnings,
                'lifelines_used': session.get_lifelines_used()
            }
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@game_bp.route('/answer', methods=['POST'])
def submit_answer():
    """Submit an answer and validate it."""
    try:
        data = request.get_json()
        session_token = data.get('session_token')
        question_id = data.get('question_id')
        user_answer = data.get('answer')
        
        if not all([session_token, question_id, user_answer]):
            return jsonify({
                'success': False,
                'error': 'Missing required fields: session_token, question_id, answer'
            }), 400
        
        # Find active session
        session = GameSession.query.filter_by(
            session_token=session_token,
            is_active=True
        ).first()
        
        if not session:
            return jsonify({
                'success': False,
                'error': 'Invalid or expired session'
            }), 404
        
        # Find the question
        question = TriviaQuestion.query.get(question_id)
        if not question:
            return jsonify({
                'success': False,
                'error': 'Question not found'
            }), 404
        
        # Find the session question record
        session_question = SessionQuestion.query.filter_by(
            session_id=session.id,
            question_id=question_id
        ).first()
        
        if not session_question:
            return jsonify({
                'success': False,
                'error': 'Question not associated with this session'
            }), 400
        
        # Check if answer is correct
        is_correct = question.is_answer_correct(user_answer)
        
        # Update session question record
        session_question.user_answer = user_answer
        session_question.is_correct = is_correct
        session_question.answered_at = datetime.utcnow()
        
        # Update session stats
        session.questions_answered += 1
        
        if is_correct:
            # Get prize amount for current level
            difficulty = DifficultyLevel.query.filter_by(
                level_order=session.current_question_level
            ).first()
            
            if difficulty:
                session.total_winnings = difficulty.prize_amount
            
            # Move to next level
            session.current_question_level += 1
            
            # Check if game is complete (reached maximum level)
            max_level = DifficultyLevel.query.count()
            if session.current_question_level > max_level:
                session.end_session()
                game_complete = True
            else:
                game_complete = False
        else:
            # Wrong answer - end game
            session.end_session()
            game_complete = True
        
        db.session.commit()
        
        return jsonify({
            'success': True,
            'is_correct': is_correct,
            'correct_answer': question.correct_answer,
            'explanation': question.explanation,
            'game_complete': game_complete,
            'session': session.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@game_bp.route('/lifeline/fifty-fifty', methods=['POST'])
def use_fifty_fifty():
    """Use 50/50 lifeline to eliminate two wrong answers."""
    try:
        data = request.get_json()
        session_token = data.get('session_token')
        question_id = data.get('question_id')
        
        if not all([session_token, question_id]):
            return jsonify({
                'success': False,
                'error': 'Missing required fields: session_token, question_id'
            }), 400
        
        # Find active session
        session = GameSession.query.filter_by(
            session_token=session_token,
            is_active=True
        ).first()
        
        if not session:
            return jsonify({
                'success': False,
                'error': 'Invalid or expired session'
            }), 404
        
        # Check if lifeline already used
        if 'fifty_fifty' in session.get_lifelines_used():
            return jsonify({
                'success': False,
                'error': '50/50 lifeline already used'
            }), 400
        
        # Find the question
        question = TriviaQuestion.query.get(question_id)
        if not question:
            return jsonify({
                'success': False,
                'error': 'Question not found'
            }), 404
        
        # Get all answers and remove 2 incorrect ones
        all_answers = question.get_shuffled_answers()
        correct_answer = next(ans for ans in all_answers if ans['is_correct'])
        incorrect_answers = [ans for ans in all_answers if not ans['is_correct']]
        
        # Randomly select 1 incorrect answer to keep
        kept_incorrect = random.choice(incorrect_answers)
        
        # Return correct answer and 1 incorrect answer
        remaining_answers = [correct_answer, kept_incorrect]
        random.shuffle(remaining_answers)
        
        # Mark lifeline as used
        session.use_lifeline('fifty_fifty')
        db.session.commit()
        
        return jsonify({
            'success': True,
            'remaining_answers': [ans['text'] for ans in remaining_answers],
            'lifelines_used': session.get_lifelines_used()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@game_bp.route('/session/<session_token>', methods=['GET'])
def get_session_info(session_token):
    """Get current session information."""
    try:
        session = GameSession.query.filter_by(session_token=session_token).first()
        
        if not session:
            return jsonify({
                'success': False,
                'error': 'Session not found'
            }), 404
        
        return jsonify({
            'success': True,
            'session': session.to_dict()
        }), 200
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@game_bp.route('/quit', methods=['POST'])
def quit_game():
    """Quit current game session."""
    try:
        data = request.get_json()
        session_token = data.get('session_token')
        
        if not session_token:
            return jsonify({
                'success': False,
                'error': 'Missing session_token'
            }), 400
        
        session = GameSession.query.filter_by(
            session_token=session_token,
            is_active=True
        ).first()
        
        if not session:
            return jsonify({
                'success': False,
                'error': 'Active session not found'
            }), 404
        
        session.end_session()
        db.session.commit()
        
        return jsonify({
            'success': True,
            'message': 'Game session ended',
            'final_winnings': session.total_winnings
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500