using System.Threading.Channels;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Controllers;
using ChatApp.Models;
using ChatApp.Services;
using ChatApp.LLM.Services;
using ChatApp.LLM.Models;
using System;
using System.Collections.Generic;

namespace ChatApp.Services;

public class MessageProcessorService : BackgroundService
{
    private readonly ChannelReader<ChatController.ChatMessage> _channelReader;
    private readonly List<IResponseStream> _clients;
    private readonly IDatabaseService _databaseService;
    private readonly ILLMService _llmService;

    public MessageProcessorService(
        Channel<ChatController.ChatMessage> messageChannel,
        List<IResponseStream> clients,
        IDatabaseService databaseService,
        ILLMService llmService)
    {
        _channelReader = messageChannel.Reader;
        _clients = clients;
        _databaseService = databaseService;
        _llmService = llmService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _databaseService.InitializeAsync();

        var tools = new List<LLMTool>
        {
            new LLMTool
            {
                Name = "calculator",
                Description = "Evaluates a mathematical expression",
                Parameters = new Dictionary<string, object>
                {
                    { "expression", new { type = "string", description = "Mathematical expression (e.g., 2+2)" } }
                }
            }
        };

        var llmConfig = new LLMConfig
        {
            ModelName = "gpt-4o",
            SystemMessage = "You are a helpful assistant."
        };

        await foreach (var chatMessage in _channelReader.ReadAllAsync(stoppingToken))
        {
            var message = new Message
            {
                Role = "user",
                User = chatMessage.User ?? "Unknown",
                Content = chatMessage.Message ?? "",
                Timestamp = DateTime.UtcNow
            };
            await _databaseService.SaveMessageAsync(message);

            var messageData = JsonSerializer.Serialize(new
            {
                user = message.User,
                message = message.Content,
                timestamp = message.Timestamp.ToString("o")
            });
            BroadcastMessage($"data: {messageData}\n\n");

            if (chatMessage.Message != null && chatMessage.Message.Contains("abc", StringComparison.OrdinalIgnoreCase))
            {
                var botMessage = new Message
                {
                    Role = "assistant",
                    User = "bot",
                    Content = $"{chatMessage.User} said abc",
                    Timestamp = DateTime.UtcNow
                };
                await _databaseService.SaveMessageAsync(botMessage);

                var botMessageData = JsonSerializer.Serialize(new
                {
                    user = botMessage.User,
                    message = botMessage.Content,
                    timestamp = botMessage.Timestamp.ToString("o")
                });
                BroadcastMessage($"data: {botMessageData}\n\n");
            }

            if (chatMessage.Message != null && chatMessage.Message.StartsWith("/llm ", StringComparison.OrdinalIgnoreCase))
            {
                var history = await _databaseService.GetRecentMessagesAsync(10);
                var llmHistory = history.ConvertAll(m => new LLMMessage
                {
                    Role = m.Role,
                    Content = m.Content,
                    ToolCallId = m.Role == "tool_call" || m.Role == "tool_response" ? Guid.NewGuid().ToString() : null,
                    ToolName = m.Role == "tool_call" ? "calculator" : null,
                    Arguments = m.Role == "tool_call" ? new Dictionary<string, object> { { "expression", m.Content } } : null,
                    ToolResult = m.Role == "tool_response" ? JsonSerializer.Deserialize<Dictionary<string, object>>(m.Content) : null
                });

                llmHistory.Insert(0, new LLMMessage
                {
                    Role = "system",
                    Content = llmConfig.SystemMessage
                });
                llmHistory.Add(new LLMMessage
                {
                    Role = "user",
                    Content = chatMessage.Message.Substring(5)
                });

                try
                {
                    var llmResponse = await _llmService.GenerateResponseAsync(llmHistory, tools, llmConfig);

                    if (llmResponse.ToolCallName == "calculator")
                    {
                        var toolCallMessage = new Message
                        {
                            Role = "tool_call",
                            User = "bot",
                            Content = llmResponse.ToolCallArguments?["expression"]?.ToString() ?? "",
                            Timestamp = DateTime.UtcNow
                        };
                        await _databaseService.SaveMessageAsync(toolCallMessage);

                        var toolCallData = JsonSerializer.Serialize(new
                        {
                            user = toolCallMessage.User,
                            message = $"Tool Call: {toolCallMessage.Content}",
                            timestamp = toolCallMessage.Timestamp.ToString("o")
                        });
                        BroadcastMessage($"data: {toolCallData}\n\n");

                        Dictionary<string, object> toolResult;
                        try
                        {
                            var expression = llmResponse.ToolCallArguments?["expression"]?.ToString();
                            var result = EvaluateExpression(expression);
                            toolResult = new Dictionary<string, object>
                            {
                                { "result", result },
                                { "expression", expression ?? "" }
                            };
                        }
                        catch (Exception ex)
                        {
                            toolResult = new Dictionary<string, object>
                            {
                                { "error", ex.Message }
                            };
                        }

                        var toolResponseMessage = new Message
                        {
                            Role = "tool_response",
                            User = "bot",
                            Content = JsonSerializer.Serialize(toolResult),
                            Timestamp = DateTime.UtcNow
                        };
                        await _databaseService.SaveMessageAsync(toolResponseMessage);

                        var toolResponseData = JsonSerializer.Serialize(new
                        {
                            user = toolResponseMessage.User,
                            message = $"Tool Result: {toolResponseMessage.Content}",
                            timestamp = toolResponseMessage.Timestamp.ToString("o")
                        });
                        BroadcastMessage($"data: {toolResponseData}\n\n");

                        llmHistory.Add(new LLMMessage
                        {
                            Role = "tool_response",
                            Content = JsonSerializer.Serialize(toolResult),
                            ToolCallId = llmResponse.ToolCallId,
                            ToolResult = toolResult
                        });

                        llmResponse = await _llmService.GenerateResponseAsync(llmHistory, tools, llmConfig);
                    }

                    var assistantMessage = new Message
                    {
                        Role = "assistant",
                        User = "bot",
                        Content = llmResponse.Content,
                        Timestamp = DateTime.UtcNow
                    };
                    await _databaseService.SaveMessageAsync(assistantMessage);

                    var assistantMessageData = JsonSerializer.Serialize(new
                    {
                        user = assistantMessage.User,
                        message = assistantMessage.Content,
                        timestamp = assistantMessage.Timestamp.ToString("o")
                    });
                    BroadcastMessage($"data: {assistantMessageData}\n\n");
                }
                catch (Exception ex)
                {
                    var errorMessage = new Message
                    {
                        Role = "assistant",
                        User = "bot",
                        Content = $"LLM error: {ex.Message}",
                        Timestamp = DateTime.UtcNow
                    };
                    await _databaseService.SaveMessageAsync(errorMessage);

                    var errorMessageData = JsonSerializer.Serialize(new
                    {
                        user = errorMessage.User,
                        message = errorMessage.Content,
                        timestamp = errorMessage.Timestamp.ToString("o")
                    });
                    BroadcastMessage($"data: {errorMessageData}\n\n");
                }
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

    private double EvaluateExpression(string? expression)
    {
        if (string.IsNullOrEmpty(expression)) throw new ArgumentException("Expression is empty.");
        if (expression.Contains("+"))
        {
            var parts = expression.Split('+').Select(double.Parse).ToArray();
            return parts.Sum();
        }
        throw new NotSupportedException("Only addition is supported.");
    }
}
