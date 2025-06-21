namespace Learn;

public class Tools
{
    public static List<ToolDefinition> Definitions = new List<ToolDefinition>
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
}
