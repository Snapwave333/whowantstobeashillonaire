# Getting Started

Follow these steps to set up the project locally.

## Prerequisites
- Node.js v18+
- Python 3.9+
- Git

## Setup
```bash
# Clone
git clone https://github.com/Snapwave333/whowantstobeashillonaire.git
cd whowantstobeashillonaire

# Backend
cd backend
pip install -r requirements.txt
python app.py

# Frontend (new terminal)
cd ../frontend
npm ci
npm start

# Desktop (new terminal)
cd ../desktop-app
npm ci
npm run dev
```

## Build
```bash
# Desktop installer
cd desktop-app
npm run build:installer

# Web build
cd ../frontend
npm run build
```

## Releases
Download the latest `.exe` or `.zip` from the Releases page.
