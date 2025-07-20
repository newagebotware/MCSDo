namespace Learn;

internal static class Context
{
    public static List<ChatMessage> FindNextFile(string understanding, Dictionary<string, string> files)
    {
        var fileList = string.Join("\n", files.Select(kvp => $"{kvp.Key}: {kvp.Value}\n"));

        return new List<ChatMessage>
        {
            new ChatMessage
            {
                Role = "system",
                Content = "You are an AI agent that is very good at explaining large code bases to new engineers onboarding."
            },
            new ChatMessage
            {
                Role = "system",
                Content = "The list of files that you have already looked are marked as \"seen\" and the rest are marked \"unseen\""
            },
            new ChatMessage
            {
                Role = "system",
                Content = $"Here are the list of files: {fileList}"
            },
            new ChatMessage
            {
                Role = "system",
                Content = $"The understanding so far is {understanding}"
            },
            new ChatMessage
            {
                Role = "system",
                Content = $"Get the next file to read to update the understanding"
            },
        };
    }

    public async static Task<List<ChatMessage>>  AddUnderstanding(string understanding, string fileName)
    {
        string fileContent = await File.ReadAllTextAsync(fileName);
        return new List<ChatMessage>
        {
            new ChatMessage
            {
                Role = "system",
                Content = "You are an AI agent that is very good at explaining large code bases to new engineers onboarding."
            },
            new ChatMessage
            {
                Role = "system",
                Content = $"The understanding so far is {understanding}"
            },
            new ChatMessage
            {
                Role = "system",
                Content = $"Contents of another file from the source {fileName} is as follows: {fileContent}"
            },
            new ChatMessage
            {
                Role = "system",
                Content = $"Process the updated understanding"
            },
        };
    }
}
