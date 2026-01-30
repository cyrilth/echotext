# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.5] - 2026-01-30

### Fixed
- Fix JSON serialization error when checking for updates in trimmed builds

## [1.0.4] - 2026-01-30

### Fixed
- Prevent multiple instances of the application from running simultaneously

## [1.0.3] - 2026-01-30

### Added
- About dialog showing application version, MIT license, and information
- Toast notifications using Avalonia's notification system (bottom-right corner)
- Update check dialog showing current version, latest version, and download link

### Fixed
- Assembly version now matches git tag during release builds

## [1.0.2] - 2026-01-30

### Fixed
- Update checker now uses correct GitHub repository URL

## [1.0.1] - 2026-01-30

### Added
- Start with System option to automatically launch EchoText on login
  - Windows: Registry-based startup (HKCU\Software\Microsoft\Windows\CurrentVersion\Run)
  - Linux: XDG Autostart desktop entry (~/.config/autostart/)
  - macOS: LaunchAgent plist file (~/Library/LaunchAgents/)

### Fixed
- Threading issues causing UI crashes when showing/hiding recording overlay
- Hotkey detection not recognizing modifier keys correctly
- Download button greyed out in Settings when selecting a model
- Unable to change keyboard shortcut in Settings window
- Selected model not loading after changing in Settings (required restart)

### Changed
- Improved CI workflow with proper permissions for test reporting

## [1.0.0] - 2026-01-29

### Added
- Global hotkey support for starting/stopping voice recording
- Real-time speech-to-text transcription using OpenAI Whisper
- Multiple Whisper model options (tiny, base, small, medium, large)
- Automatic text output to clipboard
- Optional auto-type functionality to paste text into active application
- Cross-platform support for Windows, Linux, and macOS
- System tray integration with context menu
- Recording overlay with audio level visualization
- Configurable settings window
  - Audio input device selection
  - Hotkey customization
  - Push-to-Talk and Toggle recording modes
  - Language selection (auto-detect or specific language)
  - Model selection
  - Output preferences
- Sound effects and notifications for recording events
- First-run experience with automatic model download
- Update checker for new releases
- Comprehensive logging for troubleshooting
- Fully offline operation (no internet required after setup)

### Security
- All processing happens locally on your device
- No data is sent to external servers
- Privacy-first architecture

[Unreleased]: https://github.com/cyrilth/echotext/compare/v1.0.5...HEAD
[1.0.5]: https://github.com/cyrilth/echotext/compare/v1.0.4...v1.0.5
[1.0.4]: https://github.com/cyrilth/echotext/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/cyrilth/echotext/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/cyrilth/echotext/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/cyrilth/echotext/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/cyrilth/echotext/releases/tag/v1.0.0
