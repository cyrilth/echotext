# EchoText

**Your voice echoes back as text.**

A privacy-focused, cross-platform speech-to-text application that runs completely offline using OpenAI's Whisper model.

![EchoText Demo](docs/images/demo.gif)

[![Build Status](https://github.com/cyrilth/echotext/workflows/Build/badge.svg)](https://github.com/cyrilth/echo-text/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Latest Release](https://img.shields.io/github/v/release/cyrilth/echotext)](https://github.com/cyrilth/echotext/releases)

---

## Features

- **100% Offline** - Your voice never leaves your computer. Complete privacy.
- **Cross-Platform** - Works on Windows, Linux (Ubuntu), and macOS
- **Global Hotkey** - Press a hotkey anywhere to start recording
- **Multiple Modes** - Push-to-talk (hold) or toggle (press once to start/stop)
- **Flexible Output** - Copy to clipboard or auto-type into any application
- **Multiple Model Sizes** - Choose from tiny to large Whisper models for speed vs. accuracy
- **System Tray Integration** - Runs quietly in the background
- **Audio Feedback** - Optional sound effects for recording start/stop
- **No Account Required** - Just download and run
- **Open Source** - Inspect the code, contribute, or build it yourself

---

## Screenshots

### System Tray Integration
![Tray Icon](docs/images/screenshot-tray.png)

### Settings Window
![Settings](docs/images/screenshot-settings.png)

### Recording Overlay
![Recording](docs/images/screenshot-recording.png)

---

## Platform Support

| Platform | Status |
|----------|--------|
| Windows 11 | ✅ Tested |
| Windows 10 | ⚠️ Untested (binaries provided) |
| Linux (Ubuntu) | ⚠️ Untested (binaries provided) |
| macOS (Intel) | ⚠️ Untested (binaries provided) |
| macOS (Apple Silicon) | ⚠️ Untested (binaries provided) |

> **Note:** This application has only been tested on Windows 11. Binaries are provided for other platforms but are not guaranteed to work. Community feedback and contributions for other platforms are welcome!

---

## Installation

### Windows

1. Download `EchoText-{version}-win-x64.zip` from [Releases](https://github.com/cyrilth/echotext/releases)
2. Extract to a folder (e.g., `C:\Program Files\EchoText`)
3. Run `EchoText.exe`
4. If "Windows protected your PC" appears:
   - Click **More info**
   - Click **Run anyway**
5. The app will appear in your system tray
6. On first run, it will download the default speech recognition model

**Optional:** Right-click `EchoText.exe` > **Create shortcut** and move to your Startup folder for auto-start.

### Linux (Ubuntu 20.04+)

```bash
# Download and extract
wget https://github.com/cyrilth/echotext/releases/latest/download/EchoText-linux-x64.tar.gz
tar -xzf EchoText-linux-x64.tar.gz

# Run it
./EchoText
```

**System Tray Support (GNOME):**
If you're using GNOME, install the AppIndicator extension:
```bash
sudo apt install gnome-shell-extension-appindicator
```

**Note:** EchoText works best with X11. Wayland support is limited (global hotkeys may not work).

### macOS (11 Big Sur or later)

1. Download the appropriate tarball for your Mac:
   - **Intel Macs:** `EchoText-{version}-osx-x64.tar.gz`
   - **Apple Silicon (M1/M2/M3):** `EchoText-{version}-osx-arm64.tar.gz`
2. Extract the tarball:
   ```bash
   tar -xzf EchoText-*-osx-*.tar.gz
   ```
3. Move to Applications (optional):
   ```bash
   mv EchoText /Applications/
   ```
4. **First launch only:** Right-click the app > **Open** > Click **Open** in the dialog
   - This is required because the app is not notarized
5. Grant **Accessibility** permission when prompted:
   - System Preferences > Privacy & Security > Accessibility
   - Enable **EchoText**
6. Grant **Microphone** permission when prompted on first recording

**Not sure which version?**
- Open Apple menu > **About This Mac**
- **Chip: Apple M1/M2/M3** = Use ARM64 version
- **Processor: Intel** = Use x64 version

---

## Usage

### Quick Start

1. **Start Recording:** Press the global hotkey (default: `Ctrl+Shift+Space`)
2. **Speak:** Say what you want to transcribe
3. **Stop Recording:** Release the hotkey (push-to-talk mode) or press it again (toggle mode)
4. **Get Text:** The transcribed text is copied to your clipboard automatically

### Basic Workflow

```
Position cursor in any text field
     ↓
Press and hold Ctrl+Shift+Space
     ↓
Speak clearly
     ↓
Release the hotkey
     ↓
Wait 2-5 seconds (processing)
     ↓
Paste with Ctrl+V
```

### Tips for Best Results

- **Speak clearly** and at a normal pace
- **Use a good microphone** - headset mics work better than built-in laptop mics
- **Minimize background noise**
- **Shorter recordings** (under 30 seconds) process faster
- **Choose the right model:**
  - **Tiny:** Fastest, good for quick notes
  - **Base:** Balanced speed and accuracy (default)
  - **Small:** Better accuracy, slower
  - **Medium/Large:** Best accuracy, requires powerful CPU

---

## Configuration

Right-click the system tray icon and select **Settings** to customize:

### Audio Settings
- **Input Device:** Choose your microphone from the dropdown
- **Test Microphone:** Click to test audio levels

### Hotkey Settings
- **Mode:**
  - **Push-to-Talk:** Hold the hotkey while speaking, release to stop
  - **Toggle:** Press once to start, press again to stop
- **Key Combination:** Click to set a custom hotkey (e.g., `Ctrl+Alt+V`)

### Output Settings
- **Copy to Clipboard:** Automatically copy transcribed text (recommended)
- **Auto-Type:** Type the text into the active window automatically
- **Play Sounds:** Audio feedback when recording starts/stops

### Recognition Settings
- **Model:** Choose Whisper model size

  | Model  | Size   | Speed      | Accuracy |
  |--------|--------|------------|----------|
  | Tiny   | 75 MB  | Very Fast  | Good     |
  | Base   | 142 MB | Fast       | Better   |
  | Small  | 466 MB | Medium     | Great    |
  | Medium | 1.5 GB | Slow       | Excellent|
  | Large  | 3 GB   | Very Slow  | Best     |

- **Language:** Select your language or use **Auto-detect**

### General Settings
- **Start with System:** Launch EchoText when you log in
- **Show Notifications:** Display toast notifications
- **Check for Updates:** Automatically check GitHub for new releases

---

## Building from Source

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

### Clone and Build

```bash
# Clone the repository
git clone https://github.com/cyrilth/echotext.git
cd echotext

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/EchoText
```

### Build Release Binaries

**Windows:**
```bash
dotnet publish src/EchoText -c Release -r win-x64 --self-contained
```

**Linux:**
```bash
dotnet publish src/EchoText -c Release -r linux-x64 --self-contained
```

**macOS (Intel):**
```bash
dotnet publish src/EchoText -c Release -r osx-x64 --self-contained
```

**macOS (Apple Silicon):**
```bash
dotnet publish src/EchoText -c Release -r osx-arm64 --self-contained
```

Binaries will be in `src/EchoText/bin/Release/net8.0/{runtime}/publish/`

---

## Frequently Asked Questions

### Does this require an internet connection?

No! After the initial model download, EchoText works completely offline. Your voice data never leaves your computer.

### How accurate is the transcription?

Accuracy depends on the model size and audio quality. With the **base** model and a good microphone, expect 90%+ accuracy for clear English speech. Larger models provide better accuracy but take longer to process.

### What languages are supported?

Whisper supports 99 languages including English, Spanish, French, German, Chinese, Japanese, and more. Select your language in Settings or use Auto-detect.

### Why does Windows/macOS warn me about the app?

The app is not code-signed (requires paid developer accounts). The app is open source and safe - you can review the code yourself. This is a one-time warning that can be bypassed.

### Can I use this with specific applications?

Yes! EchoText works with any application. It operates at the system level:
- **Clipboard mode:** Works everywhere that supports paste (Ctrl+V)
- **Auto-type mode:** Types directly into the active window

### How much CPU does it use?

- **Idle:** Less than 1%
- **Recording:** Less than 5%
- **Transcribing:** 50-100% of available cores (brief burst, then back to idle)

### Where are models stored?

| Platform | Location |
|----------|----------|
| Windows  | `%APPDATA%\EchoText\models\` |
| Linux    | `~/.local/share/echotext/models/` |
| macOS    | `~/Library/Application Support/EchoText/models/` |

### Can I delete models to save space?

Yes! Open Settings and select a different model. You can manually delete unused model files from the location above.

### Does it work on Wayland (Linux)?

Partially. Clipboard functionality works, but global hotkeys have limitations on Wayland. We recommend using an X11 session for full functionality.

### How do I enable auto-start?

- **Windows:** Copy a shortcut to `shell:startup`
- **Linux:** Add to your desktop environment's startup applications
- **macOS:** System Preferences > Users & Groups > Login Items > Add EchoText

---

## Troubleshooting

### Windows: "Windows protected your PC" warning

This is normal for unsigned apps. Click **More info** > **Run anyway**. This only appears once.

### macOS: "Cannot be opened because the developer cannot be verified"

1. Right-click the app > **Open**
2. Click **Open** in the dialog
3. This only needs to be done once

### macOS: App doesn't respond to hotkey

1. Go to **System Preferences** > **Privacy & Security** > **Accessibility**
2. Ensure **EchoText** is in the list and enabled
3. You may need to restart the app after granting permission

### Linux: No system tray icon (GNOME)

Install the AppIndicator extension:
```bash
sudo apt install gnome-shell-extension-appindicator
```
Then restart GNOME Shell (Alt+F2, type `r`, press Enter).

### Transcription is slow

- Use a smaller model (tiny or base)
- Ensure other CPU-intensive apps aren't running
- Shorter recordings process faster

### Poor transcription quality

- Use a better microphone (headset recommended)
- Speak clearly and at a normal pace
- Reduce background noise
- Try a larger model (small, medium, or large)
- Select the correct language in Settings

### Model download fails

- Check your internet connection
- The download is large (75 MB to 3 GB depending on model)
- Try downloading again from Settings

---

## Contributing

We welcome contributions! Here's how you can help:

1. **Report Bugs:** Open an [issue](https://github.com/cyrilth/echotext/issues) with details
2. **Suggest Features:** Share your ideas in [discussions](https://github.com/cyrilth/echotext/discussions)
3. **Submit Pull Requests:**
   - Fork the repository
   - Create a feature branch (`git checkout -b feature/amazing-feature`)
   - Commit your changes (`git commit -m 'Add amazing feature'`)
   - Push to the branch (`git push origin feature/amazing-feature`)
   - Open a Pull Request

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

---

## Project Structure

```
echotext/
├── src/
│   ├── EchoText/              # Main application
│   │   ├── Models/            # Data models
│   │   ├── Services/          # Business logic
│   │   ├── Platform/          # OS-specific code
│   │   ├── ViewModels/        # MVVM ViewModels
│   │   └── Views/             # Avalonia XAML views
│   └── EchoText.Tests/        # Unit tests
├── docs/                      # Documentation
│   ├── REQUIREMENTS.md
│   ├── ARCHITECTURE.md
│   └── TASKS.md
├── .github/workflows/         # CI/CD pipelines
├── README.md                  # This file
└── LICENSE                    # MIT License
```

---

## Technology Stack

- **Language:** C# / .NET 8
- **UI Framework:** [Avalonia UI 11](https://avaloniaui.net/)
- **Speech Recognition:** [Whisper.net](https://github.com/sandrohanea/whisper.net) (OpenAI Whisper)
- **Audio Capture:** [NAudio](https://github.com/naudio/NAudio)
- **Global Hotkeys:** [SharpHook](https://github.com/TolikPylypchuk/SharpHook)
- **Architecture:** MVVM with Dependency Injection

---

## Privacy & Security

- **No telemetry** - Zero data collection
- **No analytics** - We don't track usage
- **No cloud services** - Everything runs locally
- **No account required** - Just download and use
- **Open source** - Audit the code yourself

Your privacy is our priority. Your voice recordings are processed locally and never stored or transmitted.

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- **OpenAI** for the [Whisper](https://github.com/openai/whisper) speech recognition model
- **whisper.cpp** team for the efficient C++ implementation
- **Sandro Hanea** for [Whisper.net](https://github.com/sandrohanea/whisper.net) bindings
- **Avalonia UI** team for the cross-platform UI framework
- All contributors who help improve EchoText

---

## Support

- **Documentation:** [docs/](docs/)
- **Issues:** [GitHub Issues](https://github.com/cyrilth/echotext/issues)
- **Discussions:** [GitHub Discussions](https://github.com/cyrilth/echotext/discussions)

---

## Roadmap

### v1.0 (Current)
- ✅ Core transcription functionality
- ✅ Windows, Linux, and macOS support
- ✅ Multiple Whisper models
- ✅ System tray integration
- ✅ Auto-type and clipboard output

### v1.1 (Planned)
- ⬜ Voice commands ("new line", "period", "delete that")
- ⬜ Transcription history
- ⬜ Custom vocabulary
- ⬜ Punctuation commands

### v2.0 (Future)
- ⬜ Real-time streaming transcription
- ⬜ Multiple language detection in same session
- ⬜ Text formatting commands
- ⬜ Plugin system for extensibility

---

**Made with ❤️ for privacy-conscious users everywhere.**

**Star this repo if you find it useful!** ⭐
