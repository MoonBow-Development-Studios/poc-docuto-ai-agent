namespace AICrawler.Agent;

public static class DocumentationService
{
    public static async Task SaveAsync(DocumentationResult doc, string rootFolder)
    {
        var path = Path.Combine(rootFolder, doc.DocumentationPath);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await File.WriteAllTextAsync(path, doc.Content);
    }
}
