# Who Wants to Be a Shillionaire

A desktop trivia game built with Electron, React, and Python Flask backend. Test your knowledge and compete for cryptocurrency prizes!

## 🎮 Features

### Desktop Application
- **Cross-platform support** - Windows, macOS, and Linux
- **System tray integration** - Minimize to tray and quick access
- **Auto-update functionality** - Automatic updates when new versions are available
- **Native menus** - Full menu system with keyboard shortcuts
- **Windows-specific enhancements**:
  - Taskbar flashing notifications
  - Balloon notifications
  - Always on top option
  - Hide to tray functionality

### Game Features
- **Multiple difficulty levels** - Easy, Normal, and Hard
- **Sound and music controls** - Toggle sound effects and background music
- **High score tracking** - Local score persistence
- **Responsive design** - Optimized for desktop experience

### Technical Features
- **Modern architecture** - Electron with React frontend
- **Secure communication** - Context isolation and preload scripts
- **Persistent settings** - User preferences saved locally
- **Development tools** - Hot reload and debugging support

## 🚀 Quick Start

### Prerequisites
- Node.js 18+ 
- Python 3.8+
- npm or yarn

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd whowants
   ```

2. **Install backend dependencies**
   ```bash
   cd backend
   pip install -r requirements.txt
   ```

3. **Install frontend dependencies**
   ```bash
   cd ../frontend
   npm install
   ```

4. **Install desktop app dependencies**
   ```bash
   cd ../desktop-app
   npm install
   ```

### Running the Application

1. **Start the backend server**
   ```bash
   cd backend
   python app.py
   ```

2. **Start the frontend (for development)**
   ```bash
   cd frontend
   npm start
   ```

3. **Start the desktop application**
   ```bash
   cd desktop-app
   npm start
   ```

### Building for Production

1. **Build the React frontend**
   ```bash
   cd desktop-app
   npm run build:react
   ```

2. **Build the desktop executable**
   ```bash
   # Windows
   npm run build:win
   
   # macOS
   npm run build:mac
   
   # Linux
   npm run build:linux
   
   # All platforms
   npm run pack
   ```

## 📁 Project Structure

```
whowants/
├── backend/                 # Python Flask API server
│   ├── app.py              # Main Flask application
│   ├── requirements.txt    # Python dependencies
│   └── ...
├── frontend/               # React web application
│   ├── src/               # React source code
│   ├── public/            # Static assets
│   ├── package.json       # Frontend dependencies
│   └── ...
├── desktop-app/           # Electron desktop application
│   ├── src/               # Electron main process
│   │   ├── main.js        # Main Electron process
│   │   ├── preload.js     # Preload script
│   │   └── updater.js     # Auto-update functionality
│   ├── assets/            # Desktop app assets
│   ├── build/             # Built React app (generated)
│   ├── dist/              # Built executables (generated)
│   └── package.json       # Desktop app dependencies
└── README.md              # This file
```

## 🛠️ Development

### Available Scripts

**Backend:**
- `python app.py` - Start Flask development server

**Frontend:**
- `npm start` - Start React development server
- `npm run build` - Build React app for production

**Desktop App:**
- `npm start` - Start Electron app
- `npm run dev` - Start with hot reload
- `npm run build:react` - Build React app into desktop app
- `npm run build` - Build Windows executable
- `npm run pack` - Build React app and executable

### Development Workflow

1. Start the backend server for API functionality
2. For web development: Start the frontend React server
3. For desktop development: Build React app and start Electron
4. Use the built-in developer tools for debugging

## 🔧 Configuration

### Auto-Updates
The desktop app includes auto-update functionality. Configure update servers in `desktop-app/src/updater.js`.

### Settings Storage
User preferences are stored locally using `electron-store`:
- Sound settings
- Difficulty preferences
- Window state
- Game progress

## 🚀 Deployment

### GitHub Actions
The project includes automated CI/CD with GitHub Actions:
- Builds for Windows, macOS, and Linux
- Automatic releases on version tags
- Artifact uploads for each platform

### Manual Deployment
1. Build the application for your target platform
2. Distribute the executable from the `dist/` folder
3. Set up update server for auto-update functionality

## 🎯 Game Rules

1. Answer trivia questions to progress through levels
2. Each correct answer increases your potential winnings
3. Use lifelines when you're unsure:
   - 50/50: Remove two incorrect answers
   - Ask the Audience: See what others would choose
   - Phone a Friend: Get expert advice
4. Walk away at any time to keep your current winnings
5. One wrong answer and you lose everything!

## 🏆 Scoring System

- **Easy Mode**: Lower point values, more forgiving
- **Normal Mode**: Standard scoring system
- **Hard Mode**: Higher stakes, maximum rewards

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🐛 Troubleshooting

### Common Issues

**App won't start:**
- Ensure all dependencies are installed
- Check that ports 3000 and 5000 are available
- Verify Python and Node.js versions

**Build failures:**
- Clear node_modules and reinstall dependencies
- Check that all required build tools are installed
- Ensure sufficient disk space for builds

**Auto-update issues:**
- Verify update server configuration
- Check network connectivity
- Review update server logs

## 📞 Support

For support and questions:
- Create an issue on GitHub
- Check the troubleshooting section
- Review the development documentation

---

**Enjoy playing Who Wants to Be a Shillionaire!** 🎉
