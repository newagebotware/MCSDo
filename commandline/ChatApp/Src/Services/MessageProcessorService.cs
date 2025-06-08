using System.Threading.Channels;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ChatApp.Controllers;
using ChatApp.Data;
using ChatApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Services
{
    public class MessageProcessorService : BackgroundService
    {
        private readonly ChannelReader<ChatController.ChatMessage> _channelReader;
        private readonly List<IResponseStream> _clients;
        private readonly IServiceScopeFactory _scopeFactory;

        public MessageProcessorService(
            Channel<ChatController.ChatMessage> messageChannel,
            List<IResponseStream> clients,
            IServiceScopeFactory scopeFactory)
        {
            _channelReader = messageChannel.Reader;
            _clients = clients;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var chatMessage in _channelReader.ReadAllAsync(stoppingToken))
            {
                // Save to database
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
                    var message = new Message
                    {
                        User = chatMessage.User ?? "Unknown",
                        Content = chatMessage.Message ?? "",
                        Timestamp = DateTime.UtcNow
                    };
                    dbContext.Messages.Add(message);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                // Broadcast the original message
                var messageData = JsonSerializer.Serialize(new
                {
                    user = chatMessage.User,
                    message = chatMessage.Message,
                    timestamp = DateTime.UtcNow.ToString("o")
                });
                BroadcastMessage($"data: {messageData}\n\n");

                // Inspect message and broadcast bot message if it contains "abc"
                if (chatMessage.Message != null && chatMessage.Message.Contains("abc", StringComparison.OrdinalIgnoreCase))
                {
                    var botMessage = new Message
                    {
                        User = "bot",
                        Content = $"{chatMessage.User} said abc",
                        Timestamp = DateTime.UtcNow
                    };

                    // Save bot message to database
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
                        dbContext.Messages.Add(botMessage);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }

                    var botMessageData = JsonSerializer.Serialize(new
                    {
                        user = botMessage.User,
                        message = botMessage.Content,
                        timestamp = botMessage.Timestamp.ToString("o")
                    });
                    BroadcastMessage($"data: {botMessageData}\n\n");
                }
            }
        }

        private void BroadcastMessage(string message)
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
}
