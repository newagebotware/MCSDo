﻿using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

class Program
{
    // Configuration - replace with your values
    private static readonly string adoUrl = "https://dev.azure.com/msazure";
    private static readonly string project = "CCI";
    private static readonly int parentWorkItemId = 33080125; // Replace with your parent work item ID
    private static readonly string textFilePath = "workitems.txt"; // Path to your text file

    static async Task Main(string[] args)
    {
        try
        {
            // Read work item titles from the text file
            var workItemTitles = File.ReadAllLines(textFilePath);
            //var workItemTitles = new string[] { "hello" };
            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pat = File.ReadAllText($"{homeDirectory}\\.tokens\\adopat").Trim();

            if (workItemTitles.Length == 0)
            {
                Console.WriteLine("No work item titles found in the text file.");
                return;
            }

            // Connect to Azure DevOps
            var credentials = new VssBasicCredential(string.Empty, pat);
            var connection = new VssConnection(new Uri(adoUrl), credentials);
            var workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>();

            var num = 16000;
            // Create work items
            foreach (var title in workItemTitles)
            {
                if (string.IsNullOrWhiteSpace(title))
                    continue;

                await CreateWorkItemAsync(workItemTrackingClient, title.Trim(), parentWorkItemId, num);
                num = num + 1000;
            }

            Console.WriteLine("All work items created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static async Task CreateWorkItemAsync(WorkItemTrackingHttpClient client, string title, int parentId, int stackRank)
    {
        try
        {

            // Create a JSON patch document to define the work item
            //var patchDocument = new List<Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation>
            var patchDocument = new JsonPatchDocument
                {
                    new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
                    {
                        Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                        Path = "/fields/System.Title",
                        Value = title
                    },
                    new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
                    {
                        Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                        Path = "/fields/System.WorkItemType",
                        Value = "Feature" // Specify the work item type (e.g., Task, User Story)
                    }
                };

            // Add parent-child relationship
            patchDocument.Add(new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
            {
                Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                Path = "/relations/-",
                Value = new
                {
                    rel = "System.LinkTypes.Hierarchy-Reverse",
                    url = $"{adoUrl}/{project}/_apis/wit/workItems/{parentId}",
                    attributes = new { name = "Parent" }
                }
            });
            patchDocument.Add(new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
            {
                Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                Path = "/fields/Microsoft.VSTS.Common.StackRank",
                Value = stackRank
            });

            // Create the work item
            var workItem = await client.CreateWorkItemAsync(patchDocument, project, "Feature");

            Console.WriteLine($"Created work item: ID = {workItem.Id}, Title = {title}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create work item '{title}': {ex.Message}");
        }
    }
}
