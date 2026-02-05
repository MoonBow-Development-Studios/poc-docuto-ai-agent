using System.Text.Json.Serialization;

namespace Docuto.Agent.Models;

public class DocumentationResult
{
    [JsonPropertyName("documentationPath")]
    public string DocumentationPath { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}