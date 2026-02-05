using OpenAI.Chat;

namespace AICrawler.Agent;

public interface ILlmManager
{
    ChatClient GetChatClient();
}

public class LlmManager: ILlmManager
{ 
    private readonly string _apiKey = "";
    private ChatClient _chatClient;
    
    public LlmManager(string apiKey, string? model = null)
    {
        _apiKey = apiKey;
        var resolvedModel = string.IsNullOrWhiteSpace(model) ? "gpt-5.1" : model;
        _chatClient = new ChatClient(resolvedModel, _apiKey);
    }
    
    public ChatClient GetChatClient()
    {
        return _chatClient;
    }
}
