namespace Learn;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string apiKey = await CapiToken.GetToken();
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

        try
        {
            var result = await client.CreateChatCompletionAsync(messages, Tools.Definitions);
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
