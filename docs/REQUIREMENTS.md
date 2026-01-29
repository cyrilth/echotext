# EchoText - Speech-to-Text Application
## Requirements Document

**Version:** 1.0 (Draft)  
**Date:** January 28, 2025  
**Status:** Under Review

---

## 1. Project Overview

### 1.1 Purpose
Build a local, privacy-focused speech-to-text application for Windows, Linux, and macOS that allows users to dictate text using their voice. EchoText runs entirely offline—your voice echoes back as text, instantly.

### 1.2 Goals
- Provide fast and accurate voice-to-text transcription
- Run completely offline (no cloud dependencies)
- Minimal system resource usage when idle
- Simple, non-intrusive user interface
- **Cross-platform support (Windows, Ubuntu Linux, and macOS)**
- Seamless integration with any desktop application

### 1.3 Target Users
- Professionals who need hands-free text input
- Users with accessibility needs
- Anyone wanting to speed up text entry
- Privacy-conscious users who prefer local processing

---

## 2. Functional Requirements

### 2.1 Core Features

| ID | Feature | Description | Priority |
|----|---------|-------------|----------|
| FR-01 | Push-to-Talk | Hold a hotkey to record, release to transcribe | Must Have |
| FR-02 | Toggle Recording | Press hotkey to start/stop recording | Should Have |
| FR-03 | Clipboard Output | Automatically copy transcribed text to clipboard | Must Have |
| FR-04 | Auto-Type Output | Simulate keyboard typing into active window | Should Have |
| FR-05 | System Tray Icon | Minimize to system tray, show status | Must Have |
| FR-06 | Audio Feedback | Play sound on recording start/stop | Should Have |
| FR-07 | Visual Indicator | Show recording status (overlay or tray icon change) | Must Have |

### 2.2 Configuration Options

| ID | Feature | Description | Priority |
|----|---------|-------------|----------|
| FR-08 | Hotkey Configuration | Allow user to set custom hotkey | Must Have |
| FR-09 | Model Selection | Choose Whisper model size (tiny/base/small/medium/large) | Should Have |
| FR-10 | Language Selection | Set transcription language or use auto-detect | Should Have |
| FR-11 | Output Mode Selection | Choose between clipboard, auto-type, or both | Must Have |
| FR-12 | Startup with Windows | Option to launch on system startup | Could Have |
| FR-13 | Audio Device Selection | Choose input microphone | Should Have |

### 2.3 Advanced Features (Future)

| ID | Feature | Description | Priority |
|----|---------|-------------|----------|
| FR-14 | Voice Commands | Execute actions via voice (e.g., "new line", "delete that") | Could Have |
| FR-15 | Punctuation Commands | Add punctuation via voice ("period", "comma") | Could Have |
| FR-16 | Text Formatting | Voice commands for formatting ("bold", "italic") | Won't Have (v1) |
| FR-17 | Custom Vocabulary | Add custom words/phrases for better recognition | Could Have |
| FR-18 | Transcription History | Keep log of recent transcriptions | Could Have |

---

## 3. Non-Functional Requirements

### 3.1 Performance (CPU-Only)

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-01 | Transcription Latency | < 4 seconds for 10-second audio clip (with base model on 4-core CPU) |
| NFR-02 | Idle Memory Usage | < 100 MB when not transcribing |
| NFR-03 | Active Memory Usage | < 2 GB during transcription (model dependent) |
| NFR-04 | CPU Usage (Idle) | < 1% |
| NFR-05 | Startup Time | < 5 seconds to be ready for input |

### 3.2 Reliability

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-06 | Uptime | Should run continuously without crashes |
| NFR-07 | Error Recovery | Gracefully handle audio device disconnection |
| NFR-08 | Model Loading | Handle missing/corrupted model files gracefully |
| NFR-08a | Platform Detection | Auto-detect OS and display server (X11/Wayland/macOS) |
| NFR-08b | Permission Handling | Guide users through granting required permissions (macOS) |

### 3.3 Usability

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-09 | Setup | One-click installer or simple extraction |
| NFR-10 | Learning Curve | Usable within 1 minute of first launch |
| NFR-11 | Accessibility | Keyboard-navigable settings |

### 3.4 Security & Privacy

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-12 | Offline Operation | 100% offline after initial setup |
| NFR-13 | Data Storage | No audio recordings saved by default |
| NFR-14 | No Telemetry | Zero data sent to external servers |

---

## 4. Technical Requirements

### 4.1 System Requirements

#### Windows

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| OS | Windows 10 (64-bit) | Windows 11 |
| RAM | 4 GB | 8 GB+ |
| Storage | 1 GB (tiny model) | 3 GB (base/small models) |
| CPU | Any 64-bit processor | Modern multi-core CPU (4+ cores) |
| Microphone | Any input device | Quality USB/headset microphone |

#### Ubuntu Linux

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| OS | Ubuntu 20.04 LTS (64-bit) | Ubuntu 22.04 LTS / 24.04 LTS |
| RAM | 4 GB | 8 GB+ |
| Storage | 1 GB (tiny model) | 3 GB (base/small models) |
| CPU | Any 64-bit processor | Modern multi-core CPU (4+ cores) |
| Microphone | Any input device | Quality USB/headset microphone |
| Audio System | PulseAudio or PipeWire | PipeWire |
| Desktop | GNOME, KDE, XFCE, or similar | GNOME 42+ / KDE Plasma 5+ |

#### macOS

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| OS | macOS 11 Big Sur | macOS 13 Ventura or newer |
| Architecture | Intel x64 or Apple Silicon | Apple Silicon (M1/M2/M3) |
| RAM | 4 GB | 8 GB+ (unified memory on Apple Silicon) |
| Storage | 1 GB (tiny model) | 3 GB (base/small models) |
| Microphone | Any input device | Built-in or quality external mic |
| Permissions | Accessibility + Microphone | Same |

### 4.2 Technology Stack

#### Option A: Python Stack

| Component | Technology | Rationale |
|-----------|------------|-----------|
| Language | Python 3.10+ | Rapid development, rich ecosystem, cross-platform |
| Speech Recognition | faster-whisper | Optimized Whisper implementation |
| Audio Capture | sounddevice | Cross-platform audio input (uses PortAudio) |
| Hotkey Handling | pynput | Global hotkey support (Windows & Linux) |
| System Tray | pystray | Cross-platform system tray integration |
| Clipboard | pyperclip | Cross-platform clipboard access |
| Auto-Typing | pynput | Cross-platform keyboard simulation |
| Configuration | JSON / YAML | Human-readable config files |
| Packaging (Windows) | PyInstaller | Single executable distribution |
| Packaging (Linux) | PyInstaller / AppImage / .deb | Flexible distribution options |
| Notifications | plyer | Cross-platform desktop notifications |

#### Option B: C# / .NET Stack (Recommended)

| Component | Technology | Rationale |
|-----------|------------|-----------|
| Language | C# / .NET 8 | Native performance, cross-platform, single binary |
| Speech Recognition | Whisper.net | C# bindings for whisper.cpp |
| Audio Capture | NAudio + OpenAL | NAudio (Windows), OpenAL (Linux) |
| UI Framework | Avalonia UI 11 | True cross-platform UI (WPF-like) |
| Hotkey Handling | SharpHook | Cross-platform global keyboard hooks |
| System Tray | Avalonia TrayIcon | Native tray support |
| Clipboard | Avalonia Clipboard | Built-in cross-platform clipboard |
| Auto-Typing | SharpHook | Cross-platform input simulation |
| Configuration | System.Text.Json | Built-in JSON support |
| Packaging | dotnet publish | Self-contained single-file executable |
| Notifications | Avalonia Notifications | Cross-platform notifications |

#### Stack Comparison

| Criteria | Python | C# (.NET 8) | Winner |
|----------|--------|-------------|--------|
| Runtime Required | Yes (or bundled ~100MB) | No (self-contained) | C# |
| Startup Time | ~3-5 seconds | ~1-2 seconds | C# |
| Memory Usage | Higher | Lower | C# |
| Whisper Ecosystem | Excellent | Good | Python |
| Windows Integration | Good | Excellent | C# |
| Linux Integration | Good | Good | Tie |
| Development Speed | Fast | Moderate | Python |
| Long-term Maintenance | Moderate | Better | C# |
| Executable Size | ~80-150 MB | ~50-80 MB | C# |

**Recommendation:** C# with .NET 8 for better performance, smaller binaries, and native feel.

### 4.2.1 Platform-Specific Dependencies

#### Windows (Both Stacks)
- No additional system dependencies required

#### Ubuntu Linux - Python Stack
- **Required packages:**
  ```bash
  sudo apt install portaudio19-dev python3-dev python3-tk
  sudo apt install libgirepository1.0-dev gir1.2-appindicator3-0.1
  ```
- **For X11 (keyboard simulation):**
  ```bash
  sudo apt install xdotool xclip
  ```
- **For Wayland (keyboard simulation):**
  ```bash
  sudo apt install wtype wl-clipboard
  ```

#### Ubuntu Linux - C# Stack
- **Required packages:**
  ```bash
  sudo apt install libx11-dev libxrandr-dev libxi-dev
  sudo apt install libopenal-dev libavcodec-dev libavformat-dev
  ```
- **For system tray (GNOME):**
  ```bash
  sudo apt install gnome-shell-extension-appindicator
  ```

#### macOS - C# Stack
- **No additional packages required** (ships with necessary frameworks)
- **Required permissions (requested at runtime):**
  - Microphone access (for audio recording)
  - Accessibility access (for global hotkeys and auto-typing)
- **For distribution:**
  - Apple Developer account (for code signing & notarization)
  - Or users must allow "unidentified developer" in System Preferences
- Optional: Create universal binary for Intel + Apple Silicon

### 4.3 Distribution Strategy

#### Primary Channel: GitHub Releases

The application will be distributed via **GitHub Releases** as a free, open-source project.

**Repository Structure:**
```
github.com/[username]/echotext/
├── releases/
│   └── v1.0.0/
│       ├── EchoText-1.0.0-win-x64.zip
│       ├── EchoText-1.0.0-linux-x64.tar.gz
│       ├── EchoText-1.0.0-linux-x64.AppImage
│       ├── EchoText-1.0.0-osx-x64.dmg
│       ├── EchoText-1.0.0-osx-arm64.dmg
│       └── checksums.txt (SHA256)
```

#### Build Artifacts Per Platform

| Platform | Format | Contents | Size (Est.) |
|----------|--------|----------|-------------|
| **Windows x64** | `.zip` | Self-contained .exe + runtime | ~60-80 MB |
| **Linux x64** | `.tar.gz` | Self-contained binary | ~60-80 MB |
| **Linux x64** | `.AppImage` | Portable single-file | ~80-100 MB |
| **macOS Intel** | `.dmg` | App bundle (.app) | ~60-80 MB |
| **macOS ARM64** | `.dmg` | App bundle (.app) | ~55-75 MB |

#### Installation Instructions (Per Platform)

**Windows:**
```
1. Download EchoText-x.x.x-win-x64.zip from GitHub Releases
2. Extract to desired location (e.g., C:\Program Files\EchoText)
3. Run EchoText.exe
4. If "Windows protected your PC" appears:
   → Click "More info" → Click "Run anyway"
5. (Optional) Create shortcut or pin to taskbar
```

**Linux:**
```
# Option A: AppImage (recommended)
1. Download EchoText-x.x.x-linux-x64.AppImage
2. chmod +x EchoText-*.AppImage
3. ./EchoText-*.AppImage

# Option B: tar.gz
1. Download EchoText-x.x.x-linux-x64.tar.gz
2. tar -xzf EchoText-*.tar.gz
3. cd EchoText
4. ./EchoText
```

**macOS:**
```
1. Download EchoText-x.x.x-osx-arm64.dmg (Apple Silicon)
   or EchoText-x.x.x-osx-x64.dmg (Intel)
2. Open the .dmg file
3. Drag EchoText to Applications folder
4. First launch: Right-click → Open → Click "Open" in dialog
   (Required because app is not notarized)
5. Grant Accessibility permission when prompted
6. Grant Microphone permission on first recording
```

#### Security Warnings & Mitigations

| Platform | Warning | User Action | One-Time |
|----------|---------|-------------|----------|
| Windows | "Windows protected your PC" (SmartScreen) | More info → Run anyway | Yes |
| macOS | "Cannot be opened because developer cannot be verified" | Right-click → Open | Yes |
| Linux | None | chmod +x (for AppImage) | Yes |

**Note:** These warnings appear because we're not code-signing the binaries. This is acceptable for open-source software where users can:
- Verify the source code on GitHub
- Check SHA256 checksums against `checksums.txt`
- Build from source themselves if preferred

#### Auto-Update Mechanism

The app will check for updates via GitHub Releases API:

```
GET https://api.github.com/repos/[username]/echotext/releases/latest
```

**Update Flow:**
1. App checks GitHub API on startup (configurable: daily/weekly/never)
2. If newer version exists, show notification with changelog
3. User clicks "Download" → Opens GitHub release page in browser
4. User downloads and installs manually (no silent updates)

**Future Enhancement:** In-app download + automatic replacement (v2)

#### CI/CD Pipeline (GitHub Actions)

```yaml
# .github/workflows/release.yml
name: Build and Release

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    strategy:
      matrix:
        include:
          - os: windows-latest
            rid: win-x64
          - os: ubuntu-latest
            rid: linux-x64
          - os: macos-latest
            rid: osx-x64
          - os: macos-latest
            rid: osx-arm64

    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet publish -c Release -r ${{ matrix.rid }} --self-contained
      - uses: actions/upload-artifact@v4
        # ... upload build artifacts

  release:
    needs: build
    runs-on: ubuntu-latest
    steps:
      # Create GitHub release with all artifacts
```

### 4.4 Whisper Model Distribution

Models are **NOT bundled** with the application (too large). Instead:

**First-Run Experience:**
1. User launches app for first time
2. App detects no model installed
3. Dialog: "Download speech recognition model?"
4. User selects model size (tiny/base/small/medium)
5. App downloads from Hugging Face (official source)
6. Model saved to: `~/.echotext/models/`

**Model Download Sources:**
```
https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin
https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.bin
# etc.
```

| Model | Download Size | Disk Size |
|-------|---------------|-----------|
| tiny | ~75 MB | ~75 MB |
| base | ~142 MB | ~142 MB |
| small | ~466 MB | ~466 MB |
| medium | ~1.5 GB | ~1.5 GB |

### 4.5 Whisper Model Options (CPU-Only)

| Model | Size | RAM Required | Speed (10s audio)* | Accuracy |
|-------|------|--------------|-------------------|----------|
| tiny | ~75 MB | ~1 GB | ~1-2 seconds | Basic |
| base | ~140 MB | ~1 GB | ~2-4 seconds | Good |
| small | ~460 MB | ~2 GB | ~5-10 seconds | Better |
| medium | ~1.5 GB | ~5 GB | ~15-30 seconds | Great |
| large | ~3 GB | ~10 GB | ~30-60 seconds | Best |

*Approximate times on a modern 4-core CPU. Actual performance varies by hardware.

**Default recommendation:** `base` model for balance of speed and accuracy

**Note:** GPU acceleration (CUDA/Metal) is intentionally not supported to keep the application simple and ensure it works on any machine without special drivers.

---

## 5. User Interface Requirements

### 5.1 System Tray

- **Icon States:**
  - Idle (default icon)
  - Recording (red/pulsing icon)
  - Processing (animated/spinning icon)
  - Error (warning icon)

- **Right-Click Menu:**
  - Settings
  - Pause/Resume
  - View History (if enabled)
  - About
  - Exit

### 5.2 Settings Window

```
┌─────────────────────────────────────────────────┐
│ EchoText Settings                        [X]  │
├─────────────────────────────────────────────────┤
│                                                 │
│  🎤 Audio                                       │
│  ├─ Input Device: [Dropdown]                    │
│  └─ Test Microphone [Button]                    │
│                                                 │
│  ⌨️ Hotkey                                      │
│  ├─ Mode: ( ) Push-to-Talk  ( ) Toggle          │
│  └─ Key Combination: [Ctrl+Shift+Space] [Set]   │
│                                                 │
│  📝 Output                                      │
│  ├─ [✓] Copy to clipboard                       │
│  ├─ [ ] Auto-type into active window            │
│  └─ [ ] Play sound on completion                │
│                                                 │
│  🤖 Recognition                                 │
│  ├─ Model: [base ▼]                             │
│  └─ Language: [Auto-detect ▼]                   │
│                                                 │
│  ⚙️ General                                     │
│  ├─ [ ] Start with Windows                      │
│  └─ [ ] Show notifications                      │
│                                                 │
│              [Save]  [Cancel]                   │
└─────────────────────────────────────────────────┘
```

### 5.3 Recording Overlay (Optional)

- Small floating indicator showing:
  - Recording duration
  - Audio level meter
  - Cancel button

---

## 6. User Workflows

### 6.1 Primary Workflow: Quick Dictation

```
1. User positions cursor in text field (any application)
2. User holds hotkey (e.g., Ctrl+Shift+Space)
3. System shows "Recording" indicator
4. User speaks
5. User releases hotkey
6. System shows "Processing" indicator
7. System transcribes audio using Whisper
8. System copies text to clipboard (and/or types it)
9. System shows "Done" indicator briefly
10. User pastes text (Ctrl+V) if using clipboard mode
```

### 6.2 First-Time Setup

```
1. User runs installer/executable
2. System downloads default model (if not bundled)
3. Settings window opens
4. User selects microphone
5. User tests microphone
6. User sets preferred hotkey
7. User saves settings
8. App minimizes to system tray
9. User is ready to dictate
```

---

## 7. Constraints & Assumptions

### 7.1 Constraints

- Must work offline (no internet required after setup)
- **Supported platforms: Windows 10/11, Ubuntu 20.04+, and macOS 11+ (v1)**
- Single-user application (no multi-user support)
- English as primary language (other languages supported but not optimized)
- **Linux: X11 fully supported; Wayland has limited hotkey/typing support**
- **macOS: Requires user to grant Accessibility and Microphone permissions**

### 7.2 Assumptions

- User has a working microphone
- User has sufficient disk space for model files
- User has administrative rights for installation (optional)

---

## 8. Success Metrics

| Metric | Target |
|--------|--------|
| Transcription Accuracy | > 90% for clear speech |
| End-to-End Latency | < 5 seconds for typical sentence (CPU-only) |
| Daily Active Usage | User uses app multiple times per day |
| Crash Rate | < 1 crash per week of continuous use |

---

## 9. GitHub Repository Structure

```
echotext/
├── .github/
│   ├── workflows/
│   │   ├── build.yml              # CI: Build on every push
│   │   └── release.yml            # CD: Build & release on tags
│   └── ISSUE_TEMPLATE/
│       ├── bug_report.md
│       └── feature_request.md
│
├── src/
│   └── EchoText/                # Main application source
│
├── docs/
│   ├── REQUIREMENTS.md            # This document
│   ├── ARCHITECTURE.md            # Technical design
│   └── CONTRIBUTING.md            # How to contribute
│
├── assets/
│   ├── icons/
│   └── screenshots/               # For README
│
├── README.md                      # Main documentation
├── LICENSE                        # MIT or Apache 2.0
├── CHANGELOG.md                   # Release notes
└── .gitignore
```

### README.md Requirements

The README should include:
1. **Hero Section** - App name, tagline, screenshot/GIF
2. **Features** - Key capabilities
3. **Installation** - Per-platform instructions with screenshots
4. **Usage** - Quick start guide
5. **Configuration** - Settings explanation
6. **Building from Source** - For developers
7. **Contributing** - How to help
8. **License** - Open source license

---

## 10. Out of Scope (v1)

- GPU acceleration (CUDA/Metal) - CPU-only for simplicity
- Other Linux distributions (Fedora, Arch, etc. - may work but not officially supported)
- Real-time streaming transcription
- Speaker diarization (multiple speakers)
- Cloud backup/sync
- Mobile companion app (iOS/Android)
- Browser extension
- API for other applications
- Flatpak/Snap packaging (may be added later)
- Apple App Store / Microsoft Store distribution
- Code signing / notarization (may add later if demand exists)

---

## 11. Open Questions

### Resolved Decisions
| Decision | Resolution |
|----------|------------|
| Stack | C# / .NET 8 with Avalonia UI |
| Distribution | GitHub Releases (free, open-source) |
| Code Signing | Not required (users bypass OS warnings) |
| Model Bundling | Download on first run from Hugging Face |
| Linux Format | AppImage + tar.gz |
| macOS Builds | Separate Intel and ARM64 DMGs |
| GPU Support | CPU-only (no CUDA/Metal) |

### Remaining Questions

**General:**
1. **Default Model:** Which Whisper model should be default? `tiny` (fast) or `base` (balanced)?
2. **Auto-Update:** Check on startup or manual only?
3. **Telemetry:** Anonymous usage stats (opt-in) or completely offline?

**Platform-Specific:**
4. **Linux Wayland:** Invest in Wayland support or recommend X11 session?
5. **macOS Permissions:** How to gracefully handle denied Accessibility permission?

**UX Questions:**
6. **First-Run Wizard:** Full setup wizard or minimal "just works" approach?
7. **Hotkey Default:** What should the default hotkey be? (`Ctrl+Shift+Space`? `Ctrl+``?)
8. **Recording Feedback:** Audio beep, visual overlay, or both?

---

## 12. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-01-28 | Claude | Initial draft |

---

## Appendix A: Competitive Analysis

| Feature | Wispr Flow | Windows Voice Typing | macOS Dictation | Our App |
|---------|------------|---------------------|-----------------|---------|
| Windows | ✓ | ✓ | ❌ | ✓ |
| Linux | ❌ | ❌ | ❌ | ✓ |
| macOS | ✓ | ❌ | ✓ | ✓ |
| Offline | ❌ | ✓ (limited) | ✓ (limited) | ✓ |
| Accuracy | High | Medium | Medium | High (Whisper) |
| Privacy | Cloud-based | Mixed | Mixed | 100% Local |
| Custom Hotkeys | ✓ | Limited | Limited | ✓ |
| Free | ❌ ($8-24/mo) | ✓ | ✓ | ✓ |
| Model Selection | ❌ | ❌ | ❌ | ✓ |
| Open Source | ❌ | ❌ | ❌ | ✓ |

---

## Appendix B: Linux Platform Considerations

### B.1 Display Server Compatibility

| Feature | X11 | Wayland |
|---------|-----|---------|
| Global Hotkeys | ✓ Full support | ⚠️ Limited (needs portal) |
| Auto-Type | ✓ Full support | ⚠️ Limited (wtype) |
| Clipboard | ✓ xclip | ✓ wl-clipboard |
| System Tray | ✓ Full support | ✓ With AppIndicator |

### B.2 Recommended Approach

1. **Primary target:** X11 (still default on Ubuntu 22.04)
2. **Wayland:** Basic support with graceful degradation
3. **Detection:** Auto-detect display server at runtime
4. **Fallback:** If Wayland features fail, prompt user to use X11 session

### B.3 Distribution Strategy (via GitHub Releases)

| Format | Provided | Notes |
|--------|----------|-------|
| **AppImage** | ✓ Yes | Recommended for most users |
| **tar.gz** | ✓ Yes | For advanced users / custom setups |
| **.deb** | ❌ No (v1) | May add later if demand exists |

**Installation:** Users download from GitHub Releases page and run directly.

---

## Appendix B2: macOS Platform Considerations

### B2.1 Permission Requirements

| Permission | Purpose | When Requested |
|------------|---------|----------------|
| **Microphone** | Audio recording | First recording attempt |
| **Accessibility** | Global hotkeys & auto-typing | App startup |

### B2.2 Permission Grant Flow

```
1. User launches app for first time
2. App requests Accessibility permission
3. System shows "EchoText wants to control this computer"
4. User must go to System Preferences → Privacy & Security → Accessibility
5. User enables EchoText in the list
6. App may need restart to pick up permission
7. On first recording, Microphone permission is requested
8. User clicks "Allow" in system dialog
```

### B2.3 Distribution (via GitHub Releases)

| Aspect | Decision |
|--------|----------|
| **Format** | .dmg disk image |
| **Code Signing** | ❌ Unsigned (free) |
| **Notarization** | ❌ Not notarized |
| **User Experience** | User must right-click → Open on first launch |

**Why Unsigned?**
- No $99/year Apple Developer fee
- Users can verify source code on GitHub
- One-time bypass is acceptable for power users
- Can add signing later if user demand exists

**First Launch Instructions (shown in README):**
```
1. Download the .dmg for your Mac (Intel or Apple Silicon)
2. Open the .dmg and drag EchoText to Applications
3. First time only: Right-click the app → Click "Open"
4. Click "Open" in the security dialog
5. Grant permissions when prompted
```

### B2.4 Architecture Support

| Build | Provided | Target | Notes |
|-------|----------|--------|-------|
| **osx-x64** | ✓ Yes | Intel Macs | Also works on Apple Silicon via Rosetta 2 |
| **osx-arm64** | ✓ Yes | Apple Silicon (M1/M2/M3/M4) | Best performance |
| **Universal** | ❌ No | Both | Too large (~110 MB), separate builds preferred |

**User Guidance:** README will help users identify their Mac architecture.

### B2.5 macOS-Specific Features

- **Menu Bar Icon:** Native macOS menu bar integration (not system tray)
- **Dark Mode:** Automatic support via Avalonia
- **Retina Display:** High-DPI support built-in
- **Apple Silicon:** Native performance on M1/M2/M3 chips

---

## Appendix C: C# Project Structure (If Selected)

```
EchoText/
├── src/
│   ├── EchoText/                    # Main application
│   │   ├── App.axaml                  # Avalonia app definition
│   │   ├── App.axaml.cs
│   │   ├── Program.cs                 # Entry point
│   │   ├── EchoText.csproj
│   │   │
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs
│   │   │   └── SettingsViewModel.cs
│   │   │
│   │   ├── Views/
│   │   │   ├── MainWindow.axaml
│   │   │   ├── SettingsWindow.axaml
│   │   │   └── RecordingOverlay.axaml
│   │   │
│   │   ├── Services/
│   │   │   ├── IWhisperService.cs
│   │   │   ├── WhisperService.cs      # Whisper.net integration
│   │   │   ├── IAudioCaptureService.cs
│   │   │   ├── AudioCaptureService.cs
│   │   │   ├── IHotkeyService.cs
│   │   │   ├── HotkeyService.cs       # SharpHook integration
│   │   │   ├── IClipboardService.cs
│   │   │   ├── ClipboardService.cs
│   │   │   ├── IAutoTypeService.cs
│   │   │   └── AutoTypeService.cs
│   │   │
│   │   ├── Models/
│   │   │   ├── AppSettings.cs
│   │   │   ├── TranscriptionResult.cs
│   │   │   └── RecordingState.cs
│   │   │
│   │   └── Helpers/
│   │       ├── PlatformHelper.cs      # OS detection
│   │       └── ModelDownloader.cs
│   │
│   └── EchoText.Tests/              # Unit tests
│       └── EchoText.Tests.csproj
│
├── assets/
│   ├── icons/
│   │   ├── tray-idle.ico
│   │   ├── tray-recording.ico
│   │   └── tray-processing.ico
│   └── sounds/
│       ├── start.wav
│       └── stop.wav
│
├── models/                            # Whisper models (downloaded)
│   └── .gitkeep
│
├── EchoText.sln
├── README.md
└── LICENSE
```

### Key NuGet Packages (C#)

```xml
<ItemGroup>
  <!-- UI Framework -->
  <PackageReference Include="Avalonia" Version="11.0.10" />
  <PackageReference Include="Avalonia.Desktop" Version="11.0.10" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.10" />
  
  <!-- Speech Recognition -->
  <PackageReference Include="Whisper.net" Version="1.5.0" />
  <PackageReference Include="Whisper.net.Runtime" Version="1.5.0" />
  
  <!-- Audio -->
  <PackageReference Include="NAudio" Version="2.2.1" />
  
  <!-- Global Hotkeys -->
  <PackageReference Include="SharpHook" Version="5.3.1" />
  
  <!-- MVVM -->
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
</ItemGroup>
```

---

## Appendix D: Glossary

- **Push-to-Talk (PTT):** Recording only while hotkey is held down
- **Toggle Mode:** Press once to start, press again to stop
- **Whisper:** OpenAI's open-source speech recognition model
- **faster-whisper:** Optimized implementation of Whisper using CTranslate2
- **System Tray:** Windows notification area (bottom-right of taskbar)
