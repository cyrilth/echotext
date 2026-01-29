using System.Text.Json.Serialization;

namespace EchoText.Models;

/// <summary>
/// JSON source generator context for AppSettings.
/// Required for AOT/trimmed builds where reflection is not available.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(AppSettings))]
public partial class AppSettingsJsonContext : JsonSerializerContext
{
}
