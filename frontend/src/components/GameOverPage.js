import React from 'react';
import { useNavigate } from 'react-router-dom';
import styled from 'styled-components';
import { useGame } from '../context/GameContext';

const GameOverContainer = styled.div`
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
`;

const ResultCard = styled.div`
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

const ResultIcon = styled.div`
  font-size: 4rem;
  margin-bottom: 1rem;
`;

const ResultTitle = styled.h1`
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

const ResultSubtitle = styled.h2`
  font-size: 1.5rem;
  font-weight: 600;
  margin-bottom: 2rem;
  color: #f7931e;
  
  @media (max-width: 768px) {
    font-size: 1.3rem;
  }
`;

const PlayerStats = styled.div`
  background: rgba(255, 255, 255, 0.05);
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 2rem;
`;

const StatRow = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  
  &:last-child {
    margin-bottom: 0;
  }
`;

const StatLabel = styled.span`
  font-size: 1rem;
  opacity: 0.8;
`;

const StatValue = styled.span`
  font-size: 1.1rem;
  font-weight: 600;
  color: #f7931e;
`;

const Message = styled.p`
  font-size: 1.1rem;
  line-height: 1.6;
  margin-bottom: 2rem;
  opacity: 0.9;
`;

const ActionButtons = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1rem;
`;

const ActionButton = styled.button`
  padding: 1rem 2rem;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  
  &.primary {
    background: linear-gradient(45deg, #ff6b35, #f7931e);
    color: white;
    
    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(255, 107, 53, 0.3);
    }
  }
  
  &.secondary {
    background: rgba(255, 255, 255, 0.1);
    color: white;
    border: 2px solid rgba(255, 255, 255, 0.3);
    
    &:hover {
      background: rgba(255, 255, 255, 0.2);
      border-color: rgba(255, 255, 255, 0.5);
    }
  }
`;

const Confetti = styled.div`
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
  overflow: hidden;
  
  &::before,
  &::after {
    content: '';
    position: absolute;
    width: 10px;
    height: 10px;
    background: #f7931e;
    animation: confetti 3s ease-in-out infinite;
  }
  
  &::before {
    left: 10%;
    animation-delay: 0s;
  }
  
  &::after {
    left: 90%;
    animation-delay: 1s;
    background: #ff6b35;
  }
  
  @keyframes confetti {
    0% {
      transform: translateY(-100vh) rotate(0deg);
      opacity: 1;
    }
    100% {
      transform: translateY(100vh) rotate(720deg);
      opacity: 0;
    }
  }
`;

function GameOverPage() {
  const navigate = useNavigate();
  const { 
    playerName, 
    score, 
    prizeAmount, 
    currentQuestionIndex, 
    gameStatus, 
    resetGame 
  } = useGame();

  const handlePlayAgain = () => {
    resetGame();
    navigate('/game');
  };

  const handleGoHome = () => {
    resetGame();
    navigate('/game');
  };

  const isWinner = gameStatus === 'completed';
  const questionsAnswered = currentQuestionIndex;
  const accuracy = questionsAnswered > 0 ? Math.round((score / questionsAnswered) * 100) : 0;

  const getResultMessage = () => {
    if (isWinner) {
      return "Congratulations! You've answered all questions correctly and become a millionaire!";
    } else if (questionsAnswered >= 10) {
      return "Great job! You made it quite far in the game. That's an impressive performance!";
    } else if (questionsAnswered >= 5) {
      return "Not bad! You answered several questions correctly. Keep practicing and you'll do even better next time!";
    } else {
      return "Good effort! Every expert was once a beginner. Try again and see how much you can improve!";
    }
  };

  const getResultIcon = () => {
    if (isWinner) return "🏆";
    if (questionsAnswered >= 10) return "🎉";
    if (questionsAnswered >= 5) return "👏";
    return "💪";
  };

  return (
    <GameOverContainer>
      {isWinner && <Confetti />}
      <ResultCard>
        <ResultIcon>{getResultIcon()}</ResultIcon>
        <ResultTitle>
          {isWinner ? "Millionaire!" : "Game Over"}
        </ResultTitle>
        <ResultSubtitle>
          ${prizeAmount.toLocaleString()} Won!
        </ResultSubtitle>
        
        <PlayerStats>
          <StatRow>
            <StatLabel>Questions Answered:</StatLabel>
            <StatValue>{questionsAnswered} / 15</StatValue>
          </StatRow>
          <StatRow>
            <StatLabel>Correct Answers:</StatLabel>
            <StatValue>{score}</StatValue>
          </StatRow>
          <StatRow>
            <StatLabel>Accuracy:</StatLabel>
            <StatValue>{accuracy}%</StatValue>
          </StatRow>
          <StatRow>
            <StatLabel>Final Prize:</StatLabel>
            <StatValue>${prizeAmount.toLocaleString()}</StatValue>
          </StatRow>
        </PlayerStats>
        
        <Message>
          {getResultMessage()}
        </Message>
        
        <ActionButtons>
          <ActionButton className="primary" onClick={handlePlayAgain}>
            Play Again
          </ActionButton>
          <ActionButton className="secondary" onClick={handleGoHome}>
            Back to Home
          </ActionButton>
        </ActionButtons>
      </ResultCard>
    </GameOverContainer>
  );
}

export default GameOverPage;