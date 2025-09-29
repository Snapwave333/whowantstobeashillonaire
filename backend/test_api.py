#!/usr/bin/env python3
"""
Test script for the Who Wants to Be a Millionaire API endpoints.
"""

import requests
import json
import sys

BASE_URL = "http://localhost:5000/api"

def test_game_start():
    """Test starting a new game session."""
    print("Testing game start endpoint...")
    
    url = f"{BASE_URL}/game/start"
    data = {"player_name": "Test Player"}
    
    response = requests.post(url, json=data)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    if response.status_code == 200 or response.status_code == 201:
        return response.json().get('session_token')
    return None

def test_get_question(session_token):
    """Test getting a question for the session."""
    print("\nTesting get question endpoint...")
    
    url = f"{BASE_URL}/game/question/{session_token}"
    
    response = requests.get(url)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    if response.status_code == 200:
        return response.json().get('question')
    return None

def test_submit_answer(session_token, answer, question_id):
    """Test submitting an answer."""
    print("\nTesting submit answer endpoint...")
    
    url = f"{BASE_URL}/game/answer"
    data = {
        "session_token": session_token,
        "question_id": question_id,
        "answer": answer
    }
    
    response = requests.post(url, json=data)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    return response.status_code == 200

def test_lifeline_fifty_fifty(session_token, question_id):
    """Test the 50/50 lifeline."""
    print("\nTesting 50/50 lifeline endpoint...")
    
    url = f"{BASE_URL}/game/lifeline/fifty-fifty"
    data = {
        "session_token": session_token,
        "question_id": question_id
    }
    
    response = requests.post(url, json=data)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    return response.status_code == 200

def test_session_info(session_token):
    """Test getting session information."""
    print("\nTesting session info endpoint...")
    
    url = f"{BASE_URL}/game/session/{session_token}"
    
    response = requests.get(url)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    return response.status_code == 200

def test_categories():
    """Test getting categories."""
    print("\nTesting categories endpoint...")
    
    url = f"{BASE_URL}/questions/categories"
    
    response = requests.get(url)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    return response.status_code == 200

def test_difficulties():
    """Test getting difficulty levels."""
    print("\nTesting difficulties endpoint...")
    
    url = f"{BASE_URL}/questions/difficulties"
    
    response = requests.get(url)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    return response.status_code == 200

def test_random_question():
    """Test getting a random question."""
    print("\nTesting random question endpoint...")
    
    url = f"{BASE_URL}/questions/random"
    
    response = requests.get(url)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    return response.status_code == 200

def main():
    """Run all API tests."""
    print("Starting API tests...\n")
    
    # Test basic endpoints first
    test_categories()
    test_difficulties()
    test_random_question()
    
    # Test game flow
    session_token = test_game_start()
    if not session_token:
        print("Failed to start game session. Exiting.")
        sys.exit(1)
    
    question = test_get_question(session_token)
    if not question:
        print("Failed to get question. Exiting.")
        sys.exit(1)
    
    # Test lifeline
    question_id = question.get('id') if question else None
    if question_id:
        test_lifeline_fifty_fifty(session_token, question_id)
    
    # Test session info
    test_session_info(session_token)
    
    # Submit correct answer
    correct_answer = question.get('answers', [])[0] if question.get('answers') else "Unknown"
    if question_id:
        test_submit_answer(session_token, correct_answer, question_id)
    
    print("\nAll tests completed!")

if __name__ == "__main__":
    main()