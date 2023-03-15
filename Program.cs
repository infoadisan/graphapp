using System.IO;
using System.Security;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using helpers;
using Microsoft.Kiota.Abstractions.Authentication;
using graphconsoleapp.helpers;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }

    //3-once we have done 2, we can then define a method to obtain the Microsoft graph client

    //2-this method is going to create an instance of the clients we are gong to use to call Microsoft graph
    private static IAuthenticationProvider CreateAuthorizationProvider(IConfigurationRoot config, string userName, SecureString password)
    {
        var clientId = config["applicationId"];
        var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

        List<string> scopes = new List<string>();
        scopes.Add("User.Read");
        scopes.Add("Files.Read");

        var cca = PublicClientApplicationBuilder.Create(clientId)
                                                .WithAdfsAuthority(authority)
                                                .Build();
        return MsalAuthProviderG5.GetInstance(cca, scopes.ToArray(), userName, password);
    }

    //1-load settings from appsetting file
    private static IConfigurationRoot LoadAppSettings()
    {
        try
        {
            var config = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", false, true)
                                .Build();
            if (string.IsNullOrEmpty(config["ApplicationId"]) || string.IsNullOrEmpty(config["tenantId"]){
                return null;
            }
            return config;
        }
        catch (System.IO.FileNotFoundException)
        {

            return null;
        }
    }
}