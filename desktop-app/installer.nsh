; Custom NSIS installer script for Shillionaire
!macro customInstall
  ; Create registry entries for uninstaller
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTALL_APP_KEY}" "DisplayName" "${PRODUCT_NAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTALL_APP_KEY}" "DisplayVersion" "${VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTALL_APP_KEY}" "Publisher" "Snapwave"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTALL_APP_KEY}" "DisplayIcon" "$INSTDIR\${PRODUCT_FILENAME}.exe"
  
  ; Associate with game files (optional)
  WriteRegStr HKCR ".shillionaire" "" "ShillionaireGame"
  WriteRegStr HKCR "ShillionaireGame" "" "Shillionaire Game File"
  WriteRegStr HKCR "ShillionaireGame\DefaultIcon" "" "$INSTDIR\${PRODUCT_FILENAME}.exe,0"
  WriteRegStr HKCR "ShillionaireGame\shell\open\command" "" '"$INSTDIR\${PRODUCT_FILENAME}.exe" "%1"'
!macroend

!macro customUnInstall
  ; Remove registry entries
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTALL_APP_KEY}"
  DeleteRegKey HKCR ".shillionaire"
  DeleteRegKey HKCR "ShillionaireGame"
!macroend