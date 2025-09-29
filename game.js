// Who Wants to Be a Shillonair - Game Logic

class ShillonairGame {
    constructor() {
        this.currentTier = 0;
        this.currentPrize = { usd: 100, crypto: "0.001778", symbol: "BTC" };
        this.safeHavens = [5, 10];
        this.usedLifelines = [];
        this.gameActive = true;
        this.selectedAnswer = null;

        this.settings = {
            prizeMultiplier: 1.0,
            numTiers: 15,
            safeHavens: [5, 10],
            difficulty: 'general_medium',
            cryptocurrencies: ['BTC', 'ETH', 'SHILL']
        };

        this.questions = [
            {
                id: 1,
                text: "What is the capital of France?",
                answers: ["London", "Berlin", "Paris", "Madrid"],
                correctAnswer: "Paris",
                category: "geography",
                difficulty: "easy"
            },
            {
                id: 2,
                text: "Which planet is known as the Red Planet?",
                answers: ["Venus", "Mars", "Jupiter", "Saturn"],
                correctAnswer: "Mars",
                category: "science",
                difficulty: "medium"
            },
            {
                id: 3,
                text: "Who painted the Mona Lisa?",
                answers: ["Vincent van Gogh", "Pablo Picasso", "Leonardo da Vinci", "Michelangelo"],
                correctAnswer: "Leonardo da Vinci",
                category: "art",
                difficulty: "medium"
            },
            {
                id: 4,
                text: "What is the largest mammal in the world?",
                answers: ["African Elephant", "Blue Whale", "Giraffe", "Polar Bear"],
                correctAnswer: "Blue Whale",
                category: "nature",
                difficulty: "easy"
            },
            {
                id: 5,
                text: "In which year did World War II end?",
                answers: ["1944", "1945", "1946", "1947"],
                correctAnswer: "1945",
                category: "history",
                difficulty: "medium"
            }
        ];

        this.currentQuestionIndex = 0;
        this.cryptoPrices = {
            BTC: 45000,
            ETH: 2500,
            SHILL: 100
        };

        this.init();
    }

    init() {
        this.bindEvents();
        this.updateDisplay();
        this.startAutoSave();
        console.log("🚀 Who Wants to Be a Shillonair - Game Launched!");
    }

    bindEvents() {
        // Answer selection
        document.querySelectorAll('.answer').forEach(answer => {
            answer.addEventListener('click', (e) => {
                this.selectAnswer(e.target);
            });
        });

        // Lifeline usage
        document.querySelectorAll('.lifeline').forEach(lifeline => {
            lifeline.addEventListener('click', (e) => {
                const lifelineType = e.currentTarget.dataset.lifeline;
                this.useLifeline(lifelineType);
            });
        });

        // Settings panel
        document.getElementById('settings-btn').addEventListener('click', () => {
            this.toggleSettings();
        });

        document.getElementById('close-settings').addEventListener('click', () => {
            this.toggleSettings();
        });

        document.getElementById('apply-settings').addEventListener('click', () => {
            this.applySettings();
        });

        document.getElementById('reset-settings').addEventListener('click', () => {
            this.resetSettings();
        });

        // Prize multiplier slider
        document.getElementById('prize-multiplier').addEventListener('input', (e) => {
            document.getElementById('multiplier-value').textContent = e.target.value + 'x';
        });
    }

    selectAnswer(answerElement) {
        if (!this.gameActive) return;

        // Remove previous selection
        document.querySelectorAll('.answer').forEach(ans => {
            ans.classList.remove('selected');
        });

        // Add selection to clicked answer
        answerElement.classList.add('selected');
        this.selectedAnswer = answerElement.dataset.letter;

        // Auto-submit after 2 seconds (simulating final answer)
        setTimeout(() => {
            this.submitAnswer();
        }, 2000);
    }

    submitAnswer() {
        if (!this.selectedAnswer || !this.gameActive) return;

        const currentQuestion = this.questions[this.currentQuestionIndex];
        const isCorrect = this.selectedAnswer === this.getAnswerLetter(currentQuestion.correctAnswer);

        if (isCorrect) {
            this.handleCorrectAnswer();
        } else {
            this.handleWrongAnswer();
        }
    }

    handleCorrectAnswer() {
        // Animate correct answer
        const selectedAnswerElement = document.querySelector(`.answer[data-letter="${this.selectedAnswer}"]`);
        selectedAnswerElement.classList.add('correct');

        setTimeout(() => {
            selectedAnswerElement.classList.remove('correct');
            this.progressToNextTier();
        }, 1500);
    }

    handleWrongAnswer() {
        this.gameActive = false;

        // Animate wrong answer
        const selectedAnswerElement = document.querySelector(`.answer[data-letter="${this.selectedAnswer}"]`);
        selectedAnswerElement.classList.add('wrong');

        // Show correct answer
        setTimeout(() => {
            const currentQuestion = this.questions[this.currentQuestionIndex];
            const correctLetter = this.getAnswerLetter(currentQuestion.correctAnswer);
            const correctElement = document.querySelector(`.answer[data-letter="${correctLetter}"]`);
            correctElement.style.background = 'linear-gradient(145deg, #00ff88, #00cc66)';
            correctElement.style.borderColor = '#00ff88';

            // Calculate safe haven prize
            const safeHavenPrize = this.getSafeHavenPrize();
            this.showGameOver(safeHavenPrize);
        }, 2000);
    }

    progressToNextTier() {
        if (this.currentTier >= 14) {
            this.showVictory();
            return;
        }

        this.currentTier++;
        this.currentQuestionIndex++;
        this.updatePrize();
        this.updateDisplay();
        this.loadNextQuestion();

        // Trigger auto-save
        this.saveGameState();
    }

    updatePrize() {
        const basePrizes = [100, 200, 300, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000, 125000, 250000, 500000, 1000000];
        const basePrize = basePrizes[this.currentTier] || 1000000;
        const adjustedPrize = Math.floor(basePrize * this.settings.prizeMultiplier);

        // Random crypto selection
        const cryptos = this.settings.cryptocurrencies.filter(c => c !== 'SHILL');
        const randomCrypto = cryptos[Math.floor(Math.random() * cryptos.length)];

        this.currentPrize = {
            usd: adjustedPrize,
            crypto: (adjustedPrize / this.cryptoPrices[randomCrypto]).toFixed(6),
            symbol: randomCrypto
        };
    }

    loadNextQuestion() {
        if (this.currentQuestionIndex >= this.questions.length) {
            // Generate a simple new question for demo
            const question = this.generateDemoQuestion();
            this.questions.push(question);
        }

        this.selectedAnswer = null;
        document.querySelectorAll('.answer').forEach(ans => {
            ans.classList.remove('selected');
        });
    }

    generateDemoQuestion() {
        const templates = [
            {
                text: "What is the largest country in the world by land area?",
                answers: ["Russia", "Canada", "China", "United States"],
                correct: "Russia"
            },
            {
                text: "Which element has the chemical symbol 'O'?",
                answers: ["Gold", "Oxygen", "Silver", "Iron"],
                correct: "Oxygen"
            },
            {
                text: "Who wrote 'Romeo and Juliet'?",
                answers: ["Charles Dickens", "William Shakespeare", "Jane Austen", "Mark Twain"],
                correct: "William Shakespeare"
            }
        ];

        const template = templates[Math.floor(Math.random() * templates.length)];
        return {
            id: Date.now(),
            text: template.text,
            answers: template.answers,
            correctAnswer: template.correct,
            category: "general",
            difficulty: "medium"
        };
    }

    useLifeline(lifelineType) {
        if (this.usedLifelines.includes(lifelineType) || !this.gameActive) return;

        const lifelineElement = document.querySelector(`[data-lifeline="${lifelineType}"]`);
        lifelineElement.classList.add('lifeline-used');

        switch(lifelineType) {
            case '50/50':
                this.use5050Lifeline();
                break;
            case 'ask':
                this.useAskDiscordLifeline();
                break;
            case 'phone':
                this.usePhoneScammerLifeline();
                break;
        }

        this.usedLifelines.push(lifelineType);
        lifelineElement.classList.add('used');

        setTimeout(() => {
            lifelineElement.classList.remove('lifeline-used');
        }, 500);
    }

    use5050Lifeline() {
        const currentQuestion = this.questions[this.currentQuestionIndex];
        const correctAnswer = currentQuestion.correctAnswer;
        const wrongAnswers = currentQuestion.answers.filter(a => a !== correctAnswer);

        // Remove two wrong answers
        const answersToRemove = wrongAnswers.sort(() => 0.5 - Math.random()).slice(0, 2);

        document.querySelectorAll('.answer').forEach(answerElement => {
            const answerText = answerElement.textContent.replace(/^[A-D]\)\s*/, '');
            if (answersToRemove.includes(answerText) && answerText !== correctAnswer) {
                answerElement.style.opacity = '0.3';
                answerElement.style.pointerEvents = 'none';
            }
        });
    }

    useAskDiscordLifeline() {
        // Simulate Discord poll
        const pollResults = this.simulateDiscordPoll();
        const resultText = `Discord Poll Results:\n${Object.entries(pollResults)
            .map(([letter, votes]) => `${letter}: ${votes} votes`)
            .join('\n')}`;

        this.showPollResults(resultText);
    }

    usePhoneScammerLifeline() {
        // Simulate phone call
        this.showPhoneCall();
    }

    simulateDiscordPoll() {
        const currentQuestion = this.questions[this.currentQuestionIndex];
        const correctAnswer = currentQuestion.correctAnswer;
        const correctLetter = this.getAnswerLetter(correctAnswer);

        // Simulate poll with bias toward correct answer
        const results = { A: 0, B: 0, C: 0, D: 0 };
        const totalVotes = 100;

        // Give correct answer majority
        results[correctLetter] = Math.floor(totalVotes * 0.6);
        const remainingVotes = totalVotes - results[correctLetter];

        // Distribute remaining votes randomly
        const otherLetters = ['A', 'B', 'C', 'D'].filter(l => l !== correctLetter);
        otherLetters.forEach((letter, index) => {
            const votes = Math.floor(remainingVotes / otherLetters.length);
            results[letter] = index === otherLetters.length - 1 ? remainingVotes : votes;
            remainingVotes -= votes;
        });

        return results;
    }

    getAnswerLetter(answerText) {
        const currentQuestion = this.questions[this.currentQuestionIndex];
        const answerIndex = currentQuestion.answers.indexOf(answerText);
        return ['A', 'B', 'C', 'D'][answerIndex];
    }

    showPollResults(results) {
        // Create temporary popup
        const popup = document.createElement('div');
        popup.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: linear-gradient(145deg, #1a1a1a, #2a2a2a);
            border: 2px solid #9b59b6;
            border-radius: 15px;
            padding: 20px;
            color: white;
            font-family: 'Roboto Condensed', monospace;
            z-index: 1001;
            text-align: center;
            box-shadow: 0 0 20px rgba(155, 89, 182, 0.5);
        `;
        popup.innerHTML = `
            <h3>Discord Poll Results</h3>
            <pre style="font-family: inherit; margin: 15px 0;">${results}</pre>
            <button onclick="this.parentElement.remove()" style="
                background: linear-gradient(145deg, #9b59b6, #bb7eff);
                border: none;
                border-radius: 8px;
                padding: 10px 20px;
                color: white;
                cursor: pointer;
            ">CLOSE</button>
        `;

        document.body.appendChild(popup);
    }

    showPhoneCall() {
        // Create phone call simulation
        const popup = document.createElement('div');
        popup.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: linear-gradient(145deg, #1a1a1a, #2a2a2a);
            border: 2px solid #00ff88;
            border-radius: 15px;
            padding: 20px;
            color: white;
            font-family: 'Roboto Condensed', monospace;
            z-index: 1001;
            text-align: center;
            box-shadow: 0 0 20px rgba(0, 255, 136, 0.5);
        `;
        popup.innerHTML = `
            <h3>📞 Calling Expert...</h3>
            <p style="margin: 20px 0; font-style: italic;">"I think the answer is ${this.getRandomAnswer()}..."</p>
            <button onclick="this.parentElement.remove()" style="
                background: linear-gradient(145deg, #00ff88, #00cc66);
                border: none;
                border-radius: 8px;
                padding: 10px 20px;
                color: white;
                cursor: pointer;
            ">HANG UP</button>
        `;

        document.body.appendChild(popup);
    }

    getRandomAnswer() {
        const currentQuestion = this.questions[this.currentQuestionIndex];
        return currentQuestion.answers[Math.floor(Math.random() * currentQuestion.answers.length)];
    }

    getSafeHavenPrize() {
        for (let i = this.currentTier; i >= 0; i--) {
            if (this.safeHavens.includes(i)) {
                const basePrizes = [100, 200, 300, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000, 125000, 250000, 500000, 1000000];
                return Math.floor(basePrizes[i] * this.settings.prizeMultiplier);
            }
        }
        return 0;
    }

    showGameOver(finalPrize) {
        const popup = document.createElement('div');
        popup.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: linear-gradient(145deg, #1a1a1a, #2a2a2a);
            border: 2px solid #ff6b6b;
            border-radius: 15px;
            padding: 30px;
            color: white;
            font-family: 'Roboto Condensed', monospace;
            z-index: 1001;
            text-align: center;
            box-shadow: 0 0 30px rgba(255, 107, 107, 0.5);
        `;
        popup.innerHTML = `
            <h2>GAME OVER</h2>
            <p style="margin: 20px 0; font-size: 18px;">You walked away with:</p>
            <p style="font-size: 24px; color: #ffd700; margin: 10px 0;">$${finalPrize.toLocaleString()} USD</p>
            <p style="margin: 20px 0;">Better luck next time!</p>
            <button onclick="location.reload()" style="
                background: linear-gradient(145deg, #ff6b6b, #ff5252);
                border: none;
                border-radius: 8px;
                padding: 12px 24px;
                color: white;
                cursor: pointer;
                font-size: 16px;
            ">PLAY AGAIN</button>
        `;

        document.body.appendChild(popup);
    }

    showVictory() {
        const popup = document.createElement('div');
        popup.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: linear-gradient(145deg, #1a1a1a, #2a2a2a);
            border: 2px solid #ffd700;
            border-radius: 15px;
            padding: 30px;
            color: white;
            font-family: 'Roboto Condensed', monospace;
            z-index: 1001;
            text-align: center;
            box-shadow: 0 0 30px rgba(255, 215, 0, 0.5);
        `;
        popup.innerHTML = `
            <h2>🎉 SHILLIONAIRE! 🎉</h2>
            <p style="margin: 20px 0; font-size: 18px;">You won the jackpot!</p>
            <p style="font-size: 24px; color: #ffd700; margin: 10px 0;">${this.currentPrize.crypto} ${this.currentPrize.symbol}</p>
            <p style="font-size: 20px; color: #00ff88; margin: 10px 0;">$${this.currentPrize.usd.toLocaleString()} USD</p>
            <p style="margin: 20px 0;">Incredible performance!</p>
            <button onclick="location.reload()" style="
                background: linear-gradient(145deg, #ffd700, #ffed4e);
                border: none;
                border-radius: 8px;
                padding: 12px 24px;
                color: #0a0a0a;
                cursor: pointer;
                font-size: 16px;
                font-weight: 700;
            ">PLAY AGAIN</button>
        `;

        document.body.appendChild(popup);
    }

    updateDisplay() {
        // Update current prize display
        document.querySelector('.prize-amount').textContent = `${this.currentPrize.crypto} ${this.currentPrize.symbol}`;
        document.querySelector('.prize-usd').textContent = `$${this.currentPrize.usd.toLocaleString()} USD`;

        // Update question and answers
        const currentQuestion = this.questions[this.currentQuestionIndex];
        if (currentQuestion) {
            document.getElementById('question-text').textContent = currentQuestion.text;

            const answerElements = document.querySelectorAll('.answer');
            currentQuestion.answers.forEach((answer, index) => {
                const letter = ['A', 'B', 'C', 'D'][index];
                answerElements[index].textContent = `${letter}) ${answer}`;
                answerElements[index].dataset.letter = letter;
                answerElements[index].style.opacity = '1';
                answerElements[index].style.pointerEvents = 'auto';
            });
        }

        // Update lifeline status
        document.querySelectorAll('.lifeline').forEach(lifeline => {
            const lifelineType = lifeline.dataset.lifeline;
            if (this.usedLifelines.includes(lifelineType)) {
                lifeline.classList.add('used');
            } else {
                lifeline.classList.remove('used');
            }
        });
    }

    toggleSettings() {
        const settingsPanel = document.getElementById('settings-panel');
        settingsPanel.classList.toggle('hidden');
    }

    applySettings() {
        // Update settings from form
        this.settings.prizeMultiplier = parseFloat(document.getElementById('prize-multiplier').value);
        this.settings.numTiers = parseInt(document.getElementById('num-tiers').value);
        this.settings.difficulty = document.getElementById('difficulty').value;

        // Update safe havens
        this.settings.safeHavens = [];
        document.querySelectorAll('#safe-haven-tiers input:checked').forEach(checkbox => {
            this.settings.safeHavens.push(parseInt(checkbox.value));
        });

        // Update cryptocurrencies
        this.settings.cryptocurrencies = [];
        document.querySelectorAll('#crypto-selection input:checked').forEach(checkbox => {
            this.settings.cryptocurrencies.push(checkbox.value);
        });

        // Apply prize multiplier to current prize
        this.updatePrize();
        this.updateDisplay();
        this.saveGameState();
        this.toggleSettings();

        console.log('Settings applied:', this.settings);
    }

    resetSettings() {
        document.getElementById('prize-multiplier').value = 1.0;
        document.getElementById('multiplier-value').textContent = '1.0x';
        document.getElementById('num-tiers').value = 15;
        document.getElementById('difficulty').value = 'general_medium';

        document.querySelectorAll('#safe-haven-tiers input').forEach((checkbox, index) => {
            checkbox.checked = index < 2;
        });

        document.querySelectorAll('#crypto-selection input').forEach(checkbox => {
            checkbox.checked = ['BTC', 'ETH', 'SHILL'].includes(checkbox.value);
        });
    }

    startAutoSave() {
        // Auto-save every 30 seconds
        setInterval(() => {
            this.saveGameState();
        }, 30000);
    }

    saveGameState() {
        const gameState = {
            currentTier: this.currentTier,
            currentPrize: this.currentPrize,
            safeHavens: this.safeHavens,
            usedLifelines: this.usedLifelines,
            currentQuestionIndex: this.currentQuestionIndex,
            settings: this.settings,
            timestamp: Date.now()
        };

        localStorage.setItem('whowants_savestate', JSON.stringify(gameState));
        console.log('💾 Game state saved');
    }

    loadGameState() {
        const savedState = localStorage.getItem('whowants_savestate');
        if (savedState) {
            try {
                const gameState = JSON.parse(savedState);
                if (Date.now() - gameState.timestamp < 3600000) { // 1 hour
                    Object.assign(this, gameState);
                    this.updateDisplay();
                    console.log('📂 Game state loaded');
                    return true;
                }
            } catch (e) {
                console.error('Failed to load game state:', e);
            }
        }
        return false;
    }
}

// Launch the game when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.game = new ShillonairGame();

    // Try to load saved game state
    if (!window.game.loadGameState()) {
        console.log('🎮 Starting new game');
    }
});

// Global function for popup buttons
window.location = window.location;
