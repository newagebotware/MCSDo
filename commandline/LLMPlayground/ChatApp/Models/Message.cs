namespace ChatApp.Models;

public class Message
{
    public int Id { get; set; }
    public string Role { get; set; } = "user"; // user, assistant, tool_call, tool_response
    public string User { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
