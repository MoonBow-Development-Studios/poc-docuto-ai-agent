using System.Text.Json;
using OpenAI.Chat;

namespace AICrawler.app;

public interface IAgent
{
    Task<List<string>> DecideFilesToProcess(List<string> filePaths);
}

public class Agent: IAgent
{
    private readonly LlmManager _llm;
    
    public Agent(string key)
    {
        _llm = new LlmManager(key);
    }
    public async Task<List<string>> DecideFilesToProcess(List<string> filePaths)
    {
        try
        {
            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(Prompts.FileListPrompt),
                ChatMessage.CreateUserMessage(string.Join("\n", filePaths))
            };

            var client = _llm.GetChatClient();

            //var result = await client.CompleteChatAsync(messages);
            var content =
                "[ \"app/Http/Controllers/UrlController.php\", \"bootstrap/app.php\", \"config/app.php\", \"routes/web.php\", \"app/Http/Controllers/IndexController.php\", \"app/Http/Controllers/ShopController.php\", \"app/Http/Controllers/StatisticController.php\", \"app/Models/Url.php\", \"app/Models/Shop.php\", \"app/Models/Statistic.php\", \"app/Traits/UtilTrait.php\", \"app/Jobs/DetermineStatisticsCountries.php\", \"resources/views/components/layout.blade.php\", \"resources/views/dashboard.blade.php\", \"resources/views/url/create.blade.php\", \"resources/views/url/edit.blade.php\", \"resources/views/shop/index.blade.php\", \"resources/views/shop/create.blade.php\", \"resources/views/shop/edit.blade.php\", \"resources/views/statistics/show.blade.php\", \"database/migrations/2025_09_04_071804_create_urls_table.php\", \"database/migrations/2025_09_09_102952_create_shops_table.php\", \"database/migrations/2025_09_04_071820_create_statistics_table.php\" ]";

            var files = JsonSerializer.Deserialize<List<string>>(content);
            
            if (files == null || files.Count == 0)
            {
                return [];
            }

            return files;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return [];
        }
    }

    public async Task<DocumentationResult?> GenerateDocumentation(string filePath)
    {
        try
        {
            // read file content
            var fileContent = await File.ReadAllTextAsync(filePath);
            var codeExtension = Path.GetExtension(filePath);

            var formattedFileContent = $"```{codeExtension}\n{fileContent}\n```";

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(Prompts.DocumentationPrompt),
                ChatMessage.CreateUserMessage(formattedFileContent)
            };

            var client = _llm.GetChatClient();
            var result = await client.CompleteChatAsync(messages);
            var content = result.Value.Content[0].Text;

            var dResult = JsonSerializer.Deserialize<DocumentationResult>(content);

            return dResult ?? throw new Exception("Failed to deserialize documentation result");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<bool> SaveGeneratedDocumentation(DocumentationResult documentation)
    {
        try
        {
            // save it in /documentation/ in current project folder
            var documentationFolder = Path.Combine(Directory.GetCurrentDirectory(), "documentation");
            if (!Directory.Exists(documentationFolder))
            {
                Directory.CreateDirectory(documentationFolder);
            }
            
            var documentationPath = Path.Combine(documentationFolder, documentation.DocumentationPath);
            var documentationDir = Path.GetDirectoryName(documentationPath);
            if (documentationDir != null && !Directory.Exists(documentationDir))
            {
                Directory.CreateDirectory(documentationDir);
            }
            
            await File.WriteAllTextAsync(documentationPath, documentation.Content);
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}