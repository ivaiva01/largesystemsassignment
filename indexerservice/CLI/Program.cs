using System.Diagnostics.Metrics;
using Application;
using indexer.dto;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
if (rabbitMqSettings == null)
{
    throw new ArgumentNullException("RabbitMqSettings");
}

var meter = new Meter("IndexingService", "1.0");
builder.Services.AddSingleton(meter);

builder.Services.AddSingleton<ITracingService, TracingService>();


builder.Services.AddSingleton(rabbitMqSettings);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

builder.Services.AddSingleton<IMessageProcessor, MessageProcessor>();
builder.Services.AddHostedService<MessageProcessingService>();

var app = builder.Build();


using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

var publisher = services.GetRequiredService<IMessagePublisher>();

var testMessage = new MessageDto<EmailDto>
{
    Id = Guid.NewGuid().ToString(),
    Timestamp = DateTime.UtcNow,
    Content = new EmailDto
    {
        Id = "1",
        Body = "Hello, World!",
        FileName = "test.txt",
        FileBytes = "Hello, World!"u8.ToArray()
    }
};

Console.WriteLine("Publishing a message");
await publisher.PublishAsync(testMessage, CancellationToken.None);

await app.RunAsync();