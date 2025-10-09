# Changelog

All notable changes to "Who Wants to Be a Shillionaire" will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.1] - 2024-10-09

### Added
- 🎮 **Initial Release**: Complete desktop trivia game application
- 🤖 **AI Integration**: Google Gemini AI for dynamic question generation
- 🎯 **Dual-Window Interface**: Separate Game Master and Contestant views
- 🏆 **Customizable Prize Ladder**: Edit prizes, images, sounds, and display text
- 🎨 **Modern UI**: Dark theme with cinematic lighting and animations
- 🔧 **Game Master Controls**: Full host control panel with API configuration
- 📱 **Responsive Design**: Optimized for various screen sizes
- 🎵 **Audio Support**: Sound effects and ambient audio integration
- 🎲 **Question Categories**: Multiple categories including Random AI selection
- 📊 **Scaling Difficulty**: Automatic difficulty progression from easy to expert
- 🔄 **Lifelines**: 50/50, Phone a Friend, Ask the Audience
- 💾 **Settings Persistence**: Save API keys and game preferences
- 🖥️ **Cross-Platform**: Electron-based desktop application
- 📦 **Installer**: Windows installer with Squirrel.Windows
- 🎯 **Settings Button**: Easy access to Game Master settings from contestant view

### Technical Features
- **Backend**: Flask API with SQLAlchemy database
- **Frontend**: React.js with modern hooks and context
- **Desktop**: Electron with dual-window architecture
- **AI**: Gemini AI integration with fallback questions
- **Build**: Electron Forge with Squirrel.Windows installer
- **Icons**: Custom application icons and branding

### Game Features
- **Question Generation**: AI-powered questions with scaling difficulty
- **Prize Management**: Customizable prize ladder with visual editing
- **Game Flow**: Complete game progression with lifeline integration
- **Host Controls**: Full game master interface with real-time updates
- **Contestant Experience**: Clean, focused player interface
- **Progress Tracking**: Real-time game state management

### UI/UX Features
- **Modern Design**: Sleek dark theme with gold accents
- **Animations**: Smooth transitions and particle effects
- **Responsive Layout**: Adapts to different screen sizes
- **Accessibility**: Keyboard navigation and screen reader support
- **Visual Feedback**: Progress bars, status indicators, and alerts

### Installation
- **Windows Installer**: Professional installer with custom branding
- **Portable Version**: Standalone executable for easy distribution
- **Development Setup**: Complete development environment configuration

---

## Future Releases

### Planned Features
- [ ] **Multiplayer Support**: Online multiplayer functionality
- [ ] **Question Database**: Local question database with import/export
- [ ] **Custom Themes**: Multiple visual themes and color schemes
- [ ] **Statistics Tracking**: Player performance analytics
- [ ] **Tournament Mode**: Multi-round tournament support
- [ ] **Mobile App**: React Native mobile application
- [ ] **Cloud Sync**: Cross-device game state synchronization
- [ ] **Voice Integration**: Text-to-speech for questions and answers

### Known Issues
- [ ] **Icon Format**: Some icon formats may not display correctly in Windows
- [ ] **API Rate Limits**: Gemini AI rate limiting may affect question generation
- [ ] **Memory Usage**: Large image assets may impact performance on older systems

---

**Note**: This is the initial beta release. Please report any issues or suggestions through GitHub Issues.
