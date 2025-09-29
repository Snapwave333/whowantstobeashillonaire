from flask import Flask
from flask_cors import CORS
from flask_jwt_extended import JWTManager
from config import config
from database import db
import os
jwt = JWTManager()

def create_app(config_name=None):
    """Application factory pattern."""
    if config_name is None:
        config_name = os.environ.get('FLASK_ENV', 'development')
    
    app = Flask(__name__)
    app.config.from_object(config[config_name])
    
    # Initialize extensions with app
    db.init_app(app)
    jwt.init_app(app)
    CORS(app, origins=app.config['CORS_ORIGINS'])
    
    # Register blueprints
    from routes.game_routes import game_bp
    from routes.admin_routes import admin_bp
    from routes.question_routes import question_bp
    
    app.register_blueprint(game_bp, url_prefix='/api/game')
    app.register_blueprint(admin_bp, url_prefix='/api/admin')
    app.register_blueprint(question_bp, url_prefix='/api/questions')
    
    # Health check endpoint
    @app.route('/api/health')
    def health_check():
        return {'status': 'healthy', 'message': 'Trivia API is running'}
    
    # Create database tables
    with app.app_context():
        db.create_all()
    
    return app

if __name__ == '__main__':
    app = create_app()
    app.run(debug=True, host='0.0.0.0', port=5000)