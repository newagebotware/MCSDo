namespace Learn;

public class Tools
{
    public static List<ToolDefinition> GetNextFile = new List<ToolDefinition>
    {
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
    };

    public static List<ToolDefinition> UpdateUnderstanding = new List<ToolDefinition>
    {
        new ToolDefinition
        {
            Function = new ToolFunction
            {
                Name = "UpdateUnderstanding",
                Description = "Provide the updated understanding of the source base",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        {
                            "understanding", new ToolProperty
                            {
                                Type = "string",
                                Description = "The total understanding of the codebase"
                            }
                        }
                    },
                    Required = new List<string> { "understanding" }
                }
            }
        }
    };

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
        },
        new ToolDefinition
        {
            Function = new ToolFunction
            {
                Name = "UpdateUnderstanding",
                Description = "Provide the updated understanding of the source base",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        {
                            "understanding", new ToolProperty
                            {
                                Type = "string",
                                Description = "The total understanding of the codebase"
                            }
                        }
                    },
                    Required = new List<string> { "understanding" }
                }
            }
        }
    };
}
