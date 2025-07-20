namespace Learn;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

record FileName
{
    public string fileName { get; init; }
}
record Understanding
{
    public string understanding { get; init; }
}

class Program
{
    static async Task Main()
    {
        string apiKey = await CapiToken.GetToken();
        var client = new OpenAIChatClient(apiKey);

        var understanding = "";
        try
        {
            while (true)
            {
                var c = Context.FindNextFile(understanding, FileList.Data);
                var result = await client.CreateChatCompletionAsync(c, Tools.GetNextFile);
                Console.WriteLine("Response from OpenAI:");
                var fnStr = result.Completions[0].ToolCalls[0].Function.Arguments;
                var fn = JsonSerializer.Deserialize<FileName>(fnStr).fileName;


                c = await Context.AddUnderstanding(understanding, $"C:\\s\\om2\\{fn}");
                result = await client.CreateChatCompletionAsync(c, Tools.UpdateUnderstanding);
                Console.WriteLine("Response from OpenAI:");
                var u = result.Completions[0].ToolCalls[0].Function.Arguments;
                understanding = JsonSerializer.Deserialize<Understanding>(u).understanding;



                Console.WriteLine(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error:");
            Console.WriteLine(ex.Message);
        }
    }
}
