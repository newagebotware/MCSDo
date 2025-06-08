using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ChatApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private static readonly Channel<ChatMessage> _messageChannel = Channel.CreateUnbounded<ChatMessage>();
    private static readonly List<IResponseStream> _clients = new();

    public class ChatMessage
    {
        public string? User { get; set; }
        public string? Message { get; set; }
    }

    // Expose channel and clients for dependency injection
    public static Channel<ChatMessage> MessageChannel => _messageChannel;
    public static List<IResponseStream> Clients => _clients;

    [HttpPost]
    public async Task<IActionResult> PostMessage([FromBody] ChatMessage chatMessage)
    {
        if (chatMessage == null || string.IsNullOrEmpty(chatMessage.User) || string.IsNullOrEmpty(chatMessage.Message))
        {
            return BadRequest("Invalid message payload.");
        }

        await _messageChannel.Writer.WriteAsync(chatMessage);
        return Ok();
    }

    [HttpGet("stream")]
    public async Task Stream()
    {
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        var client = new ResponseStream(Response);
        _clients.Add(client);

        try
        {
            await client.KeepAlive();
        }
        finally
        {
            _clients.Remove(client);
        }
    }

    [HttpGet("transcript")]
    public async Task<IActionResult> GetTranscript([FromServices] ChatDbContext dbContext)
    {
        var messages = await dbContext.Messages
            .OrderBy(m => m.Timestamp)
            .Take(100)
            .Select(m => new { user = m.User, message = m.Content, timestamp = m.Timestamp.ToString("o") })
            .ToListAsync();
        return Ok(messages);
    }
}

public interface IResponseStream
{
    Task WriteAsync(string message);
    Task KeepAlive();
}

public class ResponseStream : IResponseStream
{
    private readonly HttpResponse _response;

    public ResponseStream(HttpResponse response)
    {
        _response = response;
    }

    public async Task WriteAsync(string message)
    {
        await _response.WriteAsync(message);
        await _response.Body.FlushAsync();
    }

    public async Task KeepAlive()
    {
        while (!_response.HttpContext.RequestAborted.IsCancellationRequested)
        {
            await Task.Delay(1000);
        }
    }
}
