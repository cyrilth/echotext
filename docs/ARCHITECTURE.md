# EchoText - Architecture Document

**Version:** 1.0  
**Date:** January 28, 2025  
**Status:** Final  
**Related:** [REQUIREMENTS.md](./REQUIREMENTS.md)

---

## 1. Overview

EchoText is a cross-platform (Windows, Linux, macOS) desktop application that converts speech to text using OpenAI's Whisper model running locally. This document defines the system architecture, component interactions, and implementation patterns that all developers (human or AI) must follow.

### 1.1 Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Language | C# / .NET 8 | Cross-platform, performant, single binary |
| UI Framework | Avalonia UI 11 | True cross-platform, WPF-like |
| Speech Recognition | Whisper.net | C# bindings for whisper.cpp |
| Architecture Pattern | MVVM | Standard for Avalonia, testable |
| Dependency Injection | Microsoft.Extensions.DI | Standard, well-supported |
| Configuration | JSON | Simple, human-readable |

### 1.2 Design Principles

1. **Platform Abstraction** - All OS-specific code behind interfaces
2. **Single Responsibility** - Each service does one thing
3. **Dependency Injection** - All services injected, not instantiated
4. **Event-Driven** - Components communicate via events
5. **Fail Gracefully** - Always handle errors, never crash

---

## 2. System Architecture

### 2.1 High-Level Component Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              EchoText                                   │
├─────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      PRESENTATION LAYER                          │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │   │
│  │  │ TrayIcon    │  │ Settings    │  │ RecordingOverlay        │  │   │
│  │  │ (MainView)  │  │ Window      │  │ (Optional)              │  │   │
│  │  └──────┬──────┘  └──────┬──────┘  └───────────┬─────────────┘  │   │
│  │         └────────────────┼─────────────────────┘                │   │
│  │                          │                                       │   │
│  │                   ┌──────▼──────┐                               │   │
│  │                   │ ViewModels  │                               │   │
│  │                   └──────┬──────┘                               │   │
│  └──────────────────────────┼───────────────────────────────────────┘   │
│                             │                                           │
│  ┌──────────────────────────▼───────────────────────────────────────┐   │
│  │                       SERVICE LAYER                              │   │
│  │                                                                  │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐   │   │
│  │  │ HotkeyService│  │ AudioService │  │ TranscriptionService │   │   │
│  │  │              │  │              │  │                      │   │   │
│  │  │ - Register   │  │ - Capture    │  │ - LoadModel          │   │   │
│  │  │ - Listen     │  │ - GetDevices │  │ - Transcribe         │   │   │
│  │  └──────┬───────┘  └───────┬──────┘  └──────────┬───────────┘   │   │
│  │         │                  │                    │               │   │
│  │  ┌──────▼───────┐  ┌───────▼──────┐  ┌─────────▼───────────┐   │   │
│  │  │ ClipboardSvc │  │ OutputService│  │ NotificationService │   │   │
│  │  │              │  │              │  │                     │   │   │
│  │  │ - Copy       │  │ - TypeText   │  │ - Show              │   │   │
│  │  │ - Paste      │  │ - SendKeys   │  │ - PlaySound         │   │   │
│  │  └──────────────┘  └──────────────┘  └─────────────────────┘   │   │
│  │                                                                  │   │
│  │  ┌──────────────┐  ┌──────────────┐                             │   │
│  │  │ ConfigService│  │ ModelManager │                             │   │
│  │  │              │  │              │                             │   │
│  │  │ - Load/Save  │  │ - Download   │                             │   │
│  │  │ - GetSetting │  │ - GetPath    │                             │   │
│  │  └──────────────┘  └──────────────┘                             │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    PLATFORM ABSTRACTION LAYER                    │   │
│  │                                                                  │   │
│  │  ┌────────────────┐  ┌────────────────┐  ┌──────────────────┐   │   │
│  │  │ IPlatformHotkey│  │ IPlatformAudio │  │ IPlatformClipboard│  │   │
│  │  ├────────────────┤  ├────────────────┤  ├──────────────────┤   │   │
│  │  │ WindowsHotkey  │  │ WindowsAudio   │  │ WindowsClipboard │   │   │
│  │  │ LinuxHotkey    │  │ LinuxAudio     │  │ LinuxClipboard   │   │   │
│  │  │ MacOSHotkey    │  │ MacOSAudio     │  │ MacOSClipboard   │   │   │
│  │  └────────────────┘  └────────────────┘  └──────────────────┘   │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                      EXTERNAL DEPENDENCIES                       │   │
│  │                                                                  │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │   │
│  │  │ Whisper.net │  │ SharpHook   │  │ NAudio/OpenAL           │  │   │
│  │  │ (whisper.cpp)│ │ (lib hooks) │  │ (audio capture)         │  │   │
│  │  └─────────────┘  └─────────────┘  └─────────────────────────┘  │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Component Responsibilities

| Component | Responsibility | Dependencies |
|-----------|---------------|--------------|
| **TrayIcon/MainView** | System tray icon, context menu | ViewModels |
| **SettingsWindow** | Configuration UI | ConfigService |
| **RecordingOverlay** | Visual recording indicator | AppState |
| **HotkeyService** | Global hotkey registration & detection | IPlatformHotkey |
| **AudioService** | Microphone capture, device enumeration | IPlatformAudio |
| **TranscriptionService** | Whisper model loading & inference | Whisper.net, ModelManager |
| **ClipboardService** | Copy text to system clipboard | IPlatformClipboard |
| **OutputService** | Type text into active window | IPlatformOutput |
| **NotificationService** | Toast notifications, sounds | Avalonia |
| **ConfigService** | Load/save user settings | JSON file |
| **ModelManager** | Download & manage Whisper models | HTTP, File system |

---

## 3. Data Flow

### 3.1 Main Transcription Flow

```
┌──────────┐    ┌──────────┐    ┌───────────┐    ┌─────────────┐    ┌──────────┐
│  User    │    │ Hotkey   │    │  Audio    │    │Transcription│    │  Output  │
│ Presses  │───▶│ Service  │───▶│  Service  │───▶│   Service   │───▶│  Service │
│ Hotkey   │    │ (event)  │    │ (capture) │    │  (whisper)  │    │(clipboard│
└──────────┘    └──────────┘    └───────────┘    └─────────────┘    │/autotype)│
                                                                     └──────────┘

Timeline:
─────────────────────────────────────────────────────────────────────────────────▶
  │           │                │                  │                    │
  │ Hotkey    │ Start          │ Stop             │ Transcribe         │ Output
  │ Pressed   │ Recording      │ Recording        │ Audio              │ Text
  │           │                │                  │                    │
  ▼           ▼                ▼                  ▼                    ▼
[IDLE] ──▶ [RECORDING] ──▶ [PROCESSING] ──▶ [IDLE]
```

### 3.2 Detailed Sequence Diagram

```
User          HotkeyService    AudioService    TranscriptionService    OutputService
 │                 │                │                   │                    │
 │  Press Hotkey   │                │                   │                    │
 │────────────────▶│                │                   │                    │
 │                 │  StartCapture()│                   │                    │
 │                 │───────────────▶│                   │                    │
 │                 │                │ ◄─────────────────┤                    │
 │                 │                │  [Recording...]   │                    │
 │                 │                │                   │                    │
 │ Release Hotkey  │                │                   │                    │
 │────────────────▶│                │                   │                    │
 │                 │  StopCapture() │                   │                    │
 │                 │───────────────▶│                   │                    │
 │                 │                │                   │                    │
 │                 │     byte[] audioData               │                    │
 │                 │◄───────────────│                   │                    │
 │                 │                                    │                    │
 │                 │         Transcribe(audioData)      │                    │
 │                 │───────────────────────────────────▶│                    │
 │                 │                                    │                    │
 │                 │              string text           │                    │
 │                 │◄───────────────────────────────────│                    │
 │                 │                                                         │
 │                 │                      Output(text)                       │
 │                 │────────────────────────────────────────────────────────▶│
 │                 │                                                         │
 │◄────────────────────────────────────────────────────────────────────────────
 │                           [Text in clipboard / typed]
```

---

## 4. Application State Machine

### 4.1 States

```
                    ┌─────────────────────────────────────┐
                    │                                     │
                    ▼                                     │
              ┌──────────┐                               │
   ┌─────────▶│   IDLE   │◄─────────────────────┐       │
   │          └────┬─────┘                      │       │
   │               │                            │       │
   │               │ Hotkey Pressed             │       │
   │               ▼                            │       │
   │          ┌──────────┐                      │       │
   │   Error  │RECORDING │  Hotkey Released     │       │
   │   ┌─────▶│          │──────────────────┐   │       │
   │   │      └────┬─────┘                  │   │       │
   │   │           │                        ▼   │       │
   │   │           │ Max Time          ┌────────────┐   │
   │   │           │ Reached           │PROCESSING  │   │
   │   │           └──────────────────▶│            │───┘
   │   │                               └─────┬──────┘
   │   │                                     │
   │   │         Transcription Failed        │ Success
   │   └─────────────────────────────────────┘
   │
   │  App Startup
   │
┌──┴───────┐
│  LOADING │  (Model loading, initialization)
└──────────┘
```

### 4.2 State Definitions

| State | Description | UI Indicator | Actions Allowed |
|-------|-------------|--------------|-----------------|
| **LOADING** | App starting, loading model | Spinner icon | None (wait) |
| **IDLE** | Ready for input | Default tray icon | Start recording |
| **RECORDING** | Capturing audio | Red/pulsing icon | Stop recording |
| **PROCESSING** | Transcribing audio | Spinner icon | None (wait) |
| **ERROR** | Something went wrong | Warning icon | Retry, settings |

### 4.3 State Implementation

```csharp
public enum AppState
{
    Loading,
    Idle,
    Recording,
    Processing,
    Error
}

public interface IAppStateManager
{
    AppState CurrentState { get; }
    event EventHandler<AppState> StateChanged;
    
    void TransitionTo(AppState newState);
    bool CanTransitionTo(AppState newState);
}
```

---

## 5. Service Interfaces

### 5.1 Core Service Interfaces

```csharp
// ============================================
// HOTKEY SERVICE
// ============================================
public interface IHotkeyService : IDisposable
{
    /// <summary>
    /// Fired when the configured hotkey is pressed down
    /// </summary>
    event EventHandler HotkeyPressed;
    
    /// <summary>
    /// Fired when the configured hotkey is released
    /// </summary>
    event EventHandler HotkeyReleased;
    
    /// <summary>
    /// Register a global hotkey combination
    /// </summary>
    /// <param name="modifiers">Ctrl, Shift, Alt, etc.</param>
    /// <param name="key">The main key</param>
    /// <returns>True if registration succeeded</returns>
    bool RegisterHotkey(KeyModifiers modifiers, Key key);
    
    /// <summary>
    /// Unregister the current hotkey
    /// </summary>
    void UnregisterHotkey();
    
    /// <summary>
    /// Check if hotkey is currently registered
    /// </summary>
    bool IsRegistered { get; }
}

[Flags]
public enum KeyModifiers
{
    None = 0,
    Ctrl = 1,
    Shift = 2,
    Alt = 4,
    Meta = 8  // Windows key / Cmd key
}

// ============================================
// AUDIO SERVICE
// ============================================
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Get list of available audio input devices
    /// </summary>
    IReadOnlyList<AudioDevice> GetInputDevices();
    
    /// <summary>
    /// Start capturing audio from the specified device
    /// </summary>
    /// <param name="deviceId">Device ID, or null for default</param>
    void StartCapture(string? deviceId = null);
    
    /// <summary>
    /// Stop capturing and return the recorded audio
    /// </summary>
    /// <returns>Audio data as WAV bytes</returns>
    byte[] StopCapture();
    
    /// <summary>
    /// Whether currently recording
    /// </summary>
    bool IsRecording { get; }
    
    /// <summary>
    /// Current recording duration
    /// </summary>
    TimeSpan RecordingDuration { get; }
    
    /// <summary>
    /// Fired when audio level changes (for visualization)
    /// </summary>
    event EventHandler<float> AudioLevelChanged;
}

public record AudioDevice(string Id, string Name, bool IsDefault);

// ============================================
// TRANSCRIPTION SERVICE
// ============================================
public interface ITranscriptionService : IDisposable
{
    /// <summary>
    /// Load a Whisper model
    /// </summary>
    /// <param name="modelPath">Path to the model file</param>
    Task LoadModelAsync(string modelPath);
    
    /// <summary>
    /// Transcribe audio data to text
    /// </summary>
    /// <param name="audioData">WAV audio bytes</param>
    /// <param name="language">Language code or "auto"</param>
    /// <returns>Transcribed text</returns>
    Task<string> TranscribeAsync(byte[] audioData, string language = "auto");
    
    /// <summary>
    /// Whether a model is currently loaded
    /// </summary>
    bool IsModelLoaded { get; }
    
    /// <summary>
    /// Currently loaded model name
    /// </summary>
    string? LoadedModelName { get; }
}

// ============================================
// CLIPBOARD SERVICE
// ============================================
public interface IClipboardService
{
    /// <summary>
    /// Copy text to the system clipboard
    /// </summary>
    Task SetTextAsync(string text);
    
    /// <summary>
    /// Get text from the system clipboard
    /// </summary>
    Task<string?> GetTextAsync();
}

// ============================================
// OUTPUT SERVICE
// ============================================
public interface IOutputService
{
    /// <summary>
    /// Type text into the currently focused application
    /// </summary>
    /// <param name="text">Text to type</param>
    /// <param name="delayMs">Delay between keystrokes</param>
    Task TypeTextAsync(string text, int delayMs = 10);
}

// ============================================
// NOTIFICATION SERVICE
// ============================================
public interface INotificationService
{
    /// <summary>
    /// Show a toast notification
    /// </summary>
    void ShowNotification(string title, string message, NotificationType type = NotificationType.Info);
    
    /// <summary>
    /// Play a sound effect
    /// </summary>
    void PlaySound(SoundEffect effect);
}

public enum NotificationType { Info, Success, Warning, Error }
public enum SoundEffect { RecordingStart, RecordingStop, Success, Error }

// ============================================
// CONFIG SERVICE
// ============================================
public interface IConfigService
{
    /// <summary>
    /// Current application settings
    /// </summary>
    AppSettings Settings { get; }
    
    /// <summary>
    /// Load settings from disk
    /// </summary>
    Task LoadAsync();
    
    /// <summary>
    /// Save settings to disk
    /// </summary>
    Task SaveAsync();
    
    /// <summary>
    /// Fired when settings change
    /// </summary>
    event EventHandler SettingsChanged;
}

// ============================================
// MODEL MANAGER
// ============================================
public interface IModelManager
{
    /// <summary>
    /// Get list of available models (downloaded + available for download)
    /// </summary>
    Task<IReadOnlyList<WhisperModel>> GetAvailableModelsAsync();
    
    /// <summary>
    /// Download a model
    /// </summary>
    /// <param name="modelName">Model name (tiny, base, small, etc.)</param>
    /// <param name="progress">Progress callback (0.0 to 1.0)</param>
    Task DownloadModelAsync(string modelName, IProgress<double>? progress = null);
    
    /// <summary>
    /// Get path to a downloaded model
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <returns>Full path or null if not downloaded</returns>
    string? GetModelPath(string modelName);
    
    /// <summary>
    /// Check if a model is downloaded
    /// </summary>
    bool IsModelDownloaded(string modelName);
    
    /// <summary>
    /// Delete a downloaded model
    /// </summary>
    Task DeleteModelAsync(string modelName);
}

public record WhisperModel(
    string Name,
    string DisplayName,
    long SizeBytes,
    bool IsDownloaded,
    string? LocalPath
);
```

---

## 6. Data Models

### 6.1 Configuration Model

```csharp
public class AppSettings
{
    // Audio Settings
    public string? SelectedAudioDevice { get; set; }
    
    // Hotkey Settings
    public HotkeySettings Hotkey { get; set; } = new();
    
    // Output Settings
    public OutputSettings Output { get; set; } = new();
    
    // Recognition Settings
    public RecognitionSettings Recognition { get; set; } = new();
    
    // General Settings
    public GeneralSettings General { get; set; } = new();
}

public class HotkeySettings
{
    public KeyModifiers Modifiers { get; set; } = KeyModifiers.Ctrl | KeyModifiers.Shift;
    public string Key { get; set; } = "Space";
    public HotkeyMode Mode { get; set; } = HotkeyMode.PushToTalk;
}

public enum HotkeyMode
{
    PushToTalk,  // Hold to record, release to stop
    Toggle       // Press to start, press again to stop
}

public class OutputSettings
{
    public bool CopyToClipboard { get; set; } = true;
    public bool AutoType { get; set; } = false;
    public bool PlaySoundOnComplete { get; set; } = true;
}

public class RecognitionSettings
{
    public string ModelName { get; set; } = "base";
    public string Language { get; set; } = "auto";
}

public class GeneralSettings
{
    public bool StartWithSystem { get; set; } = false;
    public bool ShowNotifications { get; set; } = true;
    public bool CheckForUpdates { get; set; } = true;
}
```

### 6.2 Configuration File Location

| Platform | Config Path |
|----------|-------------|
| Windows | `%APPDATA%\EchoText\config.json` |
| Linux | `~/.config/echotext/config.json` |
| macOS | `~/Library/Application Support/EchoText/config.json` |

### 6.3 Model Storage Location

| Platform | Models Path |
|----------|-------------|
| Windows | `%APPDATA%\EchoText\models\` |
| Linux | `~/.local/share/echotext/models/` |
| macOS | `~/Library/Application Support/EchoText/models/` |

---

## 7. Platform Abstraction

### 7.1 Platform Detection

```csharp
public static class PlatformInfo
{
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    
    public static string ConfigDirectory => 
        IsWindows ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EchoText") :
        IsMacOS ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "EchoText") :
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "echotext");
        
    public static string ModelsDirectory =>
        IsWindows ? Path.Combine(ConfigDirectory, "models") :
        IsMacOS ? Path.Combine(ConfigDirectory, "models") :
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "echotext", "models");
}
```

### 7.2 Platform Service Registration

```csharp
public static class PlatformServices
{
    public static void Register(IServiceCollection services)
    {
        if (PlatformInfo.IsWindows)
        {
            services.AddSingleton<IPlatformHotkey, WindowsHotkeyProvider>();
            services.AddSingleton<IPlatformAudio, WindowsAudioProvider>();
            services.AddSingleton<IPlatformClipboard, WindowsClipboardProvider>();
            services.AddSingleton<IPlatformOutput, WindowsOutputProvider>();
        }
        else if (PlatformInfo.IsLinux)
        {
            services.AddSingleton<IPlatformHotkey, LinuxHotkeyProvider>();
            services.AddSingleton<IPlatformAudio, LinuxAudioProvider>();
            services.AddSingleton<IPlatformClipboard, LinuxClipboardProvider>();
            services.AddSingleton<IPlatformOutput, LinuxOutputProvider>();
        }
        else if (PlatformInfo.IsMacOS)
        {
            services.AddSingleton<IPlatformHotkey, MacOSHotkeyProvider>();
            services.AddSingleton<IPlatformAudio, MacOSAudioProvider>();
            services.AddSingleton<IPlatformClipboard, MacOSClipboardProvider>();
            services.AddSingleton<IPlatformOutput, MacOSOutputProvider>();
        }
    }
}
```

---

## 8. Project Structure

```
echotext/
├── .github/
│   ├── workflows/
│   │   ├── build.yml                    # CI: Build & test on every push
│   │   └── release.yml                  # CD: Build & release on tags
│   └── ISSUE_TEMPLATE/
│       ├── bug_report.md
│       └── feature_request.md
│
├── src/
│   ├── EchoText/                        # Main application
│   │   ├── EchoText.csproj
│   │   ├── Program.cs                   # Entry point
│   │   ├── App.axaml                    # Avalonia app definition
│   │   ├── App.axaml.cs
│   │   │
│   │   ├── Models/                      # Data models
│   │   │   ├── AppSettings.cs
│   │   │   ├── AppState.cs
│   │   │   ├── AudioDevice.cs
│   │   │   └── WhisperModel.cs
│   │   │
│   │   ├── Services/                    # Business logic
│   │   │   ├── Interfaces/              # Service interfaces
│   │   │   │   ├── IAudioService.cs
│   │   │   │   ├── IClipboardService.cs
│   │   │   │   ├── IConfigService.cs
│   │   │   │   ├── IHotkeyService.cs
│   │   │   │   ├── IModelManager.cs
│   │   │   │   ├── INotificationService.cs
│   │   │   │   ├── IOutputService.cs
│   │   │   │   └── ITranscriptionService.cs
│   │   │   │
│   │   │   ├── AudioService.cs
│   │   │   ├── ClipboardService.cs
│   │   │   ├── ConfigService.cs
│   │   │   ├── HotkeyService.cs
│   │   │   ├── ModelManager.cs
│   │   │   ├── NotificationService.cs
│   │   │   ├── OutputService.cs
│   │   │   ├── TranscriptionService.cs
│   │   │   └── AppStateManager.cs
│   │   │
│   │   ├── Platform/                    # Platform-specific implementations
│   │   │   ├── Interfaces/
│   │   │   │   ├── IPlatformAudio.cs
│   │   │   │   ├── IPlatformClipboard.cs
│   │   │   │   ├── IPlatformHotkey.cs
│   │   │   │   └── IPlatformOutput.cs
│   │   │   │
│   │   │   ├── Windows/
│   │   │   │   ├── WindowsAudioProvider.cs
│   │   │   │   ├── WindowsClipboardProvider.cs
│   │   │   │   ├── WindowsHotkeyProvider.cs
│   │   │   │   └── WindowsOutputProvider.cs
│   │   │   │
│   │   │   ├── Linux/
│   │   │   │   ├── LinuxAudioProvider.cs
│   │   │   │   ├── LinuxClipboardProvider.cs
│   │   │   │   ├── LinuxHotkeyProvider.cs
│   │   │   │   └── LinuxOutputProvider.cs
│   │   │   │
│   │   │   ├── MacOS/
│   │   │   │   ├── MacOSAudioProvider.cs
│   │   │   │   ├── MacOSClipboardProvider.cs
│   │   │   │   ├── MacOSHotkeyProvider.cs
│   │   │   │   └── MacOSOutputProvider.cs
│   │   │   │
│   │   │   ├── PlatformInfo.cs
│   │   │   └── PlatformServices.cs
│   │   │
│   │   ├── ViewModels/                  # MVVM ViewModels
│   │   │   ├── MainViewModel.cs
│   │   │   ├── SettingsViewModel.cs
│   │   │   └── ViewModelBase.cs
│   │   │
│   │   ├── Views/                       # Avalonia views
│   │   │   ├── MainWindow.axaml
│   │   │   ├── MainWindow.axaml.cs
│   │   │   ├── SettingsWindow.axaml
│   │   │   ├── SettingsWindow.axaml.cs
│   │   │   ├── RecordingOverlay.axaml
│   │   │   └── RecordingOverlay.axaml.cs
│   │   │
│   │   └── Assets/                      # Embedded resources
│   │       ├── Icons/
│   │       │   ├── tray-idle.ico
│   │       │   ├── tray-recording.ico
│   │       │   └── tray-processing.ico
│   │       └── Sounds/
│   │           ├── start.wav
│   │           └── stop.wav
│   │
│   └── EchoText.Tests/                  # Unit tests
│       ├── EchoText.Tests.csproj
│       ├── Services/
│       │   ├── AudioServiceTests.cs
│       │   ├── ConfigServiceTests.cs
│       │   └── TranscriptionServiceTests.cs
│       └── ViewModels/
│           └── MainViewModelTests.cs
│
├── docs/
│   ├── REQUIREMENTS.md
│   ├── ARCHITECTURE.md                  # This document
│   └── CONTRIBUTING.md
│
├── assets/
│   └── screenshots/                     # For README
│
├── EchoText.sln                         # Solution file
├── README.md
├── LICENSE
├── CHANGELOG.md
└── .gitignore
```

---

## 9. Dependency Injection Setup

### 9.1 Service Registration

```csharp
// Program.cs
public static void Main(string[] args)
{
    var services = new ServiceCollection();
    
    // Register platform-specific services
    PlatformServices.Register(services);
    
    // Register core services
    services.AddSingleton<IConfigService, ConfigService>();
    services.AddSingleton<IAppStateManager, AppStateManager>();
    services.AddSingleton<IModelManager, ModelManager>();
    services.AddSingleton<IAudioService, AudioService>();
    services.AddSingleton<ITranscriptionService, TranscriptionService>();
    services.AddSingleton<IHotkeyService, HotkeyService>();
    services.AddSingleton<IClipboardService, ClipboardService>();
    services.AddSingleton<IOutputService, OutputService>();
    services.AddSingleton<INotificationService, NotificationService>();
    
    // Register ViewModels
    services.AddSingleton<MainViewModel>();
    services.AddTransient<SettingsViewModel>();
    
    var serviceProvider = services.BuildServiceProvider();
    
    // Start Avalonia app
    BuildAvaloniaApp(serviceProvider).StartWithClassicDesktopLifetime(args);
}
```

---

## 10. Error Handling Strategy

### 10.1 Error Categories

| Category | Example | Handling |
|----------|---------|----------|
| **Recoverable** | Mic temporarily unavailable | Retry, notify user |
| **Configuration** | Invalid settings | Reset to defaults, notify |
| **Model** | Model file corrupted | Re-download, notify |
| **Fatal** | Out of memory | Log, show error, exit gracefully |

### 10.2 Error Handling Pattern

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    
    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
}

// Usage example
public async Task<Result<string>> TranscribeAsync(byte[] audio)
{
    try
    {
        if (!IsModelLoaded)
            return Result<string>.Failure("No model loaded");
            
        var text = await _whisper.ProcessAsync(audio);
        return Result<string>.Success(text);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Transcription failed");
        return Result<string>.Failure($"Transcription failed: {ex.Message}");
    }
}
```

---

## 11. Build & Distribution

### 11.1 Build Commands

```bash
# Development build
dotnet build

# Release build (current platform)
dotnet publish -c Release

# Platform-specific release builds
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
dotnet publish -c Release -r osx-arm64 --self-contained
```

### 11.2 GitHub Actions Matrix

```yaml
strategy:
  matrix:
    include:
      - os: windows-latest
        rid: win-x64
        artifact: EchoText-win-x64.zip
      - os: ubuntu-latest
        rid: linux-x64
        artifact: EchoText-linux-x64.tar.gz
      - os: macos-latest
        rid: osx-x64
        artifact: EchoText-osx-x64.dmg
      - os: macos-latest
        rid: osx-arm64
        artifact: EchoText-osx-arm64.dmg
```

---

## 12. Testing Strategy

### 12.1 Test Categories

| Type | Scope | Tools |
|------|-------|-------|
| **Unit Tests** | Individual services | xUnit, Moq |
| **Integration Tests** | Service interactions | xUnit |
| **UI Tests** | View interactions | Avalonia.Headless |

### 12.2 Mocking Platform Services

```csharp
// Example test
public class MainViewModelTests
{
    [Fact]
    public async Task StartRecording_ShouldChangeStateToRecording()
    {
        // Arrange
        var mockAudio = new Mock<IAudioService>();
        var mockState = new Mock<IAppStateManager>();
        var vm = new MainViewModel(mockAudio.Object, mockState.Object, ...);
        
        // Act
        vm.StartRecordingCommand.Execute(null);
        
        // Assert
        mockState.Verify(s => s.TransitionTo(AppState.Recording), Times.Once);
        mockAudio.Verify(a => a.StartCapture(It.IsAny<string>()), Times.Once);
    }
}
```

---

## 13. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-01-28 | Claude | Initial architecture |

---

## Appendix A: NuGet Packages

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
  
  <!-- Audio (Windows) -->
  <PackageReference Include="NAudio" Version="2.2.1" Condition="$([MSBuild]::IsOSPlatform('Windows'))" />
  
  <!-- Global Hotkeys -->
  <PackageReference Include="SharpHook" Version="5.3.1" />
  
  <!-- JSON Configuration -->
  <PackageReference Include="System.Text.Json" Version="8.0.0" />
  
  <!-- HTTP (for model downloads) -->
  <PackageReference Include="System.Net.Http" Version="4.3.4" />
  
  <!-- Logging -->
  <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
  <PackageReference Include="Serilog.Extensions.Logging.File" Version="3.0.0" />
</ItemGroup>

<!-- Test Project -->
<ItemGroup>
  <PackageReference Include="xunit" Version="2.7.0" />
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
</ItemGroup>
```

---

## Appendix B: Audio Format Specification

Whisper expects audio in the following format:

| Property | Value |
|----------|-------|
| Format | WAV (PCM) |
| Sample Rate | 16000 Hz |
| Channels | Mono (1) |
| Bit Depth | 16-bit |

```csharp
public static class AudioConstants
{
    public const int SampleRate = 16000;
    public const int Channels = 1;
    public const int BitsPerSample = 16;
    public const int MaxRecordingSeconds = 120;
}
```
