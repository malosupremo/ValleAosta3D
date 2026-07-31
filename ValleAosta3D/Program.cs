using Microsoft.Extensions.Configuration;
using ValleAosta3D.Infrastructure;
using ValleAosta3D.Models;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

AppOptions options =
    configuration.Get<AppOptions>()
    ?? throw new InvalidOperationException("Invalid configuration");

ApplicationFolders folders = new(options);

Console.WriteLine($"Root  : {folders.Root}");
Console.WriteLine($"Cache : {folders.Cache}");
Console.WriteLine($"Output: {folders.Output}");
