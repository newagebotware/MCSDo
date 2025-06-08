using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private static readonly ConcurrentQueue<ChatMessage> _messageQueue = new();
        private static readonly List<IResponseStream> _clients = new();
        private static readonly CancellationTokenSource _cts = new();
        private static readonly Task _processorTask;

        public class ChatMessage
        {
            public string? User { get; set; }
            public string? Message { get; set; }
        }

        static ChatController()
        {
            _processorTask = Task.Run(() => ProcessMessagesAsync(_cts.Token));
        }

        [HttpPost]
        public IActionResult PostMessage([FromBody] ChatMessage chatMessage)
        {
            if (chatMessage == null || string.IsNullOrEmpty(chatMessage.User) || string.IsNullOrEmpty(chatMessage.Message))
            {
                return BadRequest("Invalid message payload.");
            }

            _messageQueue.Enqueue(chatMessage);
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

        private static async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_messageQueue.TryDequeue(out var chatMessage))
                {
                    var messageData = JsonSerializer.Serialize(new
                    {
                        user = chatMessage.User,
                        message = chatMessage.Message,
                        timestamp = DateTime.UtcNow.ToString("o")
                    });
                    BroadcastMessage($"data: {messageData}\n\n");

                    if (chatMessage.Message != null && chatMessage.Message.Contains("abc", StringComparison.OrdinalIgnoreCase))
                    {
                        var botMessage = JsonSerializer.Serialize(new
                        {
                            user = "bot",
                            message = $"{chatMessage.User} said abc",
                            timestamp = DateTime.UtcNow.ToString("o")
                        });
                        BroadcastMessage($"data: {botMessage}\n\n");
                    }
                }
                else
                {
                    await Task.Delay(100, cancellationToken);
                }
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

        // Mark Dispose as NonAction to exclude from Swagger
        [NonAction]
        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
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
}
