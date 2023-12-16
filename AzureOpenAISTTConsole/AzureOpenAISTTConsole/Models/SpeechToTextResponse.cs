using System.Text.Json.Serialization;

namespace AzureOpenAISTTConsole.Models;

/// <summary>
///     Represents a response of the transcription
///     request.
/// </summary>
/// <param name="Text"> The transcribed text of the audio file. </param>
internal record SpeechToTextResponse(
    [property: JsonPropertyName("text")] string? Text);
