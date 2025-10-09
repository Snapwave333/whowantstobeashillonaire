const { contextBridge, ipcRenderer } = require('electron');

// Expose protected methods that allow the renderer process to use
// the ipcRenderer without exposing the entire object
contextBridge.exposeInMainWorld('electronAPI', {
  // App info
  getAppVersion: () => ipcRenderer.invoke('app-version'),
  
  // Auto updater
  checkForUpdates: () => ipcRenderer.invoke('check-for-updates'),
  
  // Store (for settings/preferences)
  getStoreValue: (key) => ipcRenderer.invoke('get-store-value', key),
  setStoreValue: (key, value) => ipcRenderer.invoke('set-store-value', key, value),
  
  // Game Master Settings
  openGameMasterSettings: () => ipcRenderer.invoke('open-game-master-settings'),
  
  // Game events
  onNewGame: (callback) => ipcRenderer.on('new-game', callback),
  onRestartGame: (callback) => ipcRenderer.on('restart-game', callback),
  
  // Remove listeners
  removeAllListeners: (channel) => ipcRenderer.removeAllListeners(channel),
  
  // Platform info
  platform: process.platform,
  isElectron: true
});