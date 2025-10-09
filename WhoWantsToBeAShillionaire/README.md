# Who Wants to Be a Shillionaire? - Windows C# WPF Application

A native Windows application built with C# and WPF, featuring a complete offline game experience with local SQLite database.

## Features

- **Native Windows Application**: Built with C# and WPF for optimal Windows integration
- **Local SQLite Database**: All questions and game data stored locally
- **Sound Effects**: Audio feedback for game actions
- **Complete Game Logic**: 15 questions across 3 difficulty levels
- **Lifelines**: 50:50, Phone a Friend, Ask the Audience
- **Prize Ladder**: Visual progress tracking
- **Admin Panel**: Add, edit, and manage questions
- **Professional UI**: Modern Windows-style interface

## Requirements

- Windows 10 or later
- .NET Framework 4.8 or .NET 6.0+
- Visual Studio 2019/2022 or Visual Studio Code with C# extension

## Building the Application

### Option 1: Using Visual Studio
1. Open `WhoWantsToBeAShillionaire.csproj` in Visual Studio
2. Build the solution (Ctrl+Shift+B)
3. Run the application (F5)

### Option 2: Using .NET CLI
```bash
dotnet restore
dotnet build
dotnet run
```

### Option 3: Using MSBuild (if available)
```bash
msbuild WhoWantsToBeAShillionaire.csproj
```

## Project Structure

```
WhoWantsToBeAShillionaire/
├── Models/
│   └── Question.cs              # Question data model
├── Services/
│   ├── DatabaseService.cs       # SQLite database operations
│   ├── AudioService.cs          # Sound effects management
│   └── GameLogic.cs             # Game rules and scoring
├── ViewModels/
│   └── MainViewModel.cs         # MVVM data binding
├── App.xaml                     # Application resources and styles
├── App.xaml.cs                  # Application startup logic
├── MainWindow.xaml              # Main UI layout
├── MainWindow.xaml.cs           # UI event handlers
└── WhoWantsToBeAShillionaire.csproj  # Project file
```

## Game Rules

1. **15 Questions**: Progress through increasingly difficult questions
2. **Prize Money**: Win up to $1,000,000
3. **Safe Havens**: Guaranteed amounts at $1,000, $32,000, and $1,000,000
4. **Lifelines**: Each can be used once per game
   - **50:50**: Remove two incorrect answers
   - **Phone a Friend**: Get advice from a friend
   - **Ask the Audience**: See audience poll results
5. **Walk Away**: Keep your current winnings at any time

## Database

The application uses SQLite for local data storage:
- Questions are categorized by difficulty (1=Easy, 2=Medium, 3=Hard)
- Default questions are automatically created on first run
- Admin panel allows adding/editing questions
- Data persists between game sessions

## Audio

Sound effects are generated programmatically if audio files are not found:
- Button clicks and menu navigation
- Correct/incorrect answer feedback
- Lifeline usage sounds
- Game start/end audio

## Customization

### Adding Questions
1. Use the Admin Panel in the application, or
2. Directly edit the SQLite database file
3. Questions should have difficulty levels 1-3

### Modifying Sounds
Place custom WAV files in the `Sounds/` directory:
- `button_click.wav`
- `correct_answer.wav`
- `wrong_answer.wav`
- `lifeline_use.wav`
- etc.

## Troubleshooting

### Build Issues
- Ensure .NET SDK is installed
- Check that all NuGet packages are restored
- Verify Windows SDK is available

### Runtime Issues
- Check that SQLite database can be created in the application directory
- Ensure audio device is available for sound effects
- Verify write permissions for the application folder

## License

This project is for educational and entertainment purposes.