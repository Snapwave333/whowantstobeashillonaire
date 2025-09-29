from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, create_access_token, get_jwt_identity
from models.models import db, TriviaQuestion, Category, DifficultyLevel, AdminUser
from werkzeug.security import generate_password_hash, check_password_hash
from datetime import datetime

admin_bp = Blueprint('admin', __name__)

@admin_bp.route('/login', methods=['POST'])
def admin_login():
    """Admin login endpoint."""
    try:
        data = request.get_json()
        username = data.get('username')
        password = data.get('password')
        
        if not all([username, password]):
            return jsonify({
                'success': False,
                'error': 'Missing username or password'
            }), 400
        
        # Find admin user
        admin = AdminUser.query.filter_by(username=username, is_active=True).first()
        
        if not admin or not check_password_hash(admin.password_hash, password):
            return jsonify({
                'success': False,
                'error': 'Invalid credentials'
            }), 401
        
        # Create access token
        access_token = create_access_token(identity=admin.id)
        
        return jsonify({
            'success': True,
            'access_token': access_token,
            'admin': admin.to_dict()
        }), 200
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@admin_bp.route('/questions', methods=['POST'])
@jwt_required()
def create_question():
    """Create a new trivia question."""
    try:
        data = request.get_json()
        
        # Validate required fields
        required_fields = ['question_text', 'correct_answer', 'incorrect_answer_1', 
                          'incorrect_answer_2', 'incorrect_answer_3']
        
        for field in required_fields:
            if not data.get(field):
                return jsonify({
                    'success': False,
                    'error': f'Missing required field: {field}'
                }), 400
        
        # Create new question
        question = TriviaQuestion(
            question_text=data['question_text'],
            correct_answer=data['correct_answer'],
            incorrect_answer_1=data['incorrect_answer_1'],
            incorrect_answer_2=data['incorrect_answer_2'],
            incorrect_answer_3=data['incorrect_answer_3'],
            explanation=data.get('explanation'),
            category_id=data.get('category_id'),
            difficulty_id=data.get('difficulty_id'),
            source=data.get('source', 'admin')
        )
        
        db.session.add(question)
        db.session.commit()
        
        return jsonify({
            'success': True,
            'message': 'Question created successfully',
            'question': question.to_dict(include_correct=True)
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@admin_bp.route('/questions/<int:question_id>', methods=['PUT'])
@jwt_required()
def update_question(question_id):
    """Update an existing trivia question."""
    try:
        question = TriviaQuestion.query.get(question_id)
        if not question:
            return jsonify({
                'success': False,
                'error': 'Question not found'
            }), 404
        
        data = request.get_json()
        
        # Update fields if provided
        if 'question_text' in data:
            question.question_text = data['question_text']
        if 'correct_answer' in data:
            question.correct_answer = data['correct_answer']
        if 'incorrect_answer_1' in data:
            question.incorrect_answer_1 = data['incorrect_answer_1']
        if 'incorrect_answer_2' in data:
            question.incorrect_answer_2 = data['incorrect_answer_2']
        if 'incorrect_answer_3' in data:
            question.incorrect_answer_3 = data['incorrect_answer_3']
        if 'explanation' in data:
            question.explanation = data['explanation']
        if 'category_id' in data:
            question.category_id = data['category_id']
        if 'difficulty_id' in data:
            question.difficulty_id = data['difficulty_id']
        if 'is_active' in data:
            question.is_active = data['is_active']
        
        question.updated_at = datetime.utcnow()
        
        db.session.commit()
        
        return jsonify({
            'success': True,
            'message': 'Question updated successfully',
            'question': question.to_dict(include_correct=True)
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@admin_bp.route('/questions/<int:question_id>', methods=['DELETE'])
@jwt_required()
def delete_question(question_id):
    """Delete a trivia question (soft delete by setting is_active=False)."""
    try:
        question = TriviaQuestion.query.get(question_id)
        if not question:
            return jsonify({
                'success': False,
                'error': 'Question not found'
            }), 404
        
        # Soft delete
        question.is_active = False
        question.updated_at = datetime.utcnow()
        
        db.session.commit()
        
        return jsonify({
            'success': True,
            'message': 'Question deleted successfully'
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@admin_bp.route('/questions', methods=['GET'])
@jwt_required()
def list_questions():
    """List all questions with pagination and filters."""
    try:
        # Get query parameters
        page = request.args.get('page', 1, type=int)
        per_page = min(request.args.get('per_page', 20, type=int), 100)
        category_id = request.args.get('category_id', type=int)
        difficulty_id = request.args.get('difficulty_id', type=int)
        include_inactive = request.args.get('include_inactive', 'false').lower() == 'true'
        
        # Build query
        query = TriviaQuestion.query
        
        if not include_inactive:
            query = query.filter(TriviaQuestion.is_active == True)
        
        if category_id:
            query = query.filter(TriviaQuestion.category_id == category_id)
        
        if difficulty_id:
            query = query.filter(TriviaQuestion.difficulty_id == difficulty_id)
        
        # Order by creation date (newest first)
        query = query.order_by(TriviaQuestion.created_at.desc())
        
        # Paginate
        pagination = query.paginate(
            page=page,
            per_page=per_page,
            error_out=False
        )
        
        questions = [q.to_dict(include_correct=True) for q in pagination.items]
        
        return jsonify({
            'success': True,
            'questions': questions,
            'pagination': {
                'page': page,
                'per_page': per_page,
                'total': pagination.total,
                'pages': pagination.pages,
                'has_next': pagination.has_next,
                'has_prev': pagination.has_prev
            }
        }), 200
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@admin_bp.route('/categories', methods=['POST'])
@jwt_required()
def create_category():
    """Create a new category."""
    try:
        data = request.get_json()
        name = data.get('name')
        description = data.get('description')
        
        if not name:
            return jsonify({
                'success': False,
                'error': 'Category name is required'
            }), 400
        
        # Check if category already exists
        existing = Category.query.filter_by(name=name).first()
        if existing:
            return jsonify({
                'success': False,
                'error': 'Category already exists'
            }), 400
        
        category = Category(name=name, description=description)
        db.session.add(category)
        db.session.commit()
        
        return jsonify({
            'success': True,
            'message': 'Category created successfully',
            'category': category.to_dict()
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@admin_bp.route('/users', methods=['POST'])
@jwt_required()
def create_admin_user():
    """Create a new admin user."""
    try:
        data = request.get_json()
        username = data.get('username')
        password = data.get('password')
        email = data.get('email')
        
        if not all([username, password]):
            return jsonify({
                'success': False,
                'error': 'Username and password are required'
            }), 400
        
        # Check if user already exists
        existing = AdminUser.query.filter_by(username=username).first()
        if existing:
            return jsonify({
                'success': False,
                'error': 'Username already exists'
            }), 400
        
        # Hash password
        password_hash = generate_password_hash(password)
        
        admin = AdminUser(
            username=username,
            password_hash=password_hash,
            email=email
        )
        
        db.session.add(admin)
        db.session.commit()
        
        return jsonify({
            'success': True,
            'message': 'Admin user created successfully',
            'admin': admin.to_dict()
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

@admin_bp.route('/dashboard', methods=['GET'])
@jwt_required()
def admin_dashboard():
    """Get admin dashboard statistics."""
    try:
        # Get various statistics
        total_questions = TriviaQuestion.query.filter(TriviaQuestion.is_active == True).count()
        total_categories = Category.query.count()
        total_sessions = db.session.query(db.func.count(db.distinct(db.text('game_sessions.id')))).scalar()
        
        # Most used questions
        popular_questions = TriviaQuestion.query.filter(
            TriviaQuestion.is_active == True
        ).order_by(TriviaQuestion.times_used.desc()).limit(5).all()
        
        return jsonify({
            'success': True,
            'dashboard': {
                'total_questions': total_questions,
                'total_categories': total_categories,
                'total_game_sessions': total_sessions,
                'popular_questions': [q.to_dict() for q in popular_questions]
            }
        }), 200
        
    except Exception as e:
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500