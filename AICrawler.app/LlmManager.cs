using OpenAI.Chat;

namespace AICrawler.app;

public interface ILlmManager
{
    ChatClient GetChatClient();
}

public class LlmManager: ILlmManager
{ 
    private readonly string _apiKey = "";
    private ChatClient _chatClient;
    
    public LlmManager(string apiKey, string model = "gpt-5.1")
    {
        _apiKey = apiKey;
        _chatClient = new ChatClient(model, _apiKey);
    }
    
    public ChatClient GetChatClient()
    {
        return _chatClient;
    }
}