using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ChatApp.Services;
using ChatApp.Models;

namespace ChatApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private static readonly Channel<ChatMessage> _messageChannel = Channel.CreateUnbounded<ChatMessage>();
    private static readonly List<IResponseStream> _clients = new();
    private readonly IDatabaseService _databaseService;

    public class ChatMessage
    {
        public string? User { get; set; }
        public string? Message { get; set; }
    }

    public static Channel<ChatMessage> MessageChannel => _messageChannel;
    public static List<IResponseStream> Clients => _clients;

    public ChatController(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

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
    public async Task Stream(CancellationToken cancellationToken)
    {
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        var client = new ResponseStream(Response);
        _clients.Add(client);

        try
        {
            // Keep-alive loop to prevent connection timeout
            while (!cancellationToken.IsCancellationRequested)
            {
                await client.WriteAsync(":ping\n\n"); // Send keep-alive comment
                await Response.Body.FlushAsync(cancellationToken);
                await Task.Delay(15000, cancellationToken); // Every 15 seconds
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected
        }
        finally
        {
            _clients.Remove(client);
        }
    }

    [HttpGet("transcript")]
    public async Task<IActionResult> GetTranscript()
    {
        var messages = await _databaseService.GetRecentMessagesAsync(100);
        var result = messages.Select(m => new
        {
            user = m.User,
            message = m.Content,
            timestamp = m.Timestamp.ToString("o")
        });
        return Ok(result);
    }
}

public interface IResponseStream
{
    Task WriteAsync(string message);
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
}
