import React from 'react';
import styled from 'styled-components';
import { useGame } from '../context/GameContext';

const LadderContainer = styled.div`
  background: rgba(0, 0, 0, 0.8);
  backdrop-filter: blur(10px);
  border-radius: 16px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  width: 280px;
  max-height: 600px;
  overflow-y: auto;
  
  @media (max-width: 1200px) {
    width: 250px;
  }
  
  @media (max-width: 768px) {
    width: 100%;
    max-width: 400px;
    margin: 0 auto 2rem;
  }
`;

const LadderTitle = styled.h3`
  color: #f7931e;
  font-size: 1.1rem;
  font-weight: 600;
  text-align: center;
  margin-bottom: 1rem;
  text-transform: uppercase;
  letter-spacing: 1px;
`;

const LadderLevel = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.6rem 1rem;
  margin-bottom: 0.3rem;
  border-radius: 8px;
  border: 1px solid transparent;
  transition: all 0.3s ease;
  position: relative;
  
  ${props => props.isCurrent && `
    background: linear-gradient(45deg, #f7931e, #ff6b35);
    border-color: #f7931e;
    box-shadow: 0 0 20px rgba(247, 147, 30, 0.4);
    transform: scale(1.02);
  `}
  
  ${props => props.isCompleted && `
    background: rgba(76, 175, 80, 0.2);
    border-color: rgba(76, 175, 80, 0.5);
  `}
  
  ${props => props.isSafeHaven && `
    background: rgba(33, 150, 243, 0.2);
    border-color: rgba(33, 150, 243, 0.5);
    
    &::after {
      content: 'SAFE';
      position: absolute;
      right: -8px;
      top: 50%;
      transform: translateY(-50%);
      background: #2196f3;
      color: white;
      font-size: 0.6rem;
      font-weight: bold;
      padding: 2px 6px;
      border-radius: 4px;
      letter-spacing: 0.5px;
    }
  `}
  
  ${props => !props.isCurrent && !props.isCompleted && `
    background: rgba(255, 255, 255, 0.05);
    border-color: rgba(255, 255, 255, 0.1);
  `}
`;

const LevelNumber = styled.span`
  font-weight: 600;
  font-size: 0.9rem;
  color: ${props => props.isCurrent ? '#000' : '#fff'};
  min-width: 20px;
`;

const PrizeAmount = styled.span`
  font-weight: 700;
  font-size: 0.9rem;
  color: ${props => props.isCurrent ? '#000' : '#f7931e'};
  text-align: right;
`;

const PrizeLadder = () => {
  const { currentQuestionIndex, gameStatus } = useGame();
  
  // Prize amounts for each level (1-15)
  const prizeAmounts = [
    100, 200, 300, 500, 1000,           // Levels 1-5
    2000, 4000, 8000, 16000, 32000,     // Levels 6-10
    64000, 125000, 250000, 500000, 1000000  // Levels 11-15
  ];
  
  // Safe haven levels (guaranteed prize levels)
  const safeHavens = [4, 9]; // 0-indexed (levels 5 and 10)
  
  const formatPrize = (amount) => {
    if (amount >= 1000000) {
      return `$${(amount / 1000000).toFixed(1)}M`;
    } else if (amount >= 1000) {
      return `$${(amount / 1000).toFixed(0)}K`;
    } else {
      return `$${amount}`;
    }
  };
  
  return (
    <LadderContainer>
      <LadderTitle>Prize Ladder</LadderTitle>
      {prizeAmounts.slice().reverse().map((amount, index) => {
        const levelIndex = prizeAmounts.length - 1 - index; // Convert back to 0-indexed
        const levelNumber = levelIndex + 1; // Display as 1-indexed
        const isCurrent = gameStatus === 'playing' && currentQuestionIndex === levelIndex;
        const isCompleted = gameStatus === 'playing' && currentQuestionIndex > levelIndex;
        const isSafeHaven = safeHavens.includes(levelIndex);
        
        return (
          <LadderLevel
            key={levelIndex}
            isCurrent={isCurrent}
            isCompleted={isCompleted}
            isSafeHaven={isSafeHaven}
          >
            <LevelNumber isCurrent={isCurrent}>{levelNumber}</LevelNumber>
            <PrizeAmount isCurrent={isCurrent}>
              {formatPrize(amount)}
            </PrizeAmount>
          </LadderLevel>
        );
      })}
    </LadderContainer>
  );
};

export default PrizeLadder;