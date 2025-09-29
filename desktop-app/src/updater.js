const { autoUpdater } = require('electron-updater');
const { dialog, BrowserWindow } = require('electron');
const log = require('electron-log');

class AppUpdater {
  constructor() {
    this.mainWindow = null;
    this.updateCheckInProgress = false;
    this.setupUpdater();
  }

  setMainWindow(window) {
    this.mainWindow = window;
  }

  setupUpdater() {
    // Configure auto updater
    autoUpdater.logger = log;
    autoUpdater.logger.transports.file.level = 'info';
    
    // Set update server (GitHub releases)
    autoUpdater.setFeedURL({
      provider: 'github',
      owner: 'Snapwave333',
      repo: 'whowantstobeashillonaire',
      private: false
    });

    // Auto updater events
    autoUpdater.on('checking-for-update', () => {
      log.info('Checking for update...');
      this.updateCheckInProgress = true;
    });

    autoUpdater.on('update-available', (info) => {
      log.info('Update available:', info);
      this.updateCheckInProgress = false;
      this.showUpdateAvailableDialog(info);
    });

    autoUpdater.on('update-not-available', (info) => {
      log.info('Update not available:', info);
      this.updateCheckInProgress = false;
    });

    autoUpdater.on('error', (err) => {
      log.error('Error in auto-updater:', err);
      this.updateCheckInProgress = false;
      this.showUpdateErrorDialog(err);
    });

    autoUpdater.on('download-progress', (progressObj) => {
      const logMessage = `Download speed: ${progressObj.bytesPerSecond} - Downloaded ${progressObj.percent}% (${progressObj.transferred}/${progressObj.total})`;
      log.info(logMessage);
      
      // Update progress in UI if window exists
      if (this.mainWindow && this.mainWindow.webContents) {
        this.mainWindow.webContents.send('download-progress', {
          percent: Math.round(progressObj.percent),
          transferred: progressObj.transferred,
          total: progressObj.total
        });
      }
    });

    autoUpdater.on('update-downloaded', (info) => {
      log.info('Update downloaded:', info);
      this.showUpdateReadyDialog(info);
    });
  }

  checkForUpdates() {
    if (this.updateCheckInProgress) {
      log.info('Update check already in progress');
      return;
    }

    log.info('Manually checking for updates...');
    autoUpdater.checkForUpdatesAndNotify().catch(err => {
      log.error('Manual update check failed:', err);
      this.showUpdateErrorDialog(err);
    });
  }

  checkForUpdatesAndNotify() {
    if (process.env.NODE_ENV === 'development') {
      log.info('Skipping auto-update check in development mode');
      return;
    }

    setTimeout(() => {
      autoUpdater.checkForUpdatesAndNotify().catch(err => {
        log.error('Auto update check failed:', err);
      });
    }, 3000); // Wait 3 seconds after app start
  }

  showUpdateAvailableDialog(info) {
    if (!this.mainWindow) return;

    const options = {
      type: 'info',
      title: 'Update Available',
      message: `A new version (${info.version}) is available!`,
      detail: 'The update will be downloaded in the background. You will be notified when it is ready to install.',
      buttons: ['OK', 'View Release Notes'],
      defaultId: 0
    };

    dialog.showMessageBox(this.mainWindow, options).then(result => {
      if (result.response === 1) {
        // Open release notes in default browser
        require('electron').shell.openExternal(`https://github.com/Snapwave333/whowantstobeashillonaire/releases/tag/v${info.version}`);
      }
    });
  }

  showUpdateReadyDialog(info) {
    if (!this.mainWindow) return;

    const options = {
      type: 'info',
      title: 'Update Ready',
      message: 'Update downloaded successfully!',
      detail: `Version ${info.version} has been downloaded and is ready to install. The application will restart to apply the update.`,
      buttons: ['Restart Now', 'Restart Later'],
      defaultId: 0
    };

    dialog.showMessageBox(this.mainWindow, options).then(result => {
      if (result.response === 0) {
        // Restart and install update
        setImmediate(() => autoUpdater.quitAndInstall());
      }
    });
  }

  showUpdateErrorDialog(error) {
    if (!this.mainWindow) return;

    const options = {
      type: 'error',
      title: 'Update Error',
      message: 'Failed to check for updates',
      detail: `An error occurred while checking for updates: ${error.message}`,
      buttons: ['OK', 'Retry'],
      defaultId: 0
    };

    dialog.showMessageBox(this.mainWindow, options).then(result => {
      if (result.response === 1) {
        // Retry update check
        setTimeout(() => this.checkForUpdates(), 1000);
      }
    });
  }

  // Force check for updates (called from menu)
  forceCheckForUpdates() {
    if (process.env.NODE_ENV === 'development') {
      dialog.showMessageBox(this.mainWindow, {
        type: 'info',
        title: 'Development Mode',
        message: 'Auto-updates are disabled in development mode',
        detail: 'Updates are only available in the production build of the application.'
      });
      return;
    }

    this.checkForUpdates();
  }
}

module.exports = AppUpdater;