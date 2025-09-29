# Who Wants to Be a Shillonair - Game Logic Pseudocode

## Core Game State Structure
```
GameState = {
    currentTier: Integer (0-15)
    currentPrize: CryptoAmount
    safeHavens: Array[Integer] (e.g., [5, 10])
    usedLifelines: Array[String] (["50/50", "AskDiscord", "PhoneScammer"])
    currentQuestion: Question
    gameActive: Boolean
    lastSaveTime: Timestamp
    selectedCryptocurrencies: Array[String] (["BTC", "ETH", "SHILL"])
}
```

## Main Game Loop

```
Function StartNewGame(settings: GameMasterSettings) -> GameState:
    Initialize new game state
    Set currentTier = 0
    Set currentPrize = settings.basePrize * settings.prizeMultiplier
    Set safeHavens = settings.safeHavenTiers
    Set usedLifelines = []
    Set gameActive = true
    Load question for tier 0
    Start auto-save timer (settings.autoSaveInterval)
    Return game state

Function ProgressToNextTier(gameState: GameState) -> GameState:
    If currentTier >= settings.maxTiers - 1:
        Return EndGame(gameState, "WIN")

    gameState.currentTier += 1
    gameState.currentPrize = CalculatePrizeForTier(gameState.currentTier)
    LoadQuestionForTier(gameState.currentTier)
    TriggerAutoSave(gameState)
    Return gameState

Function SubmitAnswer(gameState: GameState, selectedAnswer: String) -> GameState:
    If selectedAnswer == gameState.currentQuestion.correctAnswer:
        Return ProgressToNextTier(gameState)
    Else:
        Return HandleWrongAnswer(gameState)

Function HandleWrongAnswer(gameState: GameState) -> GameState:
    gameState.gameActive = false
    lastSafeHavenPrize = CalculatePrizeForLastSafeHaven(gameState.currentTier)
    gameState.currentPrize = lastSafeHavenPrize
    TriggerCustomReveal(gameState.currentQuestion, "INCORRECT")
    Return gameState
```

## Prize Calculation System

```
Function CalculatePrizeForTier(tier: Integer) -> CryptoAmount:
    baseUSD = GetBasePrizeForTier(tier)
    adjustedUSD = baseUSD * settings.prizeScaleMultiplier
    selectedCrypto = GetRandomCryptoFromSettings()
    cryptoAmount = ConvertUSDToCrypto(adjustedUSD, selectedCrypto)
    Return {
        usdValue: adjustedUSD,
        cryptoAmount: cryptoAmount,
        cryptoSymbol: selectedCrypto
    }

Function GetBasePrizeForTier(tier: Integer) -> Float:
    // Exponential scaling with safe haven bonuses
    basePrizes = [100, 200, 300, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000, 125000, 250000, 500000, 1000000]
    return basePrizes[tier] || 1000000

Function ConvertUSDToCrypto(usdAmount: Float, cryptoSymbol: String) -> Float:
    currentPrice = GetCryptoPrice(cryptoSymbol)
    return usdAmount / currentPrice

Function GetCryptoPrice(cryptoSymbol: String) -> Float:
    // Real-time API call or cached value
    return CryptoAPI.GetPrice(cryptoSymbol)
```

## Lifeline System

```
Function UseLifeline(gameState: GameState, lifelineType: String) -> GameState:
    If lifelineType not in gameState.usedLifelines:
        gameState.usedLifelines.append(lifelineType)

        Switch(lifelineType):
            Case "50/50":
                Return Apply5050Lifeline(gameState)
            Case "AskDiscord":
                Return ApplyAskDiscordLifeline(gameState)
            Case "PhoneScammer":
                Return ApplyPhoneScammerLifeline(gameState)

    Return gameState

Function Apply5050Lifeline(gameState: GameState) -> GameState:
    // Remove two incorrect answers, leave correct + one wrong
    correctAnswer = gameState.currentQuestion.correctAnswer
    wrongAnswers = [a for a in gameState.currentQuestion.answers if a != correctAnswer]
    keepWrongAnswer = SelectRandom(wrongAnswers)
    gameState.currentQuestion.answers = [correctAnswer, keepWrongAnswer]
    Shuffle(gameState.currentQuestion.answers)
    Return gameState

Function ApplyAskDiscordLifeline(gameState: GameState) -> GameState:
    // Launch poll in Discord/stream chat
    pollResults = LaunchDiscordPoll(gameState.currentQuestion)
    gameState.discordPollResults = pollResults
    Return gameState

Function ApplyPhoneScammerLifeline(gameState: GameState) -> GameState:
    // Simulate phone call segment (UI displays "calling..." animation)
    gameState.phoneCallActive = true
    // In real implementation, this would trigger a stream segment
    Return gameState
```

## Auto-Save System

```
Function TriggerAutoSave(gameState: GameState):
    gameState.lastSaveTime = CurrentTimestamp()
    SaveGameStateToStorage(gameState)
    ScheduleNextAutoSave(settings.autoSaveInterval)

Function LoadLastSavedSession() -> GameState:
    savedState = LoadGameStateFromStorage()
    If savedState exists:
        Return savedState
    Else:
        Return null

Function SaveGameStateToStorage(gameState: GameState):
    serializedState = JSON.Serialize(gameState)
    // Save to local storage or cloud storage
    Storage.Save("whowants_savestate", serializedState)

Function LoadGameStateFromStorage() -> GameState:
    serializedState = Storage.Load("whowants_savestate")
    If serializedState exists:
        Return JSON.Deserialize(serializedState)
    Return null
```

## Question Management

```
Function LoadQuestionForTier(tier: Integer) -> Question:
    difficulty = GetDifficultyForTier(tier)
    category = SelectRandomCategory(difficulty)
    question = QuestionBank.GetQuestion(category, difficulty)

    Return {
        id: question.id,
        text: question.text,
        answers: ShuffleArray(question.answers),
        correctAnswer: question.correctAnswer,
        category: question.category,
        tier: tier,
        customReveal: GetCustomRevealForTier(tier)
    }

Function GetDifficultyForTier(tier: Integer) -> String:
    If tier <= 5: Return "Easy"
    If tier <= 10: Return "Medium"
    Return "Hard"

Function GetCustomRevealForTier(tier: Integer) -> CustomReveal:
    return settings.customReveals[tier] || null
```

## Custom Answer Reveal System

```
Function TriggerCustomReveal(question: Question, result: String):
    reveal = question.customReveal
    If reveal exists:
        DisplayCustomImage(reveal.image)
        PlayCustomSound(reveal.sound)
        If result == "CORRECT":
            ShowCelebrationAnimation()
        Else:
            ShowFailureAnimation()

Function DisplayCustomImage(imagePath: String):
    // Load and display uploaded image
    overlay.ShowImage(imagePath)

Function PlayCustomSound(soundPath: String):
    // Play uploaded sound effect
    audioSystem.PlaySound(soundPath)
```

## Host Control Functions

```
Function LockInFinalAnswer(gameState: GameState, answer: String) -> GameState:
    gameState.finalAnswer = answer
    gameState.waitingForReveal = true
    Return gameState

Function RevealAnswer(gameState: GameState) -> GameState:
    gameState.waitingForReveal = false
    isCorrect = gameState.finalAnswer == gameState.currentQuestion.correctAnswer

    If isCorrect:
        TriggerCustomReveal(gameState.currentQuestion, "CORRECT")
        Return ProgressToNextTier(gameState)
    Else:
        TriggerCustomReveal(gameState.currentQuestion, "INCORRECT")
        Return HandleWrongAnswer(gameState)

Function PauseGame(gameState: GameState) -> GameState:
    gameState.isPaused = true
    Return gameState

Function ResumeGame(gameState: GameState) -> GameState:
    gameState.isPaused = false
    Return gameState
```

## Error Handling and Edge Cases

```
Function HandleTechnicalIssue(gameState: GameState, error: String) -> GameState:
    // Auto-save current state
    TriggerAutoSave(gameState)

    // Log error for debugging
    LogError(error)

    // Return recoverable state
    Return gameState

Function ValidateGameState(gameState: GameState) -> Boolean:
    // Ensure all required fields are present
    requiredFields = ["currentTier", "currentPrize", "gameActive"]
    For each field in requiredFields:
        If field not in gameState:
            Return false
    Return true
```

## Cryptocurrency Display Formatting

```
Function FormatCryptoDisplay(cryptoAmount: CryptoAmount) -> String:
    formattedCrypto = FormatNumber(cryptoAmount.cryptoAmount, 8) // 8 decimal places
    formattedUSD = FormatCurrency(cryptoAmount.usdValue)

    Return "${formattedCrypto} ${cryptoAmount.cryptoSymbol} (${formattedUSD} USD)"

Function UpdateCryptoPrices(gameState: GameState) -> GameState:
    // Update all prize displays with current prices
    For each tier in 0 to settings.maxTiers:
        gameState.pumpLadder[tier].cryptoAmount = RecalculateForTier(tier)
    Return gameState
