using Microsoft.Extensions.Configuration;
using ValleAosta3D;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

AppOptions options =
    configuration.Get<AppOptions>()
    ?? throw new InvalidOperationException("Invalid configuration");

Console.WriteLine($"Scale: 1:{options.Model.Scale}");
Console.WriteLine($"Vertical: {options.Model.VerticalExaggeration}x");
