import React from 'react';
import styled, { keyframes } from 'styled-components';

const spin = keyframes`
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
`;

const SpinnerContainer = styled.div`
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  flex-direction: column;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 9999;
`;

const Spinner = styled.div`
  border: 4px solid rgba(247, 147, 30, 0.3);
  border-top: 4px solid #f7931e;
  border-radius: 50%;
  width: 60px;
  height: 60px;
  animation: ${spin} 1s linear infinite;
  margin-bottom: 20px;
  box-shadow: 0 0 20px rgba(247, 147, 30, 0.5);
`;

const LoadingText = styled.p`
  color: #f7931e;
  font-size: 18px;
  font-weight: bold;
  text-shadow: 0 0 10px rgba(247, 147, 30, 0.5);
  animation: pulse 2s ease-in-out infinite;
  
  @keyframes pulse {
    0%, 100% { opacity: 0.8; }
    50% { opacity: 1; }
  }
`;

const LoadingSpinner = ({ text = "Loading..." }) => {
  return (
    <SpinnerContainer>
      <Spinner />
      <LoadingText>{text}</LoadingText>
    </SpinnerContainer>
  );
};

export default LoadingSpinner;