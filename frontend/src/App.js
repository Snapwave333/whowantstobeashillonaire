import React, { Suspense, lazy } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import styled from 'styled-components';
import { GameProvider } from './context/GameContext';
import Header from './components/Header';
import LoadingSpinner from './components/LoadingSpinner';

// Lazy load components for code splitting
const GamePage = lazy(() => import('./components/GamePage'));
const GameOverPage = lazy(() => import('./components/GameOverPage'));

const AppContainer = styled.div`
  min-height: 100vh;
  display: flex;
  flex-direction: column;
`;

const MainContent = styled.main`
  flex: 1;
  display: flex;
  flex-direction: column;
`;

function App() {
  return (
    <GameProvider>
      <Router>
        <AppContainer>
          <Header />
          <MainContent>
            <Suspense fallback={<LoadingSpinner />}>
              <Routes>
                <Route path="/" element={<Navigate to="/game" replace />} />
                <Route path="/game" element={<GamePage />} />
                <Route path="/game-over" element={<GameOverPage />} />
                <Route path="*" element={<Navigate to="/game" replace />} />
              </Routes>
            </Suspense>
          </MainContent>
        </AppContainer>
      </Router>
    </GameProvider>
  );
}

export default App;