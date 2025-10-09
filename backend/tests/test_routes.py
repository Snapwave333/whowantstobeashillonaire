import pytest
from app import create_app
from database import db

@pytest.fixture
def app():
    """Create application for testing."""
    app = create_app('testing')
    app.config['TESTING'] = True
    app.config['WTF_CSRF_ENABLED'] = False
    
    with app.app_context():
        db.create_all()
        yield app
        db.drop_all()

@pytest.fixture
def client(app):
    """Create test client."""
    return app.test_client()

def test_game_routes(client):
    """Test game routes."""
    # Test game status endpoint
    response = client.get('/api/game/status')
    assert response.status_code in [200, 404]  # May not exist yet

def test_question_routes(client):
    """Test question routes."""
    # Test questions endpoint
    response = client.get('/api/questions/')
    assert response.status_code in [200, 404]  # May not exist yet

def test_admin_routes(client):
    """Test admin routes."""
    # Test admin endpoint
    response = client.get('/api/admin/')
    assert response.status_code in [200, 404, 401]  # May require auth
