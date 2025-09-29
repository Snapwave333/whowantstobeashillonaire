import React from 'react';
import styled from 'styled-components';
import { useGame } from '../context/GameContext';

const HeaderContainer = styled.header`
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(10px);
  padding: 1rem 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
`;

const HeaderContent = styled.div`
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
`;

const Logo = styled.h1`
  font-size: 1.8rem;
  font-weight: 700;
  background: linear-gradient(45deg, #ff6b35, #f7931e);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0;
`;

const GameInfo = styled.div`
  display: flex;
  align-items: center;
  gap: 2rem;
  
  @media (max-width: 768px) {
    gap: 1rem;
    flex-direction: column;
    align-items: flex-end;
  }
`;

const PlayerInfo = styled.div`
  text-align: right;
  
  @media (max-width: 768px) {
    text-align: center;
  }
`;

const PlayerName = styled.div`
  font-size: 1rem;
  font-weight: 500;
  margin-bottom: 0.25rem;
`;

const Score = styled.div`
  font-size: 1.2rem;
  font-weight: 700;
  color: #f7931e;
`;

function Header() {
  const { prizeAmount, gameStatus } = useGame();

  return (
    <HeaderContainer>
      <HeaderContent>
        <Logo>Who Want to Be a Shillionaire?</Logo>
        {gameStatus === 'playing' && (
          <GameInfo>
            <PlayerInfo>
              <Score>${prizeAmount.toLocaleString()}</Score>
            </PlayerInfo>
          </GameInfo>
        )}
      </HeaderContent>
    </HeaderContainer>
  );
}

export default Header;