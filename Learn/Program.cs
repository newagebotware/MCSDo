using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class OpenAIChatClient
{
    private readonly HttpClient httpClient;
    private readonly string apiKey;
    private const string Endpoint = "https://capi-resourceproxy.us-il002.gateway.test.island.powerapps.com/v0/resourceproxy/envId.TestEnvironment/azureopenai/llm/o3-2025-04-16/chatcompletions";
    //private const string Endpoint = "http://localhost:8080";

    public OpenAIChatClient(string apiKey)
    {
        this.apiKey = apiKey;
        this.httpClient = new HttpClient();
        this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        this.httpClient.DefaultRequestHeaders.Add("x-ms-client-principal-id", "023453678-09ad-43cc-91c8-22a54a9297a5"); 
        this.httpClient.DefaultRequestHeaders.Add("x-ms-client-tenant-id", "df641a6e-c4fc-4a1c-8b21-2a767bc4cdf8"); 
        this.httpClient.DefaultRequestHeaders.Add("X-ms-Source", "{\"consumptionSource\": \"Api\",\"partnerSource\": \"PVA.Planner\"}"); 
    }

    public async Task<RESP_RootResponse> CreateChatCompletionAsync(List<ChatMessage> messages, List<ToolDefinition> tools)
    {
        var requestBody = new ChatRequest
        {
            Model = "o3-2025-04-16",
            Messages = messages,
            Tools = tools,
            ToolChoice = "required"
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(requestBody, jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(Endpoint, content);

        ////
        var stream = await response.Content.ReadAsStreamAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return await JsonSerializer.DeserializeAsync<RESP_RootResponse>(stream, options);
        
        // var responseBody = await response.Content.ReadAsStringAsync();

        // if (!response.IsSuccessStatusCode)
        // {
        //     throw new Exception($"API Error: {response.StatusCode}\n{responseBody}");
        // }

        // return responseBody;
    }
}

// Model classes

/// ////////////////////////////////////////////// RESPONSE START
public class RESP_RootResponse
{
    [JsonPropertyName("completions")]
    public List<RESP_Completion> Completions { get; set; }

    [JsonPropertyName("usage")]
    public RESP_Usage Usage { get; set; }

    [JsonPropertyName("error")]
    public object Error { get; set; }
}

public class RESP_Completion
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("toolCalls")]
    public List<RESP_ToolCall> ToolCalls { get; set; }

    [JsonPropertyName("finishReason")]
    public string FinishReason { get; set; }
}

public class RESP_ToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("function")]
    public RESP_ToolFunction Function { get; set; }
}

public class RESP_ToolFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; }
}

public class RESP_Usage
{
    [JsonPropertyName("completionTokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("promptTokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("totalTokens")]
    public int TotalTokens { get; set; }
}
/// ////////////////////////////////////////////// RESPONSE END

public class ChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; }

    [JsonPropertyName("tools")]
    public List<ToolDefinition> Tools { get; set; }

    [JsonPropertyName("tool_choice")]
    public string ToolChoice { get; set; } = "auto";
}

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class ToolDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public ToolFunction Function { get; set; }
}

public class ToolFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("parameters")]
    public ToolParameters Parameters { get; set; }
}

public class ToolParameters
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, ToolProperty> Properties { get; set; }

    [JsonPropertyName("required")]
    public List<string> Required { get; set; }
}

public class ToolProperty
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}



class Program
{
    static async Task Main()
    {
        string apiKey = "";
        var client = new OpenAIChatClient(apiKey);

        var messages = new List<ChatMessage>
        {
            new ChatMessage
            {
                Role = "system",
                Content = "You are an AI agent that is very good at explaining large code bases to new engineers onboarding. You use the tools to undestand the code base."
            },
            new ChatMessage
            {
                Role = "system",
                Content = "What you know so far is the following is that the source code is listed unde C:\\s\\om2\\src"
            },
        };

        var tools = new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Function = new ToolFunction
                {
                    Name = "listAllFileRecursively",
                    Description = "Tool Used to recursively list all the files in a directory",
                    Parameters = new ToolParameters
                    {
                        Properties = new Dictionary<string, ToolProperty>
                        {
                            {
                                "directory", new ToolProperty
                                {
                                    Type = "string",
                                    Description = "the name of the directory"
                                }
                            }
                        },
                        Required = new List<string> { "directory" }
                    }
                }
            },
            new ToolDefinition
            {
                Function = new ToolFunction
                {
                    Name = "readFileContents",
                    Description = "Read the contents of a file",
                    Parameters = new ToolParameters
                    {
                        Properties = new Dictionary<string, ToolProperty>
                        {
                            {
                                "fileName", new ToolProperty
                                {
                                    Type = "string",
                                    Description = "the name of the file to read"
                                }
                            }
                        },
                        Required = new List<string> { "fileName" }
                    }
                }
            }
        };

        try
        {
            var result = await client.CreateChatCompletionAsync(messages, tools);
            Console.WriteLine("Response from OpenAI:");
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error:");
            Console.WriteLine(ex.Message);
        }
    }
}
