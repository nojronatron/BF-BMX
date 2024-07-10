using BFBMX.ServerApi.Collections;
using BFBMX.ServerApi.EF;
using BFBMX.ServerApi.Helpers;
using BFBMX.Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

string serverPort = Environment.GetEnvironmentVariable("BFBMX_SERVER_PORT") ?? "5150";
_ = int.TryParse(serverPort, out int srvrPort);

// Add services to the container.
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    serverOptions.Listen(IPAddress.Any, srvrPort, listenOptions =>
    {
        listenOptions.UseConnectionLogging();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// entity framework must be configured and added as TRANSIENT in ASP.NET IoC
builder.Services.AddDbContextFactory <BibMessageContext> (options =>
{
    options.UseInMemoryDatabase($"BFBMX-{Guid.NewGuid()}");
});

builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

// define scoped collections, helpers, etc
builder.Services.AddSingleton<IServerEnvFactory, ServerEnvFactory>();
builder.Services.AddScoped<IBibReportsCollection, BibReportsCollection>();
builder.Services.AddScoped<IBibRecordLogger, BibRecordLogger>();
builder.Services.AddSingleton<IServerInfo, ServerInfo>();
builder.Services.AddSingleton<IServerLogWriter, ServerLogWriter>();

var app = builder.Build();

// log server startup
app.Logger.LogInformation("API Server starting up.");

// make registered services available for use in this codepage
var scope = app.Services.CreateScope();
var bibReportPayloadsCollection = scope.ServiceProvider.GetRequiredService<IBibReportsCollection>();
var bibRecordLogger = scope.ServiceProvider.GetRequiredService<IBibRecordLogger>();
var serverEnvVars = scope.ServiceProvider.GetRequiredService<IServerEnvFactory>();
var serverInfo = scope.ServiceProvider.GetRequiredService<IServerInfo>();
var serverLogWriter = scope.ServiceProvider.GetRequiredService<IServerLogWriter>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// supports logging HTTP Requests and Responses
app.UseHttpLogging();

// create bibrecordlogger folder path if not already exists
if (!Directory.Exists(serverEnvVars.GetServerLogPath()))
{
    Directory.CreateDirectory(serverEnvVars.GetServerLogPath());
}

// log server info to the console at startup
serverInfo.StartLogfileInfo();
serverInfo.StartHostInfo();

// init server activity logfile
await serverLogWriter.WriteActivityToLogAsync("Initialized server activity logger.");

app.MapGet("/api/v1/AllRecords", () =>
{
    // create a JSON file with all records from bibReportPayloadsCollection
    return bibReportPayloadsCollection.GetAllEntities();
}).Produces(200).ProducesProblem(500);

app.MapGet("/api/v1/ServerInfo", () =>
{
    if (serverInfo.CanStart())
    {
        serverInfo.StartHostInfo();
        serverInfo.StartLogfileInfo();
    }

    Dictionary<string, string> message = new();
    message.Add("version", "v1.6.0 Dev Preview");
    return Results.Json(message);
}).Produces(200).ProducesProblem(500);

app.MapPost("/WinlinkMessage", (WinlinkMessageModel request) =>
{
    bool addedRecordsToCollection = false;
    try
    {
        addedRecordsToCollection = bibReportPayloadsCollection.AddEntityToCollection(request);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning("MapPost Exception caught at AddEntityToCollection(request) call! {exceptionMessage}\n{exceptionTrace}", ex.Message, ex.StackTrace);
    }

    bool loggedInTabDelimitedFormat = false;
    try
    {
        loggedInTabDelimitedFormat = bibRecordLogger.LogWinlinkMessagePayloadToTabDelimitedFile(request);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning("MapPost Exception caught at LogFlaggedRecordsTabDelimited(request) call! {exceptionMessage}\n{exceptionTrace}", ex.Message, ex.StackTrace);
    }

    if (serverInfo.CanStart())
    {
        serverInfo.StartLogfileInfo();
        serverInfo.StartHostInfo();
    }

    // return 200 OK or 400 Bad Request
    if (addedRecordsToCollection && loggedInTabDelimitedFormat)
    {
        Results.Ok();
        serverLogWriter.WriteActivityToLogAsync("WinlinkMessage POST request processed successfully.");
    }
    else if (!addedRecordsToCollection)
    {
        Results.Problem();
        serverLogWriter.WriteActivityToLogAsync("WinlinkMessage POST request failed to add records to collection.");
    }
    else if (!loggedInTabDelimitedFormat)
    {
        Results.Problem();
        serverLogWriter.WriteActivityToLogAsync("WinlinkMessage POST request failed to log records to tab-delimited file.");
    }
    else
    {
        Results.Problem();
        serverLogWriter.WriteActivityToLogAsync("WinlinkMessage POST request failed to process.");
    }
})
.Produces(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest);

app.Run();
