import pytest
import os
import tempfile
from app import create_app
from database import db

@pytest.fixture
def app():
    """Create application for testing."""
    db_fd, db_path = tempfile.mkstemp()
    
    app = create_app('testing')
    app.config['DATABASE'] = db_path
    app.config['TESTING'] = True
    app.config['WTF_CSRF_ENABLED'] = False
    
    with app.app_context():
        db.create_all()
        yield app
        db.drop_all()
    
    os.close(db_fd)
    os.unlink(db_path)

@pytest.fixture
def client(app):
    """Create test client."""
    return app.test_client()

@pytest.fixture
def runner(app):
    """Create test runner."""
    return app.test_cli_runner()

def test_health_check(client):
    """Test health check endpoint."""
    response = client.get('/api/health')
    assert response.status_code == 200
    data = response.get_json()
    assert data['status'] == 'healthy'
    assert 'message' in data

def test_app_creation():
    """Test app creation."""
    app = create_app('testing')
    assert app is not None
    assert app.config['TESTING'] is True
