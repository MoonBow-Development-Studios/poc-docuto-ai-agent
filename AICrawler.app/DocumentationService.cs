namespace AICrawler.app;

public static class DocumentationService
{
    public static async Task SaveAsync(DocumentationResult doc, string rootFolder)
    {
        var path = Path.Combine(rootFolder, "documentation", doc.DocumentationPath);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await File.WriteAllTextAsync(path, doc.Content);
    }
}