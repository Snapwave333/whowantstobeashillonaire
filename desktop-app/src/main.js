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
    this.updater = new AppUpdater();
    
    this.init();
  }

  init() {
    // Handle app ready
    app.whenReady().then(() => {
      this.createWindow();
      this.createTray();
      this.createMenu();
      
      // Set up auto updater with main window reference
      this.updater.setMainWindow(this.mainWindow);
      this.updater.checkForUpdatesAndNotify();
      
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
    // Create the browser window
    this.mainWindow = new BrowserWindow({
      width: 1200,
      height: 800,
      minWidth: 800,
      minHeight: 600,
      webPreferences: {
        nodeIntegration: false,
        contextIsolation: true,
        preload: path.join(__dirname, 'preload.js')
      },
      icon: path.join(__dirname, '../assets/icon.png'),
      show: false, // Don't show until ready
      titleBarStyle: 'default',
      frame: true,
      backgroundColor: '#1a1a2e',
      // Windows-specific enhancements
      skipTaskbar: false,
      autoHideMenuBar: false,
      maximizable: true,
      resizable: true,
      fullscreenable: true
    });

    // Load the app
    this.mainWindow.loadFile(path.join(__dirname, '../build/index.html'));

    // Show window when ready
    this.mainWindow.once('ready-to-show', () => {
      this.mainWindow.show();
      
      // Check for updates after window is shown
      if (!isDev) {
        this.updater.checkForUpdates();
      }
      
      // Windows-specific: Flash taskbar on startup
      if (process.platform === 'win32') {
        this.mainWindow.flashFrame(true);
        setTimeout(() => {
          this.mainWindow.flashFrame(false);
        }, 2000);
      }
    });

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
          this.updater.forceCheckForUpdates();
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
              this.updater.forceCheckForUpdates();
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

// Initialize the app
new ShillionaireApp();