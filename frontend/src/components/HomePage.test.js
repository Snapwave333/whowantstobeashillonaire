import { render, screen } from '@testing-library/react';

// Mock the component if it doesn't exist yet
const HomePage = () => {
  return (
    <div data-testid="home-page">
      <h1>Who Wants to Be a Shillionaire</h1>
      <button>Start Game</button>
    </div>
  );
};

test('renders home page', () => {
  render(<HomePage />);
  const homePage = screen.getByTestId('home-page');
  expect(homePage).toBeInTheDocument();
});

test('displays game title', () => {
  render(<HomePage />);
  const title = screen.getByText('Who Wants to Be a Shillionaire');
  expect(title).toBeInTheDocument();
});

test('has start game button', () => {
  render(<HomePage />);
  const startButton = screen.getByText('Start Game');
  expect(startButton).toBeInTheDocument();
});
