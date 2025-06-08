using ChatApp.LLM.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApp.LLM.Services;

public class LLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiEndpoint;
    private readonly string _apiKey;

    public LLMService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiEndpoint = configuration["LLM:ApiEndpoint"] ?? throw new ArgumentNullException("LLM:ApiEndpoint");
        _apiKey = configuration["LLM:ApiKey"] ?? throw new ArgumentNullException("LLM:ApiKey");
    }

    public async Task<LLMResponse> GenerateResponseAsync(IEnumerable<LLMMessage> messageHistory, IEnumerable<LLMTool> tools, LLMConfig config)
    {
        var requestBody = new
        {
            messages = messageHistory,
            tools = tools,
            model = config.ModelName
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", _apiKey);

        var response = await _httpClient.PostAsync(_apiEndpoint, jsonContent);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);

        var choice = responseData?["choices"]?.ToObject<List<Dictionary<string, object>>>()?[0]?["message"]?.ToObject<Dictionary<string, object>>();
        var content = choice?["content"]?.ToString() ?? string.Empty;
        var toolCall = choice?["tool_calls"]?.ToObject<List<Dictionary<string, object>>>()?.FirstOrDefault();

        if (toolCall != null)
        {
            return new LLMResponse
            {
                ToolCallId = toolCall["id"]?.ToString(),
                ToolCallName = toolCall["function"]?.ToObject<Dictionary<string, object>>()?["name"]?.ToString(),
                ToolCallArguments = toolCall["function"]?.ToObject<Dictionary<string, object>>()?["arguments"]?.ToObject<Dictionary<string, object>>()
            };
        }

        return new LLMResponse { Content = content };
    }
}

public static class JsonExtensions
{
    public static T ToObject<T>(this object obj)
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj))!;
    }
}
