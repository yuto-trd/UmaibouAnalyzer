using Grafana.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using UmaibouAnalyzer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .UseGrafana()
    .Build();
using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .UseGrafana()
    .Build();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register HttpClient for RendererService
builder.Services.AddHttpClient<IRendererService, RendererService>();

// Register AnalysisService
builder.Services.AddSingleton<IAnalysisService, AnalysisService>();

var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

if (!string.IsNullOrEmpty(otlpEndpoint))
{
    builder.Services.AddOpenTelemetry()
        .UseGrafana();

    // Configure Logging
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.UseGrafana();
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
