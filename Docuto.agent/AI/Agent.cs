using System.Text.Json;
using AICrawler.Agent;
using Docuto.Agent.Models;
using OpenAI.Chat;

namespace Docuto.Agent.AI;

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

    public Agent(string key, string? model = null)
    {
        var llm = new LlmManager(key, model);
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
        //var raw = "[ \"README.md\", \"composer.json\", \"config/app.php\", \"config/database.php\", \"routes/web.php\", \"app/Providers/AppServiceProvider.php\", \"app/Http/Controllers/IndexController.php\", \"app/Http/Controllers/UrlController.php\", \"app/Http/Controllers/ShopController.php\", \"app/Http/Controllers/StatisticController.php\", \"app/Models/User.php\", \"app/Models/Shop.php\", \"app/Models/Url.php\", \"app/Models/Statistic.php\", \"app/Jobs/DetermineStatisticsCountries.php\" ]";

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
