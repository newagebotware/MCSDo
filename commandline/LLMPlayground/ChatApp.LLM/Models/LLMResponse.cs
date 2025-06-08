namespace ChatApp.LLM.Models;

public class LLMResponse
{
    public string Content { get; set; } = string.Empty;
    public string? ToolCallId { get; set; }
    public string? ToolCallName { get; set; }
    public Dictionary<string, object>? ToolCallArguments { get; set; }
}
