// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PassOn.Demos.Services;

Console.WriteLine("Hello, World!");

var host = Host
    .CreateDefaultBuilder()
    .ConfigureServices((ctx, services) => {
        services.AddHostedService<SimpleMapping>();
        services.AddHostedService<SourceAttributeMapping>();
        services.AddHostedService<TargetAttributeMapping>();
        services.AddHostedService<SourceCustomMapping>();
        services.AddHostedService<TargetCustomMapping>();
    })
    .Build();

await host.RunAsync();