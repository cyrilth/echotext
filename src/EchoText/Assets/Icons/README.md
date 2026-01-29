# Tray Icon Assets

This directory contains the tray icons for EchoText's system tray integration.

## Current State

The current `.ico` files are placeholders copied from the Avalonia logo. They should be replaced with proper icons that visually represent each state:

- **tray-idle.ico** - Default icon when the app is ready and idle
- **tray-recording.ico** - Icon shown during audio recording (should be red or indicate recording)
- **tray-processing.ico** - Icon shown during transcription processing (should show activity/spinner)
- **tray-error.ico** - Icon shown when an error occurs (should indicate error state)

## Icon Requirements

- Format: .ico (Windows icon format)
- Recommended sizes: 16x16, 32x32, 48x48 (for different DPI settings)
- Style: Should match the application's design language
- Colors:
  - Idle: Neutral/blue
  - Recording: Red/active
  - Processing: Yellow/orange
  - Error: Red/warning

## Replacing Icons

To replace these placeholder icons:

1. Create new .ico files with the appropriate visual design
2. Save them with the same filenames in this directory
3. The application will automatically load the new icons on next run

## Notes

The icons are loaded as Avalonia assets and embedded in the application at build time.
