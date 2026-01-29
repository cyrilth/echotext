# EchoText - Development Tasks

**Version:** 1.0
**Date:** January 28, 2025
**Status:** Ready for Development
**Related:** [REQUIREMENTS.md](./REQUIREMENTS.md) | [ARCHITECTURE.md](./ARCHITECTURE.md)

---

## Overview

This document breaks down EchoText development into sequential, actionable tasks. Each task includes dependencies, acceptance criteria, and estimated complexity.

### Complexity Legend

| Symbol | Meaning | Estimated Effort |
|--------|---------|------------------|
| 🟢 | Simple | < 1 hour |
| 🟡 | Medium | 1-4 hours |
| 🔴 | Complex | 4+ hours |

### Task Status Legend

| Symbol | Meaning |
|--------|---------|
| ⬜ | Not Started |
| 🔄 | In Progress |
| ✅ | Complete |
| ❌ | Blocked |

---

## Phase 1: Project Setup

### Task 1.1: Create Solution Structure
| | |
|---|---|
| **ID** | TASK-101 |
| **Complexity** | 🟢 Simple |
| **Dependencies** | None |
| **Status** | ✅ Complete |

**Description:**
Create the .NET solution with project structure as defined in ARCHITECTURE.md.

**Steps:**
1. Create solution file `EchoText.sln`
2. Create main project `src/EchoText/EchoText.csproj`
3. Create test project `src/EchoText.Tests/EchoText.Tests.csproj`
4. Create folder structure:
   - Models/
   - Services/Interfaces/
   - Platform/Interfaces/
   - Platform/Windows/
   - Platform/Linux/
   - Platform/MacOS/
   - ViewModels/
   - Views/
   - Assets/Icons/
   - Assets/Sounds/

**Acceptance Criteria:**
- [ ] Solution builds without errors
- [ ] All folders exist as per ARCHITECTURE.md
- [ ] Test project references main project
- [ ] `dotnet build` succeeds

---

### Task 1.2: Add NuGet Dependencies
| | |
|---|---|
| **ID** | TASK-102 |
| **Complexity** | 🟢 Simple |
| **Dependencies** | TASK-101 |
| **Status** | ✅ Complete |

**Description:**
Add all required NuGet packages to the project files.

**Packages for EchoText.csproj:**
```xml
<ItemGroup>
  <!-- UI Framework -->
  <PackageReference Include="Avalonia" Version="11.0.10" />
  <PackageReference Include="Avalonia.Desktop" Version="11.0.10" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.10" />
  <PackageReference Include="Avalonia.Diagnostics" Version="11.0.10" Condition="'$(Configuration)' == 'Debug'" />

  <!-- MVVM -->
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />

  <!-- Dependency Injection -->
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />

  <!-- Speech Recognition -->
  <PackageReference Include="Whisper.net" Version="1.5.0" />
  <PackageReference Include="Whisper.net.Runtime" Version="1.5.0" />

  <!-- Audio -->
  <PackageReference Include="NAudio" Version="2.2.1" />

  <!-- Global Hotkeys -->
  <PackageReference Include="SharpHook" Version="5.3.1" />

  <!-- Logging -->
  <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
  <PackageReference Include="Serilog.Extensions.Logging.File" Version="3.0.0" />
</ItemGroup>
```

**Packages for EchoText.Tests.csproj:**
```xml
<ItemGroup>
  <PackageReference Include="xunit" Version="2.7.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
</ItemGroup>
```

**Acceptance Criteria:**
- [ ] `dotnet restore` succeeds
- [ ] All packages resolve correctly
- [ ] `dotnet build` succeeds

---

### Task 1.3: Create Base Avalonia App
| | |
|---|---|
| **ID** | TASK-103 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-102 |
| **Status** | ✅ Complete |

**Description:**
Set up the basic Avalonia application with entry point and app definition.

**Files to Create:**
1. `Program.cs` - Entry point with DI setup
2. `App.axaml` - Avalonia app definition
3. `App.axaml.cs` - App code-behind
4. `Views/MainWindow.axaml` - Empty main window
5. `Views/MainWindow.axaml.cs` - Main window code-behind

**Acceptance Criteria:**
- [ ] Application launches without errors
- [ ] Empty window appears on screen
- [ ] Application exits cleanly when window is closed
- [ ] Works on current development platform

---

## Phase 2: Core Models & Configuration

### Task 2.1: Implement Data Models
| | |
|---|---|
| **ID** | TASK-201 |
| **Complexity** | 🟢 Simple |
| **Dependencies** | TASK-101 |
| **Status** | ✅ Complete |

**Description:**
Create all data model classes as defined in ARCHITECTURE.md.

**Files to Create:**
1. `Models/AppState.cs` - AppState enum
2. `Models/AppSettings.cs` - Settings classes (AppSettings, HotkeySettings, OutputSettings, RecognitionSettings, GeneralSettings)
3. `Models/AudioDevice.cs` - AudioDevice record
4. `Models/WhisperModel.cs` - WhisperModel record
5. `Models/KeyModifiers.cs` - KeyModifiers flags enum
6. `Models/HotkeyMode.cs` - HotkeyMode enum
7. `Models/NotificationType.cs` - NotificationType enum
8. `Models/SoundEffect.cs` - SoundEffect enum
9. `Models/Result.cs` - Result<T> class for error handling

**Acceptance Criteria:**
- [ ] All model classes compile
- [ ] All properties have appropriate types
- [ ] Default values set correctly in AppSettings
- [ ] Result<T> has Success and Failure factory methods

---

### Task 2.2: Implement PlatformInfo
| | |
|---|---|
| **ID** | TASK-202 |
| **Complexity** | 🟢 Simple |
| **Dependencies** | TASK-201 |
| **Status** | ✅ Complete |

**Description:**
Create platform detection and path resolution utilities.

**Files to Create:**
1. `Platform/PlatformInfo.cs`

**Implementation:**
```csharp
public static class PlatformInfo
{
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static string ConfigDirectory { get; }
    public static string ModelsDirectory { get; }
    public static string LogDirectory { get; }
}
```

**Acceptance Criteria:**
- [ ] Correctly detects current OS
- [ ] Returns correct config path for current OS
- [ ] Returns correct models path for current OS
- [ ] Paths use correct separators for OS

---

### Task 2.3: Implement ConfigService
| | |
|---|---|
| **ID** | TASK-203 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-201, TASK-202 |
| **Status** | ✅ Complete |

**Description:**
Implement configuration loading and saving service.

**Files to Create:**
1. `Services/Interfaces/IConfigService.cs`
2. `Services/ConfigService.cs`

**Functionality:**
- Load settings from JSON file
- Save settings to JSON file
- Create default settings if file doesn't exist
- Fire event when settings change
- Thread-safe access

**Acceptance Criteria:**
- [ ] Creates config directory if not exists
- [ ] Creates default config file if not exists
- [ ] Loads existing config correctly
- [ ] Saves modified config correctly
- [ ] Fires SettingsChanged event on save
- [ ] Unit tests pass

---

## Phase 3: Platform Abstraction Layer

### Task 3.1: Define Platform Interfaces
| | |
|---|---|
| **ID** | TASK-301 |
| **Complexity** | 🟢 Simple |
| **Dependencies** | TASK-201 |
| **Status** | ✅ Complete |

**Description:**
Create all platform abstraction interfaces.

**Files to Create:**
1. `Platform/Interfaces/IPlatformHotkey.cs`
2. `Platform/Interfaces/IPlatformAudio.cs`
3. `Platform/Interfaces/IPlatformClipboard.cs`
4. `Platform/Interfaces/IPlatformOutput.cs`

**Acceptance Criteria:**
- [ ] All interfaces defined as per ARCHITECTURE.md
- [ ] Interfaces are minimal and focused
- [ ] XML documentation on all methods

---

### Task 3.2: Implement Windows Platform Providers
| | |
|---|---|
| **ID** | TASK-302 |
| **Complexity** | 🔴 Complex |
| **Dependencies** | TASK-301 |
| **Status** | ✅ Complete |

**Description:**
Implement Windows-specific platform providers.

**Files to Create:**
1. `Platform/Windows/WindowsHotkeyProvider.cs` - Using SharpHook
2. `Platform/Windows/WindowsAudioProvider.cs` - Using NAudio
3. `Platform/Windows/WindowsClipboardProvider.cs` - Using Avalonia Clipboard
4. `Platform/Windows/WindowsOutputProvider.cs` - Using SharpHook

**Acceptance Criteria:**
- [x] Hotkey registration works on Windows
- [x] Hotkey press/release events fire correctly
- [x] Audio capture records from microphone
- [x] Audio output is 16kHz mono WAV format
- [x] Clipboard copy/paste works
- [x] Auto-type works in Notepad
- [x] All providers are disposable

---

### Task 3.3: Implement Linux Platform Providers
| | |
|---|---|
| **ID** | TASK-303 |
| **Complexity** | 🔴 Complex |
| **Dependencies** | TASK-301 |
| **Status** | ✅ Complete |

**Description:**
Implement Linux-specific platform providers.

**Files to Create:**
1. `Platform/Linux/LinuxHotkeyProvider.cs` - Using SharpHook
2. `Platform/Linux/LinuxAudioProvider.cs` - Using ALSA/PulseAudio via NAudio or OpenAL
3. `Platform/Linux/LinuxClipboardProvider.cs` - Using xclip/wl-clipboard
4. `Platform/Linux/LinuxOutputProvider.cs` - Using xdotool/wtype

**Acceptance Criteria:**
- [x] Hotkey works on X11
- [x] Audio capture works with PulseAudio
- [x] Clipboard works on X11
- [x] Auto-type works on X11
- [x] Graceful fallback if Wayland detected

---

### Task 3.4: Implement macOS Platform Providers
| | |
|---|---|
| **ID** | TASK-304 |
| **Complexity** | 🔴 Complex |
| **Dependencies** | TASK-301 |
| **Status** | ✅ Complete |

**Description:**
Implement macOS-specific platform providers.

**Files to Create:**
1. `Platform/MacOS/MacOSHotkeyProvider.cs` - Using SharpHook
2. `Platform/MacOS/MacOSAudioProvider.cs` - Using macOS audio APIs
3. `Platform/MacOS/MacOSClipboardProvider.cs` - Using Avalonia Clipboard
4. `Platform/MacOS/MacOSOutputProvider.cs` - Using SharpHook

**Acceptance Criteria:**
- [x] Hotkey works (with Accessibility permission)
- [x] Audio capture works (with Microphone permission)
- [x] Clipboard works
- [x] Auto-type works (with Accessibility permission)
- [x] Permission prompts shown appropriately

---

### Task 3.5: Implement PlatformServices Registration
| | |
|---|---|
| **ID** | TASK-305 |
| **Complexity** | 🟢 Simple |
| **Dependencies** | TASK-302, TASK-303, TASK-304 |
| **Status** | ✅ Complete |

**Description:**
Create platform service registration helper.

**Files to Create:**
1. `Platform/PlatformServices.cs`

**Acceptance Criteria:**
- [x] Registers correct providers based on OS
- [x] Throws clear error if unsupported OS

---

## Phase 4: Core Services

### Task 4.1: Implement AppStateManager
| | |
|---|---|
| **ID** | TASK-401 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-201 |
| **Status** | ✅ Complete |

**Description:**
Implement application state machine.

**Files to Create:**
1. `Services/Interfaces/IAppStateManager.cs`
2. `Services/AppStateManager.cs`

**States:** Loading → Idle ↔ Recording → Processing → Idle

**Acceptance Criteria:**
- [x] State transitions follow valid paths only
- [x] Invalid transitions throw or return false
- [x] StateChanged event fires on transition
- [x] Thread-safe state access
- [x] Unit tests for all transitions

---

### Task 4.2: Implement HotkeyService
| | |
|---|---|
| **ID** | TASK-402 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-301, TASK-203 |
| **Status** | ✅ Complete |

**Description:**
Implement hotkey service that wraps platform providers.

**Files to Create:**
1. `Services/Interfaces/IHotkeyService.cs`
2. `Services/HotkeyService.cs`

**Functionality:**
- Register/unregister hotkey based on config
- Fire HotkeyPressed/HotkeyReleased events
- Handle Push-to-Talk vs Toggle modes
- Re-register hotkey when config changes

**Acceptance Criteria:**
- [x] Hotkey can be registered
- [x] HotkeyPressed fires on key down
- [x] HotkeyReleased fires on key up
- [x] Push-to-Talk mode works correctly
- [x] Toggle mode works correctly
- [x] Config changes re-register hotkey
- [x] Unit tests pass

---

### Task 4.3: Implement AudioService
| | |
|---|---|
| **ID** | TASK-403 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-301, TASK-203 |
| **Status** | ✅ Complete |

**Description:**
Implement audio capture service.

**Files to Create:**
1. `Services/Interfaces/IAudioService.cs`
2. `Services/AudioService.cs`

**Functionality:**
- List available audio input devices
- Start/stop audio capture
- Return audio as 16kHz mono WAV bytes
- Report audio level for visualization
- Enforce max recording duration (120 seconds)

**Acceptance Criteria:**
- [ ] Lists input devices correctly
- [ ] Captures audio from selected device
- [ ] Output format is 16kHz mono 16-bit WAV
- [ ] AudioLevelChanged event fires during capture
- [ ] Recording stops at max duration
- [ ] IsRecording property is accurate
- [ ] RecordingDuration property is accurate
- [ ] Unit tests pass

---

### Task 4.4: Implement ModelManager
| | |
|---|---|
| **ID** | TASK-404 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-202 |
| **Status** | ⬜ Not Started |

**Description:**
Implement Whisper model download and management.

**Files to Create:**
1. `Services/Interfaces/IModelManager.cs`
2. `Services/ModelManager.cs`

**Model URLs (Hugging Face):**
```
tiny:   https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-tiny.bin
base:   https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin
small:  https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.bin
medium: https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-medium.bin
large:  https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-large-v3.bin
```

**Functionality:**
- List available models (with download status)
- Download model with progress reporting
- Get model file path
- Delete downloaded model

**Acceptance Criteria:**
- [ ] Lists all model options
- [ ] Correctly identifies downloaded models
- [ ] Downloads model with progress callback
- [ ] Handles download interruption gracefully
- [ ] Returns correct model paths
- [ ] Can delete models
- [ ] Unit tests pass

---

### Task 4.5: Implement TranscriptionService
| | |
|---|---|
| **ID** | TASK-405 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-404 |
| **Status** | ⬜ Not Started |

**Description:**
Implement Whisper transcription service.

**Files to Create:**
1. `Services/Interfaces/ITranscriptionService.cs`
2. `Services/TranscriptionService.cs`

**Functionality:**
- Load Whisper model from file
- Transcribe audio bytes to text
- Support language selection or auto-detect
- Unload model on dispose

**Acceptance Criteria:**
- [ ] Loads model successfully
- [ ] IsModelLoaded property is accurate
- [ ] Transcribes English speech correctly
- [ ] Auto-detect language works
- [ ] Returns empty string for silence
- [ ] Handles invalid audio gracefully
- [ ] Unit tests pass (with mock)

---

### Task 4.6: Implement ClipboardService
| | |
|---|---|
| **ID** | TASK-406 |
| **Complexity** | 🟢 Simple |
| **Dependencies** | TASK-301 |
| **Status** | ⬜ Not Started |

**Description:**
Implement clipboard service wrapper.

**Files to Create:**
1. `Services/Interfaces/IClipboardService.cs`
2. `Services/ClipboardService.cs`

**Acceptance Criteria:**
- [ ] SetTextAsync copies text to clipboard
- [ ] GetTextAsync retrieves text from clipboard
- [ ] Works on all platforms
- [ ] Unit tests pass

---

### Task 4.7: Implement OutputService
| | |
|---|---|
| **ID** | TASK-407 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-301, TASK-406 |
| **Status** | ⬜ Not Started |

**Description:**
Implement text output service (clipboard + auto-type).

**Files to Create:**
1. `Services/Interfaces/IOutputService.cs`
2. `Services/OutputService.cs`

**Functionality:**
- Copy text to clipboard
- Type text into active window (if enabled)
- Configurable keystroke delay

**Acceptance Criteria:**
- [ ] Copies text to clipboard
- [ ] Types text into active window
- [ ] Respects keystroke delay setting
- [ ] Handles special characters correctly
- [ ] Unit tests pass

---

### Task 4.8: Implement NotificationService
| | |
|---|---|
| **ID** | TASK-408 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-103 |
| **Status** | ⬜ Not Started |

**Description:**
Implement notifications and sound effects.

**Files to Create:**
1. `Services/Interfaces/INotificationService.cs`
2. `Services/NotificationService.cs`
3. `Assets/Sounds/start.wav`
4. `Assets/Sounds/stop.wav`
5. `Assets/Sounds/success.wav`
6. `Assets/Sounds/error.wav`

**Acceptance Criteria:**
- [ ] Shows toast notifications
- [ ] Plays sound effects
- [ ] Respects "show notifications" setting
- [ ] Respects "play sounds" setting
- [ ] Works on all platforms

---

## Phase 5: User Interface

### Task 5.1: Create Tray Icon & Context Menu
| | |
|---|---|
| **ID** | TASK-501 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-103, TASK-401 |
| **Status** | ⬜ Not Started |

**Description:**
Implement system tray icon with context menu.

**Files to Create/Modify:**
1. `Assets/Icons/tray-idle.ico`
2. `Assets/Icons/tray-recording.ico`
3. `Assets/Icons/tray-processing.ico`
4. `Assets/Icons/tray-error.ico`
5. `Views/MainWindow.axaml` - Add TrayIcon
6. `ViewModels/MainViewModel.cs` - Tray logic

**Menu Items:**
- Status indicator (non-clickable)
- Settings...
- Check for Updates
- ---
- About
- Exit

**Acceptance Criteria:**
- [ ] Tray icon appears on app start
- [ ] Icon changes based on app state
- [ ] Context menu appears on right-click
- [ ] Menu items trigger correct actions
- [ ] "Exit" closes app cleanly
- [ ] Works on all platforms

---

### Task 5.2: Create Settings Window
| | |
|---|---|
| **ID** | TASK-502 |
| **Complexity** | 🔴 Complex |
| **Dependencies** | TASK-203, TASK-402, TASK-403 |
| **Status** | ⬜ Not Started |

**Description:**
Implement settings window UI.

**Files to Create:**
1. `Views/SettingsWindow.axaml`
2. `Views/SettingsWindow.axaml.cs`
3. `ViewModels/SettingsViewModel.cs`

**Sections:**
- Audio: Device dropdown, test button
- Hotkey: Mode toggle, key combination picker
- Output: Clipboard checkbox, auto-type checkbox, sound checkbox
- Recognition: Model dropdown, language dropdown
- General: Start with system, show notifications

**Acceptance Criteria:**
- [ ] All settings displayed correctly
- [ ] Audio device dropdown populated
- [ ] Hotkey picker works
- [ ] Changes saved on Save button
- [ ] Changes discarded on Cancel button
- [ ] Model download triggered if needed
- [ ] Works on all platforms

---

### Task 5.3: Create Recording Overlay (Optional)
| | |
|---|---|
| **ID** | TASK-503 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-401, TASK-403 |
| **Status** | ⬜ Not Started |

**Description:**
Create optional floating recording indicator.

**Files to Create:**
1. `Views/RecordingOverlay.axaml`
2. `Views/RecordingOverlay.axaml.cs`

**Features:**
- Recording duration display
- Audio level meter
- Cancel button

**Acceptance Criteria:**
- [ ] Appears when recording starts
- [ ] Disappears when recording stops
- [ ] Shows recording duration
- [ ] Shows audio level
- [ ] Cancel button stops recording
- [ ] Always on top

---

### Task 5.4: Implement Main Orchestration Logic
| | |
|---|---|
| **ID** | TASK-504 |
| **Complexity** | 🔴 Complex |
| **Dependencies** | TASK-401 through TASK-408, TASK-501 |
| **Status** | ⬜ Not Started |

**Description:**
Wire up all services in MainViewModel to implement the core transcription workflow.

**Workflow:**
1. Hotkey pressed → Start recording
2. Hotkey released → Stop recording
3. Audio captured → Transcribe
4. Text received → Output to clipboard/auto-type
5. Update state throughout

**Files to Modify:**
1. `ViewModels/MainViewModel.cs`

**Acceptance Criteria:**
- [ ] Push-to-talk works end-to-end
- [ ] Toggle mode works end-to-end
- [ ] State transitions correctly
- [ ] Errors handled gracefully
- [ ] Notifications shown at appropriate times
- [ ] Sounds play at appropriate times

---

## Phase 6: Polish & Testing

### Task 6.1: Implement First-Run Experience
| | |
|---|---|
| **ID** | TASK-601 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-504, TASK-404 |
| **Status** | ⬜ Not Started |

**Description:**
Handle first-time launch gracefully.

**Flow:**
1. Detect no model downloaded
2. Show welcome dialog
3. Prompt to download model
4. Show progress during download
5. Ready to use

**Acceptance Criteria:**
- [ ] Detects first run correctly
- [ ] Shows welcome message
- [ ] Downloads default model (base)
- [ ] Shows download progress
- [ ] Transitions to ready state

---

### Task 6.2: Implement Update Checker
| | |
|---|---|
| **ID** | TASK-602 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-501 |
| **Status** | ⬜ Not Started |

**Description:**
Check GitHub releases for updates.

**Files to Create:**
1. `Services/Interfaces/IUpdateService.cs`
2. `Services/UpdateService.cs`

**Acceptance Criteria:**
- [ ] Checks GitHub API for latest release
- [ ] Compares version numbers correctly
- [ ] Shows notification if update available
- [ ] Opens GitHub releases page on click
- [ ] Respects "check for updates" setting

---

### Task 6.3: Write Unit Tests
| | |
|---|---|
| **ID** | TASK-603 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | All services |
| **Status** | ⬜ Not Started |

**Description:**
Write comprehensive unit tests for all services.

**Test Files to Create:**
1. `EchoText.Tests/Services/ConfigServiceTests.cs`
2. `EchoText.Tests/Services/AppStateManagerTests.cs`
3. `EchoText.Tests/Services/ModelManagerTests.cs`
4. `EchoText.Tests/ViewModels/MainViewModelTests.cs`
5. `EchoText.Tests/ViewModels/SettingsViewModelTests.cs`

**Acceptance Criteria:**
- [ ] >80% code coverage on services
- [ ] All state transitions tested
- [ ] Error conditions tested
- [ ] All tests pass

---

### Task 6.4: Add Logging
| | |
|---|---|
| **ID** | TASK-604 |
| **Complexity** | 🟢 Simple |
| **Dependencies** | All services |
| **Status** | ⬜ Not Started |

**Description:**
Add structured logging throughout the application.

**Log Locations:**
| Platform | Path |
|----------|------|
| Windows | `%APPDATA%\EchoText\logs\` |
| Linux | `~/.local/share/echotext/logs/` |
| macOS | `~/Library/Logs/EchoText/` |

**Acceptance Criteria:**
- [ ] All services log key operations
- [ ] Errors logged with stack traces
- [ ] Log files rotated (keep last 7 days)
- [ ] Log level configurable

---

## Phase 7: Build & Distribution

### Task 7.1: Create GitHub Actions CI
| | |
|---|---|
| **ID** | TASK-701 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-603 |
| **Status** | ⬜ Not Started |

**Description:**
Set up continuous integration workflow.

**Files to Create:**
1. `.github/workflows/build.yml`

**Workflow:**
- Trigger on push and PR
- Build on all platforms
- Run tests
- Report results

**Acceptance Criteria:**
- [ ] Builds on Windows runner
- [ ] Builds on Ubuntu runner
- [ ] Builds on macOS runner
- [ ] Tests run and report results
- [ ] Build fails if tests fail

---

### Task 7.2: Create GitHub Actions Release
| | |
|---|---|
| **ID** | TASK-702 |
| **Complexity** | 🟡 Medium |
| **Dependencies** | TASK-701 |
| **Status** | ⬜ Not Started |

**Description:**
Set up release automation.

**Files to Create:**
1. `.github/workflows/release.yml`

**Workflow:**
- Trigger on version tag (v*)
- Build all platform artifacts
- Create GitHub release
- Upload artifacts
- Generate checksums

**Artifacts:**
- `EchoText-{version}-win-x64.zip`
- `EchoText-{version}-linux-x64.tar.gz`
- `EchoText-{version}-linux-x64.AppImage`
- `EchoText-{version}-osx-x64.dmg`
- `EchoText-{version}-osx-arm64.dmg`
- `checksums.txt`

**Acceptance Criteria:**
- [ ] Release created on tag push
- [ ] All artifacts uploaded
- [ ] Checksums generated
- [ ] Release notes template applied

---

### Task 7.3: Create README
| | |
|---|---|
| **ID** | TASK-703 |
| **Complexity** | 🟢 Simple |
| **Dependencies** | TASK-702 |
| **Status** | ⬜ Not Started |

**Description:**
Write comprehensive README.md for GitHub.

**Sections:**
1. Hero (name, tagline, screenshot)
2. Features
3. Installation (per platform)
4. Usage
5. Configuration
6. Building from Source
7. Contributing
8. License

**Acceptance Criteria:**
- [ ] Clear installation instructions
- [ ] Screenshots/GIFs included
- [ ] All platforms documented
- [ ] Build instructions work

---

### Task 7.4: Create Additional Documentation
| | |
|---|---|
| **ID** | TASK-704 |
| **Complexity** | 🟢 Simple |
| **Dependencies** | TASK-703 |
| **Status** | ⬜ Not Started |

**Description:**
Create supporting documentation.

**Files to Create:**
1. `LICENSE` (MIT)
2. `CHANGELOG.md`
3. `CONTRIBUTING.md`
4. `.github/ISSUE_TEMPLATE/bug_report.md`
5. `.github/ISSUE_TEMPLATE/feature_request.md`

**Acceptance Criteria:**
- [ ] License file exists
- [ ] Changelog has v1.0.0 entry
- [ ] Contributing guide complete
- [ ] Issue templates work

---

## Task Dependency Graph

```
Phase 1: Setup
TASK-101 → TASK-102 → TASK-103

Phase 2: Models & Config
TASK-101 → TASK-201 → TASK-202 → TASK-203

Phase 3: Platform Layer
TASK-201 → TASK-301 → TASK-302 (Windows)
                    → TASK-303 (Linux)
                    → TASK-304 (macOS)
                    → TASK-305

Phase 4: Core Services
TASK-201 → TASK-401
TASK-301 + TASK-203 → TASK-402
TASK-301 + TASK-203 → TASK-403
TASK-202 → TASK-404 → TASK-405
TASK-301 → TASK-406 → TASK-407
TASK-103 → TASK-408

Phase 5: UI
TASK-103 + TASK-401 → TASK-501
TASK-203 + TASK-402 + TASK-403 → TASK-502
TASK-401 + TASK-403 → TASK-503
All Services → TASK-504

Phase 6: Polish
TASK-504 + TASK-404 → TASK-601
TASK-501 → TASK-602
All Services → TASK-603
All Services → TASK-604

Phase 7: Distribution
TASK-603 → TASK-701 → TASK-702 → TASK-703 → TASK-704
```

---

## Suggested Development Order

For a single developer or AI agent, here's the recommended order:

### Sprint 1: Foundation
1. ✅ TASK-101: Create Solution Structure
2. ✅ TASK-102: Add NuGet Dependencies
3. ✅ TASK-201: Implement Data Models
4. ⬜ TASK-202: Implement PlatformInfo
5. ⬜ TASK-203: Implement ConfigService
6. ⬜ TASK-103: Create Base Avalonia App

### Sprint 2: Platform Layer (Start with your dev platform)
7. ⬜ TASK-301: Define Platform Interfaces
8. ⬜ TASK-302/303/304: Implement YOUR platform providers
9. ⬜ TASK-305: Implement PlatformServices Registration

### Sprint 3: Core Services
10. ⬜ TASK-401: Implement AppStateManager
11. ⬜ TASK-402: Implement HotkeyService
12. ⬜ TASK-403: Implement AudioService
13. ⬜ TASK-404: Implement ModelManager
14. ⬜ TASK-405: Implement TranscriptionService
15. ⬜ TASK-406: Implement ClipboardService
16. ⬜ TASK-407: Implement OutputService
17. ⬜ TASK-408: Implement NotificationService

### Sprint 4: UI & Integration
18. ⬜ TASK-501: Create Tray Icon & Context Menu
19. ⬜ TASK-502: Create Settings Window
20. ⬜ TASK-504: Implement Main Orchestration Logic
21. ⬜ TASK-601: Implement First-Run Experience

### Sprint 5: Other Platforms
22. ⬜ TASK-302/303/304: Implement remaining platform providers

### Sprint 6: Polish & Release
23. ⬜ TASK-503: Create Recording Overlay (Optional)
24. ⬜ TASK-602: Implement Update Checker
25. ⬜ TASK-603: Write Unit Tests
26. ⬜ TASK-604: Add Logging
27. ⬜ TASK-701: Create GitHub Actions CI
28. ⬜ TASK-702: Create GitHub Actions Release
29. ⬜ TASK-703: Create README
30. ⬜ TASK-704: Create Additional Documentation

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-01-28 | Claude | Initial task breakdown |
