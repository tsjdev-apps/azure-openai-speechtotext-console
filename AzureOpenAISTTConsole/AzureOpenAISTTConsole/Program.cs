using AzureOpenAISTTConsole.Models;
using Spectre.Console;
using System.Net.Http.Json;

bool isRunning = true;

// Clear the console
AnsiConsole.Clear();

// Create header
CreateHeader();

// Get Endpoint
string endpoint = GetEndpoint();

if (endpoint.EndsWith('/'))
{
    endpoint = endpoint.TrimEnd('/');
}

// Get Api Key
string apiKey = GetApiKey();

// Get Deployment name
string deploymentName = GetWhisperModelDeploymentName();

// Create Speech-To-Text endpoint
string speechToTextEndoint = $"{endpoint}/openai/deployments/{deploymentName}/audio/transcriptions?api-version=2023-09-01-preview";


while (isRunning)
{
    AnsiConsole.Clear();
    CreateHeader();

    // Get file path
    string filePath = GetFilePath();

    AnsiConsole.Clear();
    CreateHeader();

    // LOGIC
    await AnsiConsole.Status()
        .StartAsync("Transcribing your MP3 file...", async ctx =>
        {

            try
            {
                byte[] soundBytes = await File.ReadAllBytesAsync(filePath);

                HttpClient client = new();

                client.DefaultRequestHeaders.Add("api-key", apiKey);

                MultipartFormDataContent content = new()
                        {
                            { new ByteArrayContent(soundBytes), "file", Path.GetFileName(filePath) },
                            { new StringContent("whisper-1"), "model" }
                        };

                HttpResponseMessage response = await client.PostAsync(speechToTextEndoint, content);
                SpeechToTextResponse? speechToTextResponse = await response.Content.ReadFromJsonAsync<SpeechToTextResponse>();

                if (!string.IsNullOrEmpty(speechToTextResponse?.Text))
                {
                    AnsiConsole.MarkupLine($"This is your transcribed text: [yellow]{speechToTextResponse?.Text}[/]");
                    AnsiConsole.WriteLine();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"Something went wrong: [red]{ex.Message}[/]");
            }
        });

    AnsiConsole.WriteLine();
    isRunning = AnsiConsole.Confirm("Do you want to transcribe another MP3 file?", false);
}


/// <summary>
///     Creates the header for the console application.
/// </summary>
static void CreateHeader()
{
    // Create a grid for the header text
    Grid grid = new();
    grid.AddColumn();
    grid.AddRow(new FigletText("Speech-To-Text").Centered().Color(Color.Red));
    grid.AddRow(Align.Center(new Panel("[red]Sample by Thomas Sebastian Jensen ([link]https://www.tsjdev-apps.de[/])[/]")));

    // Write the grid to the console
    AnsiConsole.Write(grid);
    AnsiConsole.WriteLine();
}


/// <summary>
///     Prompts the user for their Azure OpenAI endpoint.
/// </summary>
/// <returns>The user's Azure OpenAI endpoint.</returns>
static string GetEndpoint()
    => AnsiConsole.Prompt(
        new TextPrompt<string>("Please insert your full [yellow]Azure OpenAI endpoint[/]:")
        .PromptStyle("white")
        .ValidationErrorMessage("[red]Invalid prompt[/]")
        .Validate(prompt =>
        {
            if (prompt.Length < 3)
            {
                return ValidationResult.Error("[red]Endpoint too short[/]");
            }

            if (prompt.Length > 200)
            {
                return ValidationResult.Error("[red]Endpoint too long[/]");
            }

            if (!prompt.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                return ValidationResult.Error("[red]Not a valid url[/]");
            }

            if (!prompt.Contains("openai.azure.com", StringComparison.InvariantCultureIgnoreCase))
            {
                return ValidationResult.Error("[red]Not a valid url[/]");
            }

            return ValidationResult.Success();
        }));


/// <summary>
///     Prompts the user for their Azure OpenAI API key.
/// </summary>
/// <returns>The user's Azure OpenAI API key.</returns>
static string GetApiKey()
    => AnsiConsole.Prompt(
        new TextPrompt<string>("Please insert your [yellow]Azure OpenAI API key[/]:")
        .PromptStyle("white")
        .ValidationErrorMessage("[red]Invalid prompt[/]")
        .Validate(prompt =>
        {
            if (prompt.Length < 3)
            {
                return ValidationResult.Error("[red]API key too short[/]");
            }

            if (prompt.Length > 200)
            {
                return ValidationResult.Error("[red]API key too long[/]");
            }

            return ValidationResult.Success();
        }));

/// <summary>
///     Prompts the user for their Azure OpenAI Whisper Model Deployment name.
/// </summary>
/// <returns>The user's Azure OpenAI Whisper Model Deployment name.</returns>
static string GetWhisperModelDeploymentName()
    => AnsiConsole.Prompt(
        new TextPrompt<string>("Please insert your [yellow]Azure Whisper model deployment name[/]:")
        .PromptStyle("white")
        .ValidationErrorMessage("[red]Invalid prompt[/]")
        .Validate(prompt =>
        {
            if (prompt.Length > 200)
            {
                return ValidationResult.Error("[red]Model deployment name too long[/]");
            }

            return ValidationResult.Success();
        }));

/// <summary>
///     Prompts the user for the file path represents the audio file.
/// </summary>
/// <returns>The user's selected file path for the audio file.</returns>
static string GetFilePath()
    => AnsiConsole.Prompt(
        new TextPrompt<string>("Please insert the [yellow]path[/] for the MP3 file:")
        .PromptStyle("white")
        .ValidationErrorMessage("[red]Invalid input[/]")
        .Validate(prompt =>
        {
            if (prompt.Length < 3)
            {
                return ValidationResult.Error("[red]File path too short[/]");
            }

            if (prompt.Length > 256)
            {
                return ValidationResult.Error("[red]File path too long[/]");
            }

            if (!prompt.EndsWith("mp3", StringComparison.InvariantCultureIgnoreCase))
            {
                return ValidationResult.Error("[red]Not a MP3 file[/]");
            }

            return ValidationResult.Success();
        }));