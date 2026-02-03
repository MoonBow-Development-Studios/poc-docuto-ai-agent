using AICrawler.app;
using Microsoft.Extensions.Configuration;

var projectFolder = "/Users/timslager/git/ShortUrl";

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);

var config = builder.Build();

var key = config["openAi:key"];

var _agent = new Agent(key);

var excludeFolders = new List<string>
{
    "bin",
    "obj",
    ".git",
    ".vs",
    "node_modules",
    "vendor",
    "packages",
    "node_modules",
    "dist",
    "build",
    "out",
    ".idea",
    ".vscode",
    "storage",
    "cache"
};

// read project folder
var projectFiles = Directory.GetFiles(projectFolder, "*.*", SearchOption.AllDirectories)
    .Where(filePath => !excludeFolders.Any(excludeFolder => filePath.Split(Path.DirectorySeparatorChar).Contains(excludeFolder)))
    .ToList();

var folders = projectFiles.Select(
    projectFile => projectFile.Replace(
        projectFolder + Path.DirectorySeparatorChar, "")
    )
    .ToList();

var result = await _agent.DecideFilesToProcess(folders);

foreach (var file in result)
{
    var actualFilePath = Path.Combine(projectFolder, file);
    Console.WriteLine(actualFilePath);
    var documentation = await _agent.GenerateDocumentation(actualFilePath);
    
    if (documentation != null)
    {
        await _agent.SaveGeneratedDocumentation(documentation);
    }
}