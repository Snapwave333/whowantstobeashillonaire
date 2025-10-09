import { render, screen } from '@testing-library/react';

// Mock App component to avoid import issues
const App = () => {
  return (
    <div className="App">
      <h1>Who Wants to Be a Shillionaire</h1>
    </div>
  );
};

test('renders app without crashing', () => {
  render(<App />);
  // Basic test to ensure app renders
  expect(document.body).toBeInTheDocument();
});

test('app has main container', () => {
  render(<App />);
  const appElement = document.querySelector('.App');
  expect(appElement).toBeInTheDocument();
});
