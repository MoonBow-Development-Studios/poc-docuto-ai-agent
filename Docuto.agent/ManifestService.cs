using System.Security.Cryptography;
using System.Text.Json;

namespace AICrawler.Agent;

public class ManifestService
{
    private readonly string _manifestPath;
    private Dictionary<string, string> _manifest;

    public ManifestService(string projectRoot)
    {
        var docsDir = Path.Combine(projectRoot, "docs");
        if (!Directory.Exists(docsDir))
        {
            Directory.CreateDirectory(docsDir);
        }
        
        _manifestPath = Path.Combine(docsDir, ".docuto-manifest.json");
        _manifest = LoadManifest();
    }

    public bool IsChanged(string relativePath, string fullPath)
    {
        var currentHash = ComputeHash(fullPath);
        if (_manifest.TryGetValue(relativePath, out var storedHash) && storedHash == currentHash)
        {
            return false;
        }
        return true;
    }

    public void Update(string relativePath, string fullPath)
    {
        _manifest[relativePath] = ComputeHash(fullPath);
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_manifestPath, json);
    }

    private Dictionary<string, string> LoadManifest()
    {
        if (!File.Exists(_manifestPath)) return new Dictionary<string, string>();
        try 
        {
            var json = File.ReadAllText(_manifestPath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        catch
        {
            // if manifest is corrupt, start fresh
            return new Dictionary<string, string>();
        }
    }

    private string ComputeHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
