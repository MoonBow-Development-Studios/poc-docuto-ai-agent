using System.Text.Json;
using AICrawler.app;
using OpenAI.Chat;

namespace AICrawler.App;

public interface IAgent
{
    Task<HashSet<string>> DecideFilesToProcess(HashSet<string> filePaths);
    Task<DocumentationResult> GenerateDocumentation(string filePath);
}

public class Agent : IAgent
{
    private readonly ChatClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Agent(string key)
    {
        var llm = new LlmManager(key);
        _client = llm.GetChatClient();
    }

    public async Task<HashSet<string>> DecideFilesToProcess(HashSet<string> filePaths)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(Prompts.FileListPrompt),
            ChatMessage.CreateUserMessage(string.Join('\n', filePaths))
        };

        var result = await _client.CompleteChatAsync(messages);

        var raw = CleanJson(result.Value.Content[0].Text);

        return JsonSerializer.Deserialize<HashSet<string>>(raw, JsonOptions)
               ?? [];
    }

    public async Task<DocumentationResult> GenerateDocumentation(string filePath)
    {
        var fileContent = await File.ReadAllTextAsync(filePath);
        var ext = Path.GetExtension(filePath).TrimStart('.');

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(Prompts.DocumentationPrompt),
            ChatMessage.CreateUserMessage($"```{ext}\n{fileContent}\n```")
        };

        var result = await _client.CompleteChatAsync(messages);

        var raw = CleanJson(result.Value.Content[0].Text);

        return JsonSerializer.Deserialize<DocumentationResult>(raw, JsonOptions)
               ?? throw new InvalidOperationException("Invalid documentation JSON returned by LLM");
    }

    private static string CleanJson(string text)
    {
        // remove ```json fences if model adds them
        return text
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();
    }
}