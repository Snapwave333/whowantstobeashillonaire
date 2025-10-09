import React from 'react';
import styled from 'styled-components';

const TutorialOverlay = styled.div`
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10000;
  padding: 2rem;
`;

const TutorialModal = styled.div`
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  border-radius: 20px;
  padding: 2rem;
  max-width: 600px;
  width: 100%;
  max-height: 80vh;
  overflow-y: auto;
  border: 1px solid rgba(247, 147, 30, 0.3);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
`;

const TutorialHeader = styled.div`
  text-align: center;
  margin-bottom: 2rem;
`;

const TutorialTitle = styled.h2`
  font-size: 2rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
  background: linear-gradient(45deg, #f7931e, #ffd700);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
`;

const TutorialSubtitle = styled.p`
  font-size: 1.1rem;
  opacity: 0.9;
  margin-bottom: 1rem;
`;

const TutorialContent = styled.div`
  margin-bottom: 2rem;
`;

const TutorialSection = styled.div`
  margin-bottom: 1.5rem;
`;

const SectionTitle = styled.h3`
  font-size: 1.3rem;
  font-weight: 600;
  color: #f7931e;
  margin-bottom: 0.5rem;
`;

const SectionText = styled.p`
  line-height: 1.6;
  margin-bottom: 0.5rem;
  opacity: 0.9;
`;

const PrizeLadder = styled.div`
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  padding: 1rem;
  margin: 1rem 0;
`;

const PrizeLevel = styled.div`
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  
  &:last-child {
    border-bottom: none;
  }
  
  &.safe-haven {
    background: rgba(247, 147, 30, 0.2);
    border-radius: 5px;
    padding: 0.5rem;
    margin: 0.25rem 0;
  }
`;

const LifelineGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin: 1rem 0;
`;

const LifelineCard = styled.div`
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  padding: 1rem;
  text-align: center;
`;

const LifelineIcon = styled.div`
  font-size: 2rem;
  margin-bottom: 0.5rem;
`;

const LifelineName = styled.h4`
  color: #f7931e;
  margin-bottom: 0.5rem;
`;

const LifelineDesc = styled.p`
  font-size: 0.9rem;
  opacity: 0.8;
`;

const TutorialActions = styled.div`
  display: flex;
  gap: 1rem;
  justify-content: center;
`;

const TutorialButton = styled.button`
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  
  &.primary {
    background: linear-gradient(45deg, #f7931e, #ffd700);
    color: white;
    
    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(247, 147, 30, 0.3);
    }
  }
  
  &.secondary {
    background: rgba(255, 255, 255, 0.1);
    color: white;
    border: 1px solid rgba(255, 255, 255, 0.3);
    
    &:hover {
      background: rgba(255, 255, 255, 0.2);
    }
  }
`;

const Tutorial = ({ isOpen, onClose, onStartGame }) => {
  if (!isOpen) return null;

  return (
    <TutorialOverlay>
      <TutorialModal>
        <TutorialHeader>
          <TutorialTitle>How to Play</TutorialTitle>
          <TutorialSubtitle>Welcome to Who Wants to be a Shillionaire!</TutorialSubtitle>
        </TutorialHeader>

        <TutorialContent>
          <TutorialSection>
            <SectionTitle>🎯 Objective</SectionTitle>
            <SectionText>
              Answer 15 questions correctly to win the grand prize! Each question gets progressively harder and more valuable.
            </SectionText>
          </TutorialSection>

          <TutorialSection>
            <SectionTitle>💰 Prize Ladder</SectionTitle>
            <PrizeLadder>
              <PrizeLevel className="safe-haven">
                <span>Level 5</span>
                <span>$1,000 (Safe Haven)</span>
              </PrizeLevel>
              <PrizeLevel className="safe-haven">
                <span>Level 10</span>
                <span>$32,000 (Safe Haven)</span>
              </PrizeLevel>
              <PrizeLevel>
                <span>Level 15</span>
                <span>$1,000,000 (Grand Prize!)</span>
              </PrizeLevel>
            </PrizeLadder>
            <SectionText>
              <strong>Safe Havens:</strong> At levels 5 and 10, you're guaranteed to keep at least that amount even if you answer incorrectly later.
            </SectionText>
          </TutorialSection>

          <TutorialSection>
            <SectionTitle>🆘 Lifelines</SectionTitle>
            <LifelineGrid>
              <LifelineCard>
                <LifelineIcon>50:50</LifelineIcon>
                <LifelineName>50:50</LifelineName>
                <LifelineDesc>Eliminates two wrong answers, leaving you with the correct answer and one wrong answer.</LifelineDesc>
              </LifelineCard>
              <LifelineCard>
                <LifelineIcon>📞</LifelineIcon>
                <LifelineName>Phone a Friend</LifelineName>
                <LifelineDesc>Call a friend for help (simulated - gives you a hint based on the question).</LifelineDesc>
              </LifelineCard>
              <LifelineCard>
                <LifelineIcon>👥</LifelineIcon>
                <LifelineName>Ask the Audience</LifelineName>
                <LifelineDesc>See how the audience would vote (simulated - shows percentage distribution).</LifelineDesc>
              </LifelineCard>
            </LifelineGrid>
          </TutorialSection>

          <TutorialSection>
            <SectionTitle>🎮 Gameplay</SectionTitle>
            <SectionText>
              • Read each question carefully<br/>
              • Select your answer by clicking on it<br/>
              • Use lifelines if you're unsure<br/>
              • You can quit at any time to keep your current winnings<br/>
              • Answer incorrectly and you lose everything (unless at a safe haven)
            </SectionText>
          </TutorialSection>

          <TutorialSection>
            <SectionTitle>🏆 Tips</SectionTitle>
            <SectionText>
              • Take your time - there's no time limit<br/>
              • Use lifelines strategically<br/>
              • Trust your instincts<br/>
              • Remember: you can always quit and keep your money!
            </SectionText>
          </TutorialSection>
        </TutorialContent>

        <TutorialActions>
          <TutorialButton className="secondary" onClick={onClose}>
            Close Tutorial
          </TutorialButton>
          <TutorialButton className="primary" onClick={onStartGame}>
            Start Playing!
          </TutorialButton>
        </TutorialActions>
      </TutorialModal>
    </TutorialOverlay>
  );
};

export default Tutorial;
