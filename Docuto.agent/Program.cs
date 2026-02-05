using AICrawler.Agent;
using Docuto.Agent.AI;
using Docuto.Agent.Service;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false) // main config
    .AddJsonFile("appsettings.local.json", optional: true) // optional local override
    .AddCommandLine(args) // CLI args override everything
    .Build();

var projectFolder = config["projectPath"] 
                    ?? throw new Exception("projectPath missing in config");

var key = config["openAiKey"]
          ?? throw new Exception("openAiKey missing in config");

var model = config["model"];

var agent = new Agent(key, model);
var manifest = new ManifestService(projectFolder);

var dirInfo = new DirectoryInfo(projectFolder);

//TODO: Improve performance. Perhaps by using the same technique as in GetProjectFilesToProcessAsync
var dirInfoFiles = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories);

// first determine if projectRoot even exists, is accessible, is a directory and not empty
if (!dirInfo.Exists)
{
    throw new DirectoryNotFoundException($"Project folder '{projectFolder}' does not exist");
}

if (!dirInfoFiles.Any())
{
    throw new Exception($"Project folder '{projectFolder}' is empty");
}

var filesToProcess = await GetProjectFilesToProcessAsync(projectFolder, agent);

foreach (var relativePath in filesToProcess)
{ 
    var fullPath = Path.Combine(projectFolder, relativePath);
    
    if (!manifest.IsChanged(relativePath, fullPath))
    {
        Console.WriteLine($"[Skipping] Unchanged: {relativePath}");
        continue;
    }

    Console.WriteLine($"[Processing] Generating docs for: {relativePath}");
    var doc = await agent.GenerateDocumentation(fullPath);

    await DocumentationService.SaveAsync(doc, projectFolder);
    manifest.Update(relativePath, fullPath);
}

manifest.Save();

return;

async Task<HashSet<string>> GetProjectFilesToProcessAsync(string root, Agent docAgent)
{
    var ignore = ReadDocUtilityFile(Path.Combine(root, ".docignore"));
    var whitelist = ReadDocUtilityFile(Path.Combine(root, ".docwhitelist"));
    HashSet<string> files;

    // whitelist overrides everything
    if (whitelist is { Count: > 0 })
    {
        files = whitelist;
    }
    // apply ignore
    else if (ignore is { Count: > 0 })
    {
        files = Directory
            .EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path))
            .Where(path => !IsIgnored(path, ignore))
            .ToHashSet();
    }
    else
    {
        var excludeFolders = new HashSet<string>
        {
            "bin","obj",".git",".vs","node_modules","vendor",
            "packages","dist","build","out",".idea",".vscode",
            "storage","cache"
        };

        files = Directory
            .EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(path => !IsInExcludedFolder(path, excludeFolders))
            .Select(path => Path.GetRelativePath(root, path))
            .ToHashSet();
    }
    
    // let AI decide ones actually matter
    return await docAgent.DecideFilesToProcess(files);
}

static bool IsInExcludedFolder(string path, HashSet<string> excluded) 
{
    var segments = path.Split(Path.DirectorySeparatorChar);
    return segments.Any(excluded.Contains);
}

HashSet<string>? ReadDocUtilityFile(string path)
{
    if (!File.Exists(path))
    {
        return null;
    }

    return File.ReadLines(path)
        .Select(l => l.Trim())
        .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#"))
        .ToHashSet();
}

static bool IsIgnored(string path, HashSet<string> ignoreRules)
{
    foreach (var rule in ignoreRules)
    {
        // folder rule
        if (rule.EndsWith("/"))
        {
            if (path.StartsWith(rule, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        // simple glob rule
        else if (rule.StartsWith("*."))
        {
            if (Path.GetExtension(path).Equals(rule[1..], StringComparison.OrdinalIgnoreCase))
                return true;
        }
        // exact file
        else if (path.Equals(rule, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
    }

    return false;
}
