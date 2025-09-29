# Who Wants to Be a Shillonair - Visual Asset Design Specification

## Stream Overlay Design (1920x1080)

### Overall Layout Structure
```
┌─────────────────────────────────────────────────────────────────────────┐
│  HOST CAMERA FEED (400x300)            PUMP LADDER (300x600)            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │             │  │  TIER 15    │  │  TIER 14    │  │  TIER 13    │    │
│  │   HOST      │  │  1.0 BTC    │  │  0.5 BTC    │  │  0.25 BTC   │    │
│  │   VIDEO     │  │  $45,000    │  │  $22,500    │  │  $11,250    │    │
│  │             │  │  ▲ SAFE     │  │             │  │             │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │
│                                                                         │
│  QUESTION DISPLAY (1200x200)                                           │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │  Question: What is the capital of France?                          │ │
│  │                                                                     │ │
│  │  A) London          B) Berlin          C) Paris          D) Madrid  │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│  LIFELINE STATUS (400x100)                    CURRENT PRIZE (200x100)    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │  50/50      │  │  ASK THE    │  │  PHONE A    │  │  CURRENT    │    │
│  │  HODL       │  │  DISCORD    │  │  SCAMMER    │  │  PRIZE      │    │
│  │  AVAILABLE  │  │  AVAILABLE  │  │  AVAILABLE  │  │  0.1778 BTC │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  │  $8,000 USD  │    │
│                                                      └─────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

### Visual Design Specifications

#### Color Scheme
- **Primary Background**: Deep space black (#0a0a0a) with subtle starfield effect
- **Crypto Accent**: Neon green (#00ff88) for cryptocurrency elements
- **Prize Display**: Gold gradient (#ffd700 to #ffed4e) for prize amounts
- **Safe Havens**: Electric blue border (#00bfff) with pulsing animation
- **Text**: High contrast white (#ffffff) with subtle glow effects

#### Typography
- **Question Text**: Bold, 48pt futuristic font (Orbitron or similar)
- **Answer Options**: 36pt, clear sans-serif (Roboto Condensed)
- **Prize Amounts**: 32pt monospace for crypto amounts, 24pt for USD
- **Tier Labels**: 28pt, semi-bold

#### Animation Specifications
- **Prize Updates**: Smooth slide-up animation with crypto sparkle effect
- **Safe Haven Indicators**: Pulsing blue border with subtle glow
- **Correct Answer**: Green checkmark explosion with celebratory particles
- **Wrong Answer**: Red X with dramatic shake animation
- **Lifeline Usage**: Circular progress indicator with crypto-themed effects

## Pump Master Settings Panel Design

### Panel Layout (800x600 Modal)
```
┌─────────────────────────────────────────────────────────────────────────┐
│  PUMP MASTER CONTROL PANEL                          [X] [MINIMIZE]      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  GAME SETTINGS                                     [SAVE PRESET]        │
│  ┌─────────────────────────────────────┐  ┌─────────────────────────┐   │
│  │  Prize Scale Multiplier:            │  │        [1.0]            │   │
│  │  [████████████████████░░░░░░░░░░]   │  │  CURRENT: 1.5x         │   │
│  └─────────────────────────────────────┘  └─────────────────────────┘   │
│                                                                         │
│  Question Tiers: [15] tiers             Safe Havens: [5] [10] [15]     │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │  [10]       │  │  [11]       │  │  [12]       │  │  [13]       │    │
│  │  [11]       │  │  [12]       │  │  [13]       │  │  [14]       │    │
│  │  [12]       │  │  [13]       │  │  [14]       │  │  [15]       │    │
│  │  [13]       │  │  [14]       │  │  [15]       │  │  [RESET]    │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │
│                                                                         │
│  CRYPTOCURRENCY SELECTION                       DIFFICULTY             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │  ☑ BTC      │  │  ☑ ETH      │  │  ☑ SHILL    │  │  General    │    │
│  │  ☐ DOGE     │  │  ☐ SOL      │  │  ☐ ADA      │  │  Medium     │    │
│  │  ☐ MATIC    │  │  ☐ AVAX     │  │  ☐ DOT      │  │  [APPLY]    │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │
│                                                                         │
│  CUSTOM ANSWER REVEALS                                                  │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │  Tier: [15] ▼    Image: [Choose File]    Sound: [Choose File]      │ │
│  │  [PREVIEW] [UPLOAD] [CLEAR]                                         │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│  AUTO-SAVE & RECOVERY                                                   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │  Interval:  │  │  [30 sec] ▼  │  │  Last Saved: │  │  10:52:34    │    │
│  │  [ENABLED]  │  └─────────────┘  │  [LOAD LAST] │  └─────────────┘    │
│  └─────────────┘                 └─────────────┘                      │
│                                                                         │
│  OVERLAY THEME                             SOUND SETTINGS              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │  Dark       │  │  Background: │  │  [Choose File]   [PREVIEW]   │    │
│  │  [SELECTED] │  │  Correct:    │  │  [Choose File]   [PREVIEW]   │    │
│  │  Light      │  │  Wrong:      │  │  [Choose File]   [PREVIEW]   │    │
│  │  Crypto     │  │  Lifelines:  │  │  [Choose File]   [PREVIEW]   │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │
│                                                                         │
│  [APPLY CHANGES] [RESET TO DEFAULTS] [EXPORT SETTINGS] [IMPORT]        │
└─────────────────────────────────────────────────────────────────────────┘
```

## Visual Asset Requirements

### Static Assets
- **Background Elements**: Subtle geometric patterns with crypto motifs
- **Crypto Icons**: SVG icons for BTC, ETH, SHILL and other supported cryptocurrencies
- **UI Elements**: Buttons, sliders, checkboxes with crypto-futuristic styling
- **Progress Indicators**: Circular and linear progress bars with neon accents

### Dynamic Assets
- **Particle Effects**: Crypto sparkle effects for prize updates
- **Celebration Animations**: Confetti and fireworks for correct answers
- **Transition Effects**: Smooth slide animations between questions
- **Loading States**: Crypto mining-style progress indicators

### Custom Image Integration Points
- **Answer Reveal Images**: 800x600px images displayed during answer reveals
- **Host Avatar**: 200x200px circular image for host identification
- **Sponsor Logos**: 300x100px banner area for stream sponsors
- **Background Overlays**: Full 1920x1080px custom backgrounds

### Sound Integration Points
- **Background Music**: Continuous ambient music track
- **Answer Sounds**: Distinct audio cues for correct/wrong answers
- **Lifeline Audio**: Unique sounds for each lifeline activation
- **Custom Reveals**: User-uploaded sound effects for specific tiers

## Responsive Design Considerations

### Mobile/Tablet Overlay (Portrait)
```
┌─────────────────────────────────────────┐
│  HOST (200x150)    PUMP LADDER (VERT)   │
│  ┌─────────────┐  ┌───────────────────┐  │
│  │   HOST      │  │  TIER 15: 1.0 BTC │  │
│  │   VIDEO     │  │  TIER 14: 0.5 BTC │  │
│  └─────────────┘  │  ...               │  │
│                   └───────────────────┘  │
│  QUESTION (FULL WIDTH)                  │
│  [What is the capital of France?]       │
│  A) London   B) Berlin                  │
│  C) Paris   D) Madrid                   │
│  LIFELINES (HORIZONTAL SCROLL)          │
│  [50/50] [ASK] [PHONE]   [CURRENT: 0.18 BTC] │
└─────────────────────────────────────────┘
```

### Integration with Streaming Software

#### OBS Studio Setup
- **Scene Structure**: Main overlay as fullscreen scene
- **Source Layout**:
  - Background: Static image/animated wallpaper
  - Host Camera: 400x300 positioned top-left
  - Pump Ladder: 300x600 positioned top-right
  - Question Display: 1200x200 center area
  - Prize Display: 200x100 bottom-right
  - Lifeline Status: 400x100 bottom area

#### Browser Source Integration
- **Local Development**: `http://localhost:3000/overlay`
- **Production**: `https://your-domain.com/overlay?stream_key=abc123`
- **Update Mechanism**: WebSocket connection for real-time updates

## File Structure for Assets

```
assets/
├── images/
│   ├── background/
│   │   ├── space_default.jpg
│   │   ├── crypto_grid.png
│   │   └── neon_circuit.webp
│   ├── icons/
│   │   ├── btc.svg
│   │   ├── eth.svg
│   │   ├── shill.svg
│   │   └── lifeline_*.png
│   ├── ui/
│   │   ├── button_normal.png
│   │   ├── button_hover.png
│   │   ├── slider_track.png
│   │   └── checkbox_*.png
│   └── custom/
│       ├── reveals/
│       │   ├── tier_5_win.jpg
│       │   ├── tier_10_bonus.png
│       │   └── tier_15_victory.gif
│       └── sponsor_logos/
│           ├── sponsor1.png
│           └── sponsor2.png
├── sounds/
│   ├── music/
│   │   ├── ambient_theme.mp3
│   │   └── intense_mode.mp3
│   ├── effects/
│   │   ├── correct_answer.wav
│   │   ├── wrong_answer.wav
│   │   ├── lifeline_50_50.mp3
│   │   ├── lifeline_discord.mp3
│   │   ├── lifeline_phone.mp3
│   │   └── prize_update.wav
│   └── custom/
│       ├── tier_reveals/
│       │   ├── tier_15_applause.mp3
│       │   └── tier_10_bonus_sound.wav
│       └── user_uploads/
│           ├── custom_effect_001.wav
│           └── user_music_track.mp3
└── fonts/
    ├── Orbitron-Bold.ttf
    ├── RobotoCondensed-Regular.ttf
    └── DigitalNumbers.ttf
```

## Technical Implementation Notes

### Rendering Engine
- **Primary**: HTML5 Canvas with CSS animations
- **Fallback**: SVG graphics for maximum compatibility
- **Performance**: WebGL for complex particle effects

### Update Frequency
- **Real-time**: Cryptocurrency prices (30-second intervals)
- **Per Question**: Prize calculations and display updates
- **On Demand**: Custom image/sound loading for reveals

### Accessibility Features
- **High Contrast Mode**: Enhanced color contrast for visibility
- **Screen Reader Support**: ARIA labels for all interactive elements
- **Keyboard Navigation**: Full keyboard control for settings panel
- **Reduced Motion**: Option to disable animations for sensitive users

This visual design creates a professional, crypto-themed quiz show experience that maintains the excitement of the original "Who Wants to Be a Millionaire" format while incorporating modern streaming and cryptocurrency elements.
