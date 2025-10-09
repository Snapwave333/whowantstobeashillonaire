const { app, BrowserWindow, Menu, Tray, shell, ipcMain, dialog, nativeImage } = require('electron');
const log = require('electron-log');
const Store = require('electron-store');
const path = require('path');
const AppUpdater = require('./updater');
const isDev = process.env.NODE_ENV === 'development';

// Configure logging
log.transports.file.level = 'info';

// Initialize store for app settings
const store = new Store();

class ShillionaireApp {
  constructor() {
    this.mainWindow = null;
    this.tray = null;
    this.isQuitting = false;
    // this.updater = new AppUpdater(); // Disabled for standalone builds
    
    this.init();
  }

  init() {
    // Handle app ready
    app.whenReady().then(() => {
      this.createWindow();
      this.createTray();
      this.createMenu();
      
      // Set up auto updater with main window reference (disabled for standalone builds)
      // this.updater.setMainWindow(this.mainWindow);
      // this.updater.checkForUpdatesAndNotify();
      
      app.on('activate', () => {
        if (BrowserWindow.getAllWindows().length === 0) {
          this.createWindow();
        }
      });
    });

    // Handle window closed
    app.on('window-all-closed', () => {
      if (process.platform !== 'darwin') {
        app.quit();
      }
    });

    app.on('before-quit', () => {
      this.isQuitting = true;
    });
  }

  createWindow() {
    // Get screen dimensions
    const { screen } = require('electron');
    const primaryDisplay = screen.getPrimaryDisplay();
    const { width: screenWidth, height: screenHeight } = primaryDisplay.workAreaSize;
    const { x: workAreaX, y: workAreaY } = primaryDisplay.workArea;
    
    // Calculate window dimensions (fullscreen minus taskbar)
    const windowWidth = screenWidth;
    const windowHeight = screenHeight;

    // Create the main window (Contestant Game Board)
    this.mainWindow = new BrowserWindow({
      width: windowWidth,
      height: windowHeight,
      x: workAreaX,
      y: workAreaY,
      webPreferences: {
        nodeIntegration: false,
        contextIsolation: true,
        preload: path.join(__dirname, 'preload.js'),
        webSecurity: false
      },
      icon: path.join(__dirname, '../assets/icon.png'),
      show: false,
      titleBarStyle: 'default',
      frame: true,
      backgroundColor: '#1a1a2e',
      skipTaskbar: false,
      autoHideMenuBar: false,
      maximizable: true,
      resizable: true,
      fullscreenable: true
    });

    // Load the contestant game board
    const gameBoardPath = path.join(__dirname, '../contestant-game-board.html');
    console.log('Loading contestant game board from:', gameBoardPath);
    this.mainWindow.loadFile(gameBoardPath);
    
    // Show main window
    this.mainWindow.show();

    // Create Game Master Settings popup window
    this.createGameMasterWindow();
  }

  createGameMasterWindow() {
    // Get screen dimensions for Game Master window
    const { screen } = require('electron');
    const primaryDisplay = screen.getPrimaryDisplay();
    const { width: screenWidth, height: screenHeight } = primaryDisplay.workAreaSize;
    const { x: workAreaX, y: workAreaY } = primaryDisplay.workArea;
    
    // Calculate Game Master window dimensions (fullscreen minus taskbar)
    const gameMasterWidth = screenWidth;
    const gameMasterHeight = screenHeight;

    // Create the Game Master Settings popup window
    this.gameMasterWindow = new BrowserWindow({
      width: gameMasterWidth,
      height: gameMasterHeight,
      x: workAreaX,
      y: workAreaY,
      webPreferences: {
        nodeIntegration: false,
        contextIsolation: true,
        preload: path.join(__dirname, 'preload.js'),
        webSecurity: false
      },
      icon: path.join(__dirname, '../assets/icon.png'),
      show: true,
      titleBarStyle: 'default',
      frame: true,
      backgroundColor: '#1a1a2e',
      parent: this.mainWindow, // Make it a child window
      modal: false,
      skipTaskbar: true, // Don't show in taskbar
      autoHideMenuBar: true,
      maximizable: true,
      resizable: true
    });

    // Load the game master settings
    const gameMasterPath = path.join(__dirname, '../game-master-settings.html');
    console.log('Loading game master settings from:', gameMasterPath);
    this.gameMasterWindow.loadFile(gameMasterPath);

    // Handle Game Master window close
    this.gameMasterWindow.on('closed', () => {
      this.gameMasterWindow = null;
    });
  }

  loadMainApp() {
    const isDev = process.env.NODE_ENV === 'development';
    console.log('Environment:', isDev ? 'development' : 'production');
    console.log('__dirname:', __dirname);
    
    // Load the built React app
    const buildPath = path.join(__dirname, 'build/index.html');
    console.log('Looking for build at:', buildPath);
    console.log('Build exists:', require('fs').existsSync(buildPath));
    
    if (require('fs').existsSync(buildPath)) {
      console.log('Loading built React app from:', buildPath);
      this.mainWindow.loadFile(buildPath);
    } else {
      console.log('Build not found, falling back to renderer');
      const rendererPath = path.join(__dirname, 'renderer/index.html');
      console.log('Loading renderer from:', rendererPath);
      this.mainWindow.loadFile(rendererPath);
    }

    // Show window when ready (backup)
    this.mainWindow.once('ready-to-show', () => {
      this.mainWindow.show();
      this.mainWindow.focus();
      
      // Check for updates after window is shown
      if (!isDev) {
        // this.updater.checkForUpdates(); // Disabled for standalone builds
      }
      
      // Windows-specific: Flash taskbar on startup
      if (process.platform === 'win32') {
        this.mainWindow.flashFrame(true);
        setTimeout(() => {
          this.mainWindow.flashFrame(false);
        }, 2000);
      }
    });

    // Add error handling for failed loads
    this.mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription, validatedURL) => {
      console.error('Failed to load:', validatedURL, errorDescription);
      console.error('Error code:', errorCode);
      if (isDev && validatedURL.includes('localhost:3000')) {
        console.log('React dev server not available, falling back to local file');
        // Try to load built React app first
        const buildPath = path.join(__dirname, 'build/index.html');
        if (require('fs').existsSync(buildPath)) {
          this.mainWindow.loadFile(buildPath);
        } else {
          this.mainWindow.loadFile(path.join(__dirname, 'renderer/index.html'));
        }
      }
    });

    // Add success handling
    this.mainWindow.webContents.on('did-finish-load', () => {
      console.log('Page finished loading successfully');
    });

    // Add DOM ready handling
    this.mainWindow.webContents.on('dom-ready', () => {
      console.log('DOM is ready');
    });

    // Add console message handling for debugging
    this.mainWindow.webContents.on('console-message', (event, level, message) => {
      console.log('Renderer console:', message);
    });

    // Force show after a short delay if not already visible
    setTimeout(() => {
      if (this.mainWindow && !this.mainWindow.isVisible()) {
        this.mainWindow.show();
        this.mainWindow.focus();
      }
    }, 1000);

    // Handle window close
    this.mainWindow.on('close', (event) => {
      if (!this.isQuitting && process.platform === 'win32') {
        event.preventDefault();
        this.mainWindow.hide();
        this.showTrayNotification('App minimized to tray', 'Click the tray icon to restore the window');
        return false;
      }
    });

    // Windows-specific: Handle window state changes
    this.mainWindow.on('minimize', () => {
      if (process.platform === 'win32') {
        this.showTrayNotification('Minimized', 'Game is still running in the background');
      }
    });

    this.mainWindow.on('restore', () => {
      if (process.platform === 'win32') {
        this.mainWindow.flashFrame(true);
        setTimeout(() => {
          this.mainWindow.flashFrame(false);
        }, 1000);
      }
    });

    // Handle external links
    this.mainWindow.webContents.setWindowOpenHandler(({ url }) => {
      shell.openExternal(url);
      return { action: 'deny' };
    });
  }

  createTray() {
    const trayIcon = nativeImage.createFromPath(path.join(__dirname, '../assets/tray-icon.png'));
    this.tray = new Tray(trayIcon.resize({ width: 16, height: 16 }));
    
    const contextMenu = Menu.buildFromTemplate([
      {
        label: 'Show Shillionaire',
        click: () => {
          this.mainWindow.show();
          this.mainWindow.focus();
        }
      },
      {
        label: 'New Game',
        click: () => {
          this.mainWindow.show();
          this.mainWindow.focus();
          this.mainWindow.webContents.send('new-game');
        }
      },
      { type: 'separator' },
      {
        label: 'Check for Updates',
        click: () => {
          // this.updater.forceCheckForUpdates(); // Disabled for standalone builds
        }
      },
      { type: 'separator' },
      {
        label: 'Quit',
        click: () => {
          this.isQuitting = true;
          app.quit();
        }
      }
    ]);

    this.tray.setToolTip('Who Wants to Be a Shillionaire');
    this.tray.setContextMenu(contextMenu);
    
    this.tray.on('double-click', () => {
      this.mainWindow.show();
      this.mainWindow.focus();
    });
  }

  createMenu() {
    const template = [
      {
        label: 'Game',
        submenu: [
          {
            label: 'New Game',
            accelerator: 'CmdOrCtrl+N',
            click: () => {
              this.mainWindow.webContents.send('new-game');
            }
          },
          {
            label: 'Restart Game',
            accelerator: 'CmdOrCtrl+R',
            click: () => {
              this.mainWindow.webContents.send('restart-game');
            }
          },
          { type: 'separator' },
          {
            label: 'High Scores',
            accelerator: 'CmdOrCtrl+H',
            click: () => {
              this.mainWindow.webContents.send('show-scores');
            }
          },
          { type: 'separator' },
          {
            label: 'Quit',
            accelerator: process.platform === 'darwin' ? 'Cmd+Q' : 'Ctrl+Q',
            click: () => {
              this.isQuitting = true;
              app.quit();
            }
          }
        ]
      },
      {
        label: 'Settings',
        submenu: [
          {
            label: 'Sound Effects',
            type: 'checkbox',
            checked: store.get('soundEnabled', true),
            click: (menuItem) => {
              store.set('soundEnabled', menuItem.checked);
              this.mainWindow.webContents.send('sound-toggle', menuItem.checked);
            }
          },
          {
            label: 'Background Music',
            type: 'checkbox',
            checked: store.get('musicEnabled', true),
            click: (menuItem) => {
              store.set('musicEnabled', menuItem.checked);
              this.mainWindow.webContents.send('music-toggle', menuItem.checked);
            }
          },
          { type: 'separator' },
          {
            label: 'Difficulty',
            submenu: [
              {
                label: 'Easy',
                type: 'radio',
                checked: store.get('difficulty', 'normal') === 'easy',
                click: () => {
                  store.set('difficulty', 'easy');
                  this.mainWindow.webContents.send('difficulty-change', 'easy');
                }
              },
              {
                label: 'Normal',
                type: 'radio',
                checked: store.get('difficulty', 'normal') === 'normal',
                click: () => {
                  store.set('difficulty', 'normal');
                  this.mainWindow.webContents.send('difficulty-change', 'normal');
                }
              },
              {
                label: 'Hard',
                type: 'radio',
                checked: store.get('difficulty', 'normal') === 'hard',
                click: () => {
                  store.set('difficulty', 'hard');
                  this.mainWindow.webContents.send('difficulty-change', 'hard');
                }
              }
            ]
          }
        ]
      },
      {
        label: 'View',
        submenu: [
          { role: 'reload' },
          { role: 'forceReload' },
          { role: 'toggleDevTools' },
          { type: 'separator' },
          { role: 'resetZoom' },
          { role: 'zoomIn' },
          { role: 'zoomOut' },
          { type: 'separator' },
          { role: 'togglefullscreen' },
          { type: 'separator' },
          {
            label: 'Always on Top',
            type: 'checkbox',
            checked: this.mainWindow ? this.mainWindow.isAlwaysOnTop() : false,
            click: (menuItem) => {
              if (this.mainWindow) {
                this.mainWindow.setAlwaysOnTop(menuItem.checked);
              }
            }
          }
        ]
      },
      {
        label: 'Window',
        submenu: [
          { role: 'minimize' },
          { role: 'close' },
          { type: 'separator' },
          {
            label: 'Hide to Tray',
            accelerator: 'CmdOrCtrl+Shift+H',
            click: () => {
              this.mainWindow.hide();
              this.showTrayNotification('Hidden to Tray', 'Double-click tray icon to restore');
            }
          }
        ]
      },
      {
        label: 'Help',
        submenu: [
          {
            label: 'About',
            click: () => {
              dialog.showMessageBox(this.mainWindow, {
                type: 'info',
                title: 'About',
                message: 'Who Wants to Be a Shillionaire',
                detail: `Version: ${app.getVersion()}\nA trivia game with cryptocurrency prizes!`
              });
            }
          },
          {
            label: 'Check for Updates',
            click: () => {
              // this.updater.forceCheckForUpdates(); // Disabled for standalone builds
            }
          }
        ]
      }
    ];

    // Windows-specific menu adjustments
    if (process.platform === 'win32') {
      // Add Windows-specific menu items
      template[3].submenu.push(
        { type: 'separator' },
        {
          label: 'Show in Taskbar',
          type: 'checkbox',
          checked: true, // Default to showing in taskbar
          click: (menuItem) => {
            if (this.mainWindow) {
              this.mainWindow.setSkipTaskbar(!menuItem.checked);
            }
          }
        }
      );
    }

    const menu = Menu.buildFromTemplate(template);
    Menu.setApplicationMenu(menu);
  }

  showTrayNotification(title, body = '') {
    if (this.tray && process.platform === 'win32') {
      this.tray.displayBalloon({
        iconType: 'info',
        title: title,
        content: body || title,
        respectQuietTime: true
      });
    }
  }
}

// IPC handlers
ipcMain.handle('app-version', () => {
  return app.getVersion();
});

ipcMain.handle('check-for-updates', () => {
  // This will be handled by the updater instance in the app class
  // For now, we'll just return a promise
  return Promise.resolve();
});

ipcMain.handle('get-store-value', (event, key) => {
  return store.get(key);
});

ipcMain.handle('set-store-value', (event, key, value) => {
  store.set(key, value);
});

// IPC handler for opening game master settings window
ipcMain.handle('open-game-master-settings', () => {
  if (global.shillionaireApp && global.shillionaireApp.gameMasterWindow) {
    // If window exists, focus it
    global.shillionaireApp.gameMasterWindow.focus();
  } else {
    // If window doesn't exist, create it
    global.shillionaireApp.createGameMasterWindow();
  }
});

// Initialize the app
global.shillionaireApp = new ShillionaireApp();