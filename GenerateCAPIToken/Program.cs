using Microsoft.Identity.Client;
using System.Security.Cryptography.X509Certificates;

try
{
    // Get the certificate
    string tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";
    string clientId = "df641a6e-c4fc-4a1c-8b21-2a767bc4cdf8";
    string scope = "api://4309f23c-178a-4fc7-a36e-68d8fee6ca7b/.default";

    // Open the Local Machine certificate store
    using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
    store.Open(OpenFlags.ReadOnly);

    var certificate = store.Certificates
        .OfType<X509Certificate2>()
        .Where(cert => cert.Subject.Contains("local.cognitiveapi.powerapps.com"))
        .OrderByDescending(cert => cert.NotBefore)
        .FirstOrDefault();

    if (certificate == null)
    {
        Console.WriteLine("No matching certificate found.");
        return;
    }

    // Configure MSAL client
    var app = ConfidentialClientApplicationBuilder
        .Create(clientId)
        .WithCertificate(certificate, sendX5C: true)
        .WithTenantId(tenantId)
        .Build();

    // Acquire token
    var result = app.AcquireTokenForClient(new[] { scope })
        .ExecuteAsync()
        .GetAwaiter()
        .GetResult();

    // Output the access token
    Console.WriteLine(result.AccessToken);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
