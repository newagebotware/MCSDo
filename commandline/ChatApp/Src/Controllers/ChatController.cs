using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private static readonly List<IResponseStream> _clients = new();

        // Model for incoming chat message
        public class ChatMessage
        {
            public string? User { get; set; }
            public string? Message { get; set; }
        }

        // POST: api/chat
        [HttpPost]
        public IActionResult PostMessage([FromBody] ChatMessage chatMessage)
        {
            if (chatMessage == null || string.IsNullOrEmpty(chatMessage.User) || string.IsNullOrEmpty(chatMessage.Message))
            {
                return BadRequest("Invalid message payload.");
            }

            // Broadcast the message to all connected SSE clients
            var messageData = JsonSerializer.Serialize(new
            {
                user = chatMessage.User,
                message = chatMessage.Message,
                timestamp = DateTime.UtcNow.ToString("o")
            });

            BroadcastMessage($"data: {messageData}\n\n");

            return Ok();
        }

        // GET: api/chat/stream
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
                // Keep the connection open for SSE
                await client.KeepAlive();
            }
            finally
            {
                _clients.Remove(client);
            }
        }

        private static void BroadcastMessage(string message)
        {
            foreach (var client in _clients.ToList())
            {
                try
                {
                    client.WriteAsync(message).GetAwaiter().GetResult();
                }
                catch
                {
                    _clients.Remove(client);
                }
            }
        }
    }

    // Helper class to manage SSE client streams
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
            // Keep the connection open until cancelled
            while (!_response.HttpContext.RequestAborted.IsCancellationRequested)
            {
                await Task.Delay(1000); // Send a keep-alive ping or wait
            }
        }
    }
}