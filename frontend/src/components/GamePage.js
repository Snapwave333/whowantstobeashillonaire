import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import styled from 'styled-components';
import { useGame } from '../context/GameContext';
import LoadingSpinner from './LoadingSpinner';
import PrizeLadder from './PrizeLadder';

const GameContainer = styled.div`
  flex: 1;
  display: flex;
  gap: 2rem;
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
  width: 100%;
  
  @media (max-width: 1200px) {
    max-width: 1000px;
  }
  
  @media (max-width: 768px) {
    flex-direction: column;
    gap: 1rem;
  }
`;

const MainGameArea = styled.div`
  flex: 1;
  display: flex;
  flex-direction: column;
`;

const QuestionSection = styled.div`
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border-radius: 16px;
  padding: 2rem;
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
`;

const QuestionHeader = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
  gap: 1rem;
`;

const QuestionNumber = styled.div`
  font-size: 1.1rem;
  font-weight: 600;
  color: #f7931e;
`;

const QuestionMeta = styled.div`
  display: flex;
  gap: 1rem;
  font-size: 0.9rem;
  opacity: 0.8;
  
  @media (max-width: 768px) {
    flex-direction: column;
    gap: 0.5rem;
  }
`;

const QuestionText = styled.h2`
  font-size: 1.5rem;
  font-weight: 600;
  line-height: 1.4;
  margin-bottom: 2rem;
  
  @media (max-width: 768px) {
    font-size: 1.3rem;
  }
`;

const AnswersGrid = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 2rem;
  
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
`;

const AnswerButton = styled.button`
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 12px;
  padding: 1.2rem;
  font-size: 1rem;
  text-align: left;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  
  &:hover:not(:disabled) {
    background: rgba(255, 255, 255, 0.2);
    border-color: rgba(255, 255, 255, 0.5);
    transform: translateY(-2px);
  }
  
  &:disabled {
    opacity: 0.3;
    cursor: not-allowed;
  }
  
  &.selected {
    background: linear-gradient(45deg, #2196f3, #21cbf3);
    border-color: #2196f3;
  }
  
  &.correct {
    background: linear-gradient(45deg, #4caf50, #66bb6a);
    border-color: #4caf50;
  }
  
  &.incorrect {
    background: linear-gradient(45deg, #f44336, #ef5350);
    border-color: #f44336;
  }
  
  &.eliminated {
    opacity: 0.3;
    pointer-events: none;
  }
`;

const AnswerLabel = styled.span`
  display: inline-block;
  width: 30px;
  height: 30px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 50%;
  text-align: center;
  line-height: 30px;
  margin-right: 1rem;
  font-weight: 600;
`;

const LifelinesSection = styled.div`
  display: flex;
  justify-content: center;
  gap: 1rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
`;

const LifelineButton = styled.button`
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  padding: 0.8rem 1.5rem;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  
  &:hover:not(:disabled) {
    background: rgba(255, 255, 255, 0.2);
    border-color: rgba(255, 255, 255, 0.5);
  }
  
  &:disabled {
    opacity: 0.3;
    cursor: not-allowed;
  }
`;

const ActionButtons = styled.div`
  display: flex;
  justify-content: center;
  gap: 1rem;
  flex-wrap: wrap;
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
    
    &:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(255, 107, 53, 0.3);
    }
  }
  
  &.secondary {
    background: rgba(255, 255, 255, 0.1);
    color: white;
    border: 2px solid rgba(255, 255, 255, 0.3);
    
    &:hover:not(:disabled) {
      background: rgba(255, 255, 255, 0.2);
      border-color: rgba(255, 255, 255, 0.5);
    }
  }
  
  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
`;

const ErrorMessage = styled.div`
  background: rgba(244, 67, 54, 0.1);
  border: 1px solid rgba(244, 67, 54, 0.3);
  border-radius: 8px;
  padding: 1rem;
  color: #ff6b6b;
  text-align: center;
  margin-bottom: 1rem;
`;

function GamePage() {
  const navigate = useNavigate();
  const {
    sessionId,
    currentQuestion,
    currentQuestionIndex,
    gameStatus,
    lifelines,
    eliminatedAnswers,
    loading,
    error,
    startGame,
    getQuestion,
    submitAnswer,
    submitFiftyFifty
  } = useGame();

  const [selectedAnswer, setSelectedAnswer] = useState(null);
  const [showResult, setShowResult] = useState(false);
  const [answerResult, setAnswerResult] = useState(null);
  const [gameStarting, setGameStarting] = useState(false);

  // Memoize answer labels to prevent recreation on every render
  const answerLabels = useMemo(() => ['A', 'B', 'C', 'D'], []);

  // Memoize formatted answers to optimize rendering
  const formattedAnswers = useMemo(() => {
    if (!currentQuestion?.answers) return [];
    
    return currentQuestion.answers.map((answer, index) => {
      const isEliminated = eliminatedAnswers.includes(answer);
      const isSelected = selectedAnswer === answer;
      let className = '';
      
      if (showResult && answerResult) {
        if (answer === answerResult.correct_answer) {
          className = 'correct';
        } else if (isSelected && answer !== answerResult.correct_answer) {
          className = 'incorrect';
        }
      } else if (isSelected) {
        className = 'selected';
      }
      
      if (isEliminated) {
        className += ' eliminated';
      }

      return {
        answer_text: answer,
        label: answerLabels[index],
        className,
        isEliminated,
        isSelected
      };
    });
  }, [currentQuestion?.answers, eliminatedAnswers, selectedAnswer, showResult, answerResult, answerLabels]);

  const loadQuestion = useCallback(async () => {
    try {
      await getQuestion();
      setSelectedAnswer(null);
      setShowResult(false);
      setAnswerResult(null);
    } catch (error) {
      console.error('Failed to load question:', error);
    }
  }, [getQuestion]);

  const handleStartGame = useCallback(async () => {
    if (gameStarting) return;
    
    try {
      setGameStarting(true);
      await startGame();
    } catch (error) {
      console.error('Failed to start game:', error);
    } finally {
      setGameStarting(false);
    }
  }, [startGame, gameStarting]);

  useEffect(() => {
    // Auto-start game immediately if no session exists
    if (!sessionId && gameStatus === 'not_started' && !gameStarting) {
      handleStartGame();
      return;
    }

    if (sessionId && gameStatus === 'playing' && !currentQuestion) {
      loadQuestion();
    }
  }, [sessionId, gameStatus, currentQuestion, loadQuestion, handleStartGame, gameStarting]);

  useEffect(() => {
    if (gameStatus === 'game_over' || gameStatus === 'won') {
      navigate('/game-over');
    }
  }, [gameStatus, navigate]);

  const handleAnswerSelect = (answerText) => {
    if (showResult || loading) return;
    setSelectedAnswer(answerText);
  };

  const handleSubmitAnswer = async () => {
    if (!selectedAnswer || loading) return;

    try {
      const result = await submitAnswer(selectedAnswer);
      setAnswerResult(result);
      setShowResult(true);
      
      // Auto-advance after showing result
      setTimeout(() => {
        if (result.session.status === 'playing') {
          loadQuestion();
        }
      }, 3000);
    } catch (error) {
      console.error('Failed to submit answer:', error);
    }
  };

  const handleUseFiftyFifty = async () => {
    if (!lifelines.fiftyFifty || loading) return;
    
    try {
      await submitFiftyFifty();
    } catch (error) {
      console.error('Failed to use 50/50:', error);
    }
  };

  const handleQuit = () => {
    navigate('/game-over');
  };

  if (!currentQuestion && !loading && !gameStarting) {
    return <LoadingSpinner text="Starting game..." />;
  }

  return (
    <GameContainer>
      <MainGameArea>
        {error && (
          <ErrorMessage>
            {error}
          </ErrorMessage>
        )}
        
        <QuestionSection>
        <QuestionHeader>
          <QuestionNumber>
            Question {currentQuestionIndex + 1} of 15
          </QuestionNumber>
          <QuestionMeta>
            <span>Category: {currentQuestion?.category || 'Loading...'}</span>
            <span>Difficulty: {currentQuestion?.difficulty || 'Loading...'}</span>
          </QuestionMeta>
        </QuestionHeader>
        
        <QuestionText>{currentQuestion?.question_text || 'Loading question...'}</QuestionText>
        
        <AnswersGrid>
          {formattedAnswers.map((answer, index) => (
            <AnswerButton
              key={index}
              onClick={() => handleAnswerSelect(answer.answer_text)}
              disabled={answer.isEliminated || showResult || loading}
              className={answer.className}
            >
              <AnswerLabel>{answer.label}</AnswerLabel>
              {answer.answer_text}
            </AnswerButton>
          ))}
        </AnswersGrid>
        
        <LifelinesSection>
          <LifelineButton
            onClick={handleUseFiftyFifty}
            disabled={!lifelines.fiftyFifty || showResult || loading}
          >
            50/50 {!lifelines.fiftyFifty && '(Used)'}
          </LifelineButton>
          <LifelineButton disabled>
            Phone a Friend (Coming Soon)
          </LifelineButton>
          <LifelineButton disabled>
            Ask the Audience (Coming Soon)
          </LifelineButton>
        </LifelinesSection>
        
        <ActionButtons>
          {!showResult ? (
            <>
              <ActionButton
                className="primary"
                onClick={handleSubmitAnswer}
                disabled={!selectedAnswer || loading}
              >
                {loading ? 'Submitting...' : 'Final Answer'}
              </ActionButton>
              <ActionButton
                className="secondary"
                onClick={handleQuit}
                disabled={loading}
              >
                Quit Game
              </ActionButton>
            </>
          ) : (
            <ActionButton
              className="primary"
              onClick={loadQuestion}
              disabled={loading}
            >
              {answerResult?.session.status === 'playing' ? 'Next Question' : 'Continue'}
            </ActionButton>
          )}
        </ActionButtons>
      </QuestionSection>
      </MainGameArea>
      
      <PrizeLadder />
    </GameContainer>
  );
}

export default GamePage;