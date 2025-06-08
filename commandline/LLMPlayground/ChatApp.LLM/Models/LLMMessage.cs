namespace ChatApp.LLM.Models;

public class LLMMessage
{
    public string Role { get; set; } = string.Empty; // user, assistant, tool_call, tool_response, system
    public string Content { get; set; } = string.Empty;
    public string? ToolCallId { get; set; }
    public string? ToolName { get; set; }
    public Dictionary<string, object>? Arguments { get; set; }
    public Dictionary<string, object>? ToolResult { get; set; }
}
