# EchoText

A cross-platform (Windows, Linux, macOS) speech-to-text desktop application that runs locally using OpenAI's Whisper model. Your voice echoes back as text.

## Documentation

Detailed documentation is in the `/docs` folder. **Read these before starting work:**

| Document | Purpose |
|----------|---------|
| `docs/REQUIREMENTS.md` | Features, constraints, decisions |
| `docs/ARCHITECTURE.md` | Components, interfaces, patterns |
| `docs/TASKS.md` | Development tasks with acceptance criteria |
| `docs/CLAUDE_CODE_GUIDE.md` | Prompting guide and best practices |

## Tech Stack

- **Language:** C# / .NET 8
- **UI Framework:** Avalonia UI 11
- **Speech Recognition:** Whisper.net (CPU-only)
- **Global Hotkeys:** SharpHook
- **Audio Capture:** NAudio
- **Pattern:** MVVM with Dependency Injection

## Project Structure

```
echotext/
├── src/
│   ├── EchoText/              # Main application
│   │   ├── Models/            # Data models
│   │   ├── Services/          # Business logic
│   │   │   └── Interfaces/    # Service contracts
│   │   ├── Platform/          # OS-specific code
│   │   │   ├── Windows/
│   │   │   ├── Linux/
│   │   │   └── MacOS/
│   │   ├── ViewModels/        # MVVM ViewModels
│   │   ├── Views/             # Avalonia XAML views
│   │   └── Assets/            # Icons, sounds
│   └── EchoText.Tests/        # Unit tests
├── docs/                      # Documentation
├── CLAUDE.md                  # This file
└── EchoText.sln              # Solution file
```

## Common Commands

```bash
# Build
dotnet build

# Run
dotnet run --project src/EchoText

# Test
dotnet test

# Publish (Windows)
dotnet publish src/EchoText -c Release -r win-x64 --self-contained

# Publish (Linux)
dotnet publish src/EchoText -c Release -r linux-x64 --self-contained

# Publish (macOS Intel)
dotnet publish src/EchoText -c Release -r osx-x64 --self-contained

# Publish (macOS ARM)
dotnet publish src/EchoText -c Release -r osx-arm64 --self-contained
```

## Coding Conventions

### General
- Use C# 12 features where appropriate
- Follow Microsoft C# coding conventions
- Use `var` when type is obvious
- Use file-scoped namespaces

### Naming
- **Interfaces:** `IServiceName` (e.g., `IAudioService`)
- **Services:** `ServiceName` (e.g., `AudioService`)
- **Platform implementations:** `{Platform}{Interface}` (e.g., `WindowsAudioProvider`)
- **ViewModels:** `{Name}ViewModel` (e.g., `MainViewModel`)
- **Views:** `{Name}Window` or `{Name}View` (e.g., `SettingsWindow`)

### Architecture Rules
1. **All services behind interfaces** - For testability and DI
2. **Platform code isolated** - In `Platform/{OS}/` folders only
3. **ViewModels don't reference Views** - MVVM separation
4. **Use Result<T> for errors** - Not exceptions for expected failures
5. **Events for cross-service communication** - Loose coupling

### File Organization
- One class per file (except small related types)
- Interfaces in `Services/Interfaces/` or `Platform/Interfaces/`
- Group by feature, not by type

## Current Development State

Check `docs/TASKS.md` for:
- Task status (⬜ Not Started, 🔄 In Progress, ✅ Complete)
- Current phase
- Task dependencies

## Task Workflow

1. **Start task:** Read acceptance criteria in `docs/TASKS.md`
2. **Reference:** Check `docs/ARCHITECTURE.md` for interfaces/patterns
3. **Implement:** Follow coding conventions above
4. **Verify:** Run `dotnet build` and `dotnet test`
5. **Complete:** Ensure all acceptance criteria met

## Key Interfaces

When implementing services, follow these interfaces exactly as defined in `docs/ARCHITECTURE.md` section 5:

- `IHotkeyService` - Global hotkey registration
- `IAudioService` - Microphone capture
- `ITranscriptionService` - Whisper transcription
- `IClipboardService` - System clipboard
- `IOutputService` - Auto-type text
- `IConfigService` - Settings persistence
- `INotificationService` - Toasts and sounds
- `IModelManager` - Whisper model downloads
- `IAppStateManager` - Application state machine

## Platform Abstraction

For OS-specific code:

1. Define interface in `Platform/Interfaces/`
2. Implement in `Platform/{OS}/`
3. Register in `Platform/PlatformServices.cs`

Example:
```csharp
// Platform/Interfaces/IPlatformClipboard.cs
public interface IPlatformClipboard { ... }

// Platform/Windows/WindowsClipboardProvider.cs
public class WindowsClipboardProvider : IPlatformClipboard { ... }

// Platform/Linux/LinuxClipboardProvider.cs
public class LinuxClipboardProvider : IPlatformClipboard { ... }
```

## Error Handling

Use `Result<T>` pattern for expected failures:

```csharp
public async Task<Result<string>> TranscribeAsync(byte[] audio)
{
    if (!IsModelLoaded)
        return Result<string>.Failure("No model loaded");
    
    var text = await _whisper.ProcessAsync(audio);
    return Result<string>.Success(text);
}
```

## Audio Format

Whisper requires specific format:
- **Sample Rate:** 16000 Hz
- **Channels:** Mono (1)
- **Bit Depth:** 16-bit
- **Format:** WAV/PCM

## Don't Forget

- ✅ Run `dotnet build` after changes
- ✅ Check acceptance criteria in TASKS.md
- ✅ Follow interfaces in ARCHITECTURE.md
- ✅ Keep platform code isolated
- ✅ Use dependency injection
- ❌ Don't hardcode OS-specific code in services
- ❌ Don't skip unit tests for services
- ❌ Don't deviate from documented interfaces
