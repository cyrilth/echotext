# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/cyrilth/echotext/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/cyrilth/echotext/releases/tag/v1.0.0
