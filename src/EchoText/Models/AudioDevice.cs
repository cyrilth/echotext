namespace EchoText.Models;

/// <summary>
/// Represents an audio input device
/// </summary>
/// <param name="Id">Device identifier</param>
/// <param name="Name">Human-readable device name</param>
/// <param name="IsDefault">Whether this is the system's default device</param>
public record AudioDevice(string Id, string Name, bool IsDefault);
