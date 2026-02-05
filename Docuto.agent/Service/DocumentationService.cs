using AICrawler.Agent;
using Docuto.Agent.Models;

namespace Docuto.Agent.Service;

public static class DocumentationService
{
    public static async Task SaveAsync(DocumentationResult doc, string rootFolder)
    {
        var path = Path.Combine(rootFolder, doc.DocumentationPath);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await File.WriteAllTextAsync(path, doc.Content);
    }
}
