# Contributing to EchoText

Thank you for your interest in contributing to EchoText! This document provides guidelines and instructions for contributing.

## Code of Conduct

This project adheres to a code of conduct that all contributors are expected to follow:

- **Be respectful**: Treat everyone with respect and kindness
- **Be constructive**: Provide helpful feedback and suggestions
- **Be inclusive**: Welcome newcomers and diverse perspectives
- **Be professional**: Keep discussions focused and on-topic

Please report unacceptable behavior to the project maintainers.

## How to Report Bugs

If you find a bug in EchoText, please help us fix it by following these steps:

1. **Check existing issues**: Search the [issue tracker](https://github.com/cyrilth/echotext/issues) to see if the bug has already been reported
2. **Use the bug report template**: Create a new issue using the bug report template
3. **Provide details**:
   - Clear description of the bug
   - Steps to reproduce the issue
   - Expected behavior vs actual behavior
   - Your operating system and version
   - EchoText version
   - Log files (located in your platform's log directory)
   - Screenshots or recordings if applicable

## How to Suggest Features

We welcome feature suggestions! To suggest a new feature:

1. **Check existing requests**: Search the [issue tracker](https://github.com/cyrilth/echotext/issues) to see if the feature has been requested
2. **Use the feature request template**: Create a new issue using the feature request template
3. **Describe clearly**:
   - What problem does this feature solve?
   - How should it work?
   - Are there any alternatives you've considered?
   - Would you be willing to implement it?

## Pull Request Process

### Before You Start

1. **Discuss major changes**: For significant changes, please open an issue first to discuss your approach
2. **Check the documentation**: Review the [ARCHITECTURE.md](docs/ARCHITECTURE.md) and [REQUIREMENTS.md](docs/REQUIREMENTS.md) documents
3. **Review coding conventions**: See the [CLAUDE.md](CLAUDE.md) file for coding standards

### Development Setup

1. **Prerequisites**:
   - .NET 8 SDK
   - Git
   - A code editor (Visual Studio, VS Code, or Rider)

2. **Clone the repository**:
   ```bash
   git clone https://github.com/cyrilth/echotext.git
   cd echotext
   ```

3. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

4. **Build the project**:
   ```bash
   dotnet build
   ```

5. **Run tests**:
   ```bash
   dotnet test
   ```

6. **Run the application**:
   ```bash
   dotnet run --project src/EchoText
   ```

### Making Changes

1. **Create a branch**:
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   ```

2. **Make your changes**:
   - Follow the coding conventions (see below)
   - Write or update tests as needed
   - Update documentation if applicable

3. **Test your changes**:
   ```bash
   dotnet build
   dotnet test
   ```

4. **Commit your changes**:
   ```bash
   git add .
   git commit -m "Description of your changes"
   ```

   Use clear, descriptive commit messages:
   - `feat: Add voice activity detection`
   - `fix: Resolve hotkey conflict on Linux`
   - `docs: Update installation instructions`
   - `refactor: Simplify audio capture logic`

5. **Push your branch**:
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Open a Pull Request**:
   - Go to the repository on GitHub
   - Click "New Pull Request"
   - Select your branch
   - Fill out the PR template with:
     - Description of changes
     - Related issue numbers
     - Testing performed
     - Screenshots (if UI changes)

### Pull Request Review

- **Respond to feedback**: Maintainers may request changes
- **Keep it focused**: One feature or fix per PR
- **Update your branch**: Rebase or merge from main if needed
- **Be patient**: Reviews may take a few days

## Coding Conventions

### General Style

- **Language**: C# 12
- **Framework**: .NET 8
- **Formatting**: Follow Microsoft C# coding conventions
- **Naming**:
  - Interfaces: `IServiceName` (e.g., `IAudioService`)
  - Services: `ServiceName` (e.g., `AudioService`)
  - Platform implementations: `{Platform}{Interface}` (e.g., `WindowsAudioProvider`)

### Architecture Rules

1. **All services behind interfaces**: For testability and dependency injection
2. **Platform code isolated**: Place OS-specific code in `Platform/{OS}/` folders only
3. **MVVM separation**: ViewModels should not reference Views
4. **Error handling**: Use `Result<T>` pattern for expected failures, not exceptions
5. **Event-driven**: Use events for cross-service communication

### File Organization

- One class per file (except for small related types)
- Place interfaces in `Services/Interfaces/` or `Platform/Interfaces/`
- Group by feature, not by type

### Documentation

- Add XML documentation comments to all public APIs
- Update relevant documentation files for significant changes
- Include code comments for complex logic

### Testing

- Write unit tests for new services
- Aim for >80% code coverage
- Use xUnit, Moq, and FluentAssertions
- Mock platform-specific dependencies

## Project Structure

```
echotext/
├── src/
│   ├── EchoText/              # Main application
│   │   ├── Models/            # Data models
│   │   ├── Services/          # Business logic
│   │   │   └── Interfaces/    # Service contracts
│   │   ├── Platform/          # OS-specific code
│   │   ├── ViewModels/        # MVVM ViewModels
│   │   ├── Views/             # Avalonia XAML views
│   │   └── Assets/            # Icons, sounds
│   └── EchoText.Tests/        # Unit tests
├── docs/                      # Documentation
└── .github/                   # GitHub workflows & templates
```

## Building for Release

### Windows
```bash
dotnet publish src/EchoText -c Release -r win-x64 --self-contained
```

### Linux
```bash
dotnet publish src/EchoText -c Release -r linux-x64 --self-contained
```

### macOS (Intel)
```bash
dotnet publish src/EchoText -c Release -r osx-x64 --self-contained
```

### macOS (ARM)
```bash
dotnet publish src/EchoText -c Release -r osx-arm64 --self-contained
```

## Getting Help

- **Documentation**: Check the [docs/](docs/) folder
- **Issues**: Search or create an issue on GitHub
- **Discussions**: Use GitHub Discussions for questions

## License

By contributing to EchoText, you agree that your contributions will be licensed under the MIT License.

## Recognition

All contributors will be recognized in the project's README and release notes. Thank you for helping make EchoText better!
