using ChatApp.LLM.Models;

namespace ChatApp.LLM.Services;

public interface ILLMService
{
    Task<LLMResponse> GenerateResponseAsync(IEnumerable<LLMMessage> messageHistory, IEnumerable<LLMTool> tools, LLMConfig config);
}
