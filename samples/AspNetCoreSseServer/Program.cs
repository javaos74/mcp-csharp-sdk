using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using TestServerWithHosting.Tools;
using UiPath.Robot.Api;
using UiPath.Robot.MCP.Tools;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<EchoTool>()
    .WithTools<UiPathRobotTool>()
    .WithTools<SampleLlmTool>();

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(b => b.AddMeter("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithLogging()
    .UseOtlpExporter();
/*
builder.Services.AddSingleton<RobotClient>(sp =>
{
    var client = new RobotClient();
    return client;
});
*/


var app = builder.Build();

app.MapMcp();

app.Run();
