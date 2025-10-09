import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import styled from 'styled-components';
import { useGame } from '../context/GameContext';
import TutorialModal from './TutorialModal';

const HomeContainer = styled.div`
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
`;

const WelcomeCard = styled.div`
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border-radius: 20px;
  padding: 3rem;
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  text-align: center;
  max-width: 500px;
  width: 100%;
`;

const Title = styled.h1`
  font-size: 2.5rem;
  font-weight: 700;
  margin-bottom: 1rem;
  background: linear-gradient(45deg, #ff6b35, #f7931e);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  
  @media (max-width: 768px) {
    font-size: 2rem;
  }
`;

const Subtitle = styled.p`
  font-size: 1.2rem;
  margin-bottom: 2rem;
  opacity: 0.9;
  line-height: 1.6;
`;

const Form = styled.form`
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
`;

const InputGroup = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  text-align: left;
`;

const Label = styled.label`
  font-weight: 500;
  font-size: 1rem;
`;

const Input = styled.input`
  padding: 1rem;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.1);
  color: white;
  font-size: 1rem;
  transition: all 0.3s ease;
  
  &::placeholder {
    color: rgba(255, 255, 255, 0.6);
  }
  
  &:focus {
    outline: none;
    border-color: #f7931e;
    background: rgba(255, 255, 255, 0.15);
  }
`;

const Select = styled.select`
  padding: 1rem;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.1);
  color: white;
  font-size: 1rem;
  transition: all 0.3s ease;
  
  &:focus {
    outline: none;
    border-color: #f7931e;
    background: rgba(255, 255, 255, 0.15);
  }
  
  option {
    background: #1e3c72;
    color: white;
  }
`;

const StartButton = styled.button`
  padding: 1rem 2rem;
  border: none;
  border-radius: 8px;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  background: linear-gradient(45deg, #ff6b35, #f7931e);
  color: white;
  
  &:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 8px 25px rgba(255, 107, 53, 0.3);
  }
  
  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
`;

const LoadingSpinner = styled.div`
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top: 2px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
`;

const ErrorMessage = styled.div`
  background: rgba(244, 67, 54, 0.1);
  border: 1px solid rgba(244, 67, 54, 0.3);
  border-radius: 8px;
  padding: 1rem;
  color: #ff6b6b;
  margin-top: 1rem;
`;

const TutorialButton = styled.button`
  padding: 0.75rem 1.5rem;
  border: 2px solid rgba(247, 147, 30, 0.5);
  border-radius: 8px;
  background: transparent;
  color: #f7931e;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  margin-top: 1rem;
  
  &:hover {
    background: rgba(247, 147, 30, 0.1);
    border-color: #f7931e;
    transform: translateY(-2px);
  }
`;

function HomePage() {
  const navigate = useNavigate();
  const { 
    startGame, 
    setPlayerName, 
    fetchCategories, 
    fetchDifficulties,
    categories, 
    difficulties, 
    loading, 
    error 
  } = useGame();
  
  const [formData, setFormData] = useState({
    playerName: '',
    difficulty: '',
    category: ''
  });
  
  const [showTutorial, setShowTutorial] = useState(false);

  useEffect(() => {
    fetchCategories();
    fetchDifficulties();
  }, [fetchCategories, fetchDifficulties]);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!formData.playerName.trim()) {
      return;
    }

    try {
      setPlayerName(formData.playerName.trim());
      await startGame(
        formData.playerName.trim(),
        formData.difficulty || null,
        formData.category || null
      );
      navigate('/game');
    } catch (error) {
      console.error('Failed to start game:', error);
    }
  };

  return (
    <HomeContainer>
      <WelcomeCard>
        <Title>Welcome!</Title>
        <Subtitle>
          Test your knowledge and climb the money ladder. 
          Answer 15 questions correctly to become a millionaire!
        </Subtitle>
        
        <Form onSubmit={handleSubmit}>
          <InputGroup>
            <Label htmlFor="playerName">Your Name</Label>
            <Input
              type="text"
              id="playerName"
              name="playerName"
              placeholder="Enter your name"
              value={formData.playerName}
              onChange={handleInputChange}
              required
              maxLength={50}
            />
          </InputGroup>
          
          <InputGroup>
            <Label htmlFor="difficulty">Difficulty (Optional)</Label>
            <Select
              id="difficulty"
              name="difficulty"
              value={formData.difficulty}
              onChange={handleInputChange}
            >
              <option value="">Any Difficulty</option>
              {difficulties.map(diff => (
                <option key={diff.id} value={diff.id}>
                  {diff.level_name}
                </option>
              ))}
            </Select>
          </InputGroup>
          
          <InputGroup>
            <Label htmlFor="category">Category (Optional)</Label>
            <Select
              id="category"
              name="category"
              value={formData.category}
              onChange={handleInputChange}
            >
              <option value="">Any Category</option>
              {categories.map(cat => (
                <option key={cat.id} value={cat.id}>
                  {cat.name}
                </option>
              ))}
            </Select>
          </InputGroup>
          
          <StartButton type="submit" disabled={loading || !formData.playerName.trim()}>
            {loading ? <LoadingSpinner /> : 'Start Game'}
          </StartButton>
        </Form>
        
        {error && (
          <ErrorMessage>
            {error}
          </ErrorMessage>
        )}
        
        <TutorialButton onClick={() => setShowTutorial(true)}>
          📚 How to Play
        </TutorialButton>
      </WelcomeCard>
      
      <TutorialModal 
        isOpen={showTutorial}
        onClose={() => setShowTutorial(false)}
        onStartGame={() => {
          setShowTutorial(false);
          // Auto-fill form if user wants to start after tutorial
          if (!formData.playerName) {
            setFormData(prev => ({ ...prev, playerName: 'Player' }));
          }
        }}
      />
    </HomeContainer>
  );
}

export default HomePage;