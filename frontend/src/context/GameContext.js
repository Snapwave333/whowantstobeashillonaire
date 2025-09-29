import React, { createContext, useContext, useReducer, useCallback, useState } from 'react';
import axios from 'axios';

// Create axios instance with base configuration
const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Initial state
const initialState = {
  sessionId: null,
  sessionToken: null,
  currentQuestion: null,
  questionIndex: 0,
  prizeAmount: 0,
  score: 0,
  gameStatus: 'not_started', // not_started, playing, completed, failed
  categories: [],
  difficulties: [],
  lifelines: {
    fiftyFifty: true,
    phoneAFriend: true,
    askTheAudience: true
  },
  eliminatedAnswers: []
};

// Reducer function
function gameReducer(state, action) {
  switch (action.type) {
    case 'SET_CATEGORIES':
      return { ...state, categories: action.payload };
    case 'SET_DIFFICULTIES':
      return { ...state, difficulties: action.payload };
    case 'START_GAME':
      return {
        ...state,
        sessionId: action.payload.sessionId,
        sessionToken: action.payload.sessionToken,
        gameStatus: 'playing',
        questionIndex: 0,
        score: 0,
        lifelines: {
          fiftyFifty: true,
          phoneAFriend: true,
          askTheAudience: true
        },
        eliminatedAnswers: []
      };
    case 'SET_CURRENT_QUESTION':
      return {
        ...state,
        currentQuestion: action.payload.question,
        questionIndex: action.payload.questionIndex,
        prizeAmount: action.payload.prizeAmount,
        eliminatedAnswers: []
      };
    case 'SUBMIT_ANSWER':
      return {
        ...state,
        score: action.payload.score,
        gameStatus: action.payload.gameStatus
      };
    case 'USE_LIFELINE':
      return {
        ...state,
        lifelines: {
          ...state.lifelines,
          [action.payload.lifeline]: false
        },
        eliminatedAnswers: action.payload.eliminatedAnswers || state.eliminatedAnswers
      };
    case 'RESET_GAME':
      return initialState;
    default:
      return state;
  }
}

// Create context
const GameContext = createContext();

// Provider component
export function GameProvider({ children }) {
  const [state, dispatch] = useReducer(gameReducer, initialState);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Clear error when starting new actions
  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const fetchCategories = useCallback(async () => {
    try {
      setLoading(true);
      clearError();
      const response = await api.get('/questions/categories');
      dispatch({ type: 'SET_CATEGORIES', payload: response.data.categories });
    } catch (error) {
      setError('Failed to fetch categories');
      console.error('Error fetching categories:', error);
    } finally {
      setLoading(false);
    }
  }, [clearError]);

  const fetchDifficulties = useCallback(async () => {
    try {
      setLoading(true);
      clearError();
      const response = await api.get('/questions/difficulties');
      dispatch({ type: 'SET_DIFFICULTIES', payload: response.data.difficulties });
    } catch (error) {
      setError('Failed to fetch difficulties');
      console.error('Error fetching difficulties:', error);
    } finally {
      setLoading(false);
    }
  }, [clearError]);

  const startGame = useCallback(async (playerName = "Live Player", difficulty = null, category = null) => {
    try {
      clearError();
      const response = await api.post('/game/start', {
        player_name: playerName,
        difficulty_id: difficulty,
        category_id: category
      });
      
      dispatch({
        type: 'START_GAME',
        payload: {
          sessionId: response.data.session_id,
          sessionToken: response.data.session_token
        }
      });
      
      return response.data;
    } catch (error) {
      setError('Failed to start game');
      console.error('Error starting game:', error);
      throw error;
    }
  }, [clearError]);

  const getQuestion = useCallback(async () => {
    if (!state.sessionId || !state.sessionToken) {
      setError('No active game session');
      return;
    }

    try {
      setLoading(true);
      clearError();
      const response = await api.get(`/game/question/${state.sessionId}`, {
        headers: {
          'Authorization': `Bearer ${state.sessionToken}`
        }
      });
      
      dispatch({
        type: 'SET_CURRENT_QUESTION',
        payload: {
          question: response.data.question,
          questionIndex: response.data.session.current_question_index,
          prizeAmount: response.data.session.current_prize_amount
        }
      });
      
      return response.data;
    } catch (error) {
      setError('Failed to get question');
      console.error('Error getting question:', error);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [state.sessionId, state.sessionToken, clearError]);

  const submitAnswer = useCallback(async (answerId) => {
    if (!state.sessionToken || !state.currentQuestion?.id) {
      setError('Invalid game state for answer submission');
      return;
    }

    try {
      setLoading(true);
      clearError();
      const response = await api.post('/game/answer', {
        session_token: state.sessionToken,
        question_id: state.currentQuestion.id,
        answer_id: answerId
      });
      
      dispatch({
        type: 'SUBMIT_ANSWER',
        payload: {
          score: response.data.session.score,
          gameStatus: response.data.session.status
        }
      });
      
      return response.data;
    } catch (error) {
      setError('Failed to submit answer');
      console.error('Error submitting answer:', error);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [state.sessionToken, state.currentQuestion?.id, clearError]);

  const submitFiftyFifty = useCallback(async () => {
    if (!state.sessionToken || !state.currentQuestion?.id) {
      setError('Invalid game state for lifeline usage');
      return;
    }

    try {
      setLoading(true);
      clearError();
      const response = await api.post('/game/lifeline/fifty-fifty', {
        session_token: state.sessionToken,
        question_id: state.currentQuestion.id
      });
      
      dispatch({
        type: 'USE_LIFELINE',
        payload: {
          lifeline: 'fiftyFifty',
          eliminatedAnswers: response.data.eliminated_answers
        }
      });
      
      return response.data;
    } catch (error) {
      setError('Failed to use 50/50 lifeline');
      console.error('Error using 50/50 lifeline:', error);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [state.sessionToken, state.currentQuestion?.id, clearError]);

  const resetGame = useCallback(() => {
    dispatch({ type: 'RESET_GAME' });
    clearError();
  }, [clearError]);

  const value = {
    ...state,
    loading,
    error,
    fetchCategories,
    fetchDifficulties,
    startGame,
    getQuestion,
    submitAnswer,
    submitFiftyFifty,
    resetGame,
    clearError
  };

  return (
    <GameContext.Provider value={value}>
      {children}
    </GameContext.Provider>
  );
}

// Custom hook to use the game context
export function useGame() {
  const context = useContext(GameContext);
  if (!context) {
    throw new Error('useGame must be used within a GameProvider');
  }
  return context;
}

export default GameContext;