using BFBMX.ServerApi.Collections;
using BFBMX.ServerApi.EF;
using BFBMX.ServerApi.Helpers;
using BFBMX.Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

string serverPort = Environment.GetEnvironmentVariable("BFBMX_SERVERPORT") ?? "5150";
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
builder.Services.AddScoped<IBibReportsCollection, BibReportsCollection>();
builder.Services.AddScoped<IBibRecordLogger, BibRecordLogger>();

var app = builder.Build();

// log server startup
app.Logger.LogInformation("API Server starting up.");

// make registered services available for use in this codepage
var scope = app.Services.CreateScope();
var bibReportPayloadsCollection = scope.ServiceProvider.GetRequiredService<IBibReportsCollection>();
var bibRecordLogger = scope.ServiceProvider.GetRequiredService<IBibRecordLogger>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// supports logging HTTP Requests and Responses
app.UseHttpLogging();

app.MapGet("/", () => new
{
    Message = "!Ehlo Werld"
}).Produces(200).ProducesProblem(400);

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
        loggedInTabDelimitedFormat = bibRecordLogger.LogFlaggedRecordsTabDelimited(request);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning("MapPost Exception caught at LogFlaggedRecordsTabDelimited(request) call! {exceptionMessage}\n{exceptionTrace}", ex.Message, ex.StackTrace);
    }

    bool wroteToJsonAuditFile = false;
    try
    {
        wroteToJsonAuditFile = bibRecordLogger.LogWinlinkMessagePayloadToJsonAuditFile(request);
    }
        catch (Exception ex)
    {
        app.Logger.LogWarning("MapPost Exception caught at LogWinlinkMessagePayloadToJsonAuditFile(request) call! {exceptionMessage}\n{exceptionTrace}", ex.Message, ex.StackTrace);
    }

    // return 200 OK or 400 Bad Request
    if (addedRecordsToCollection && loggedInTabDelimitedFormat && wroteToJsonAuditFile)
    {
        Results.Ok();
    }
    else if (!addedRecordsToCollection)
    {
        Results.Problem();
    }
    else if (!loggedInTabDelimitedFormat)
    {
        Results.Problem();
    }
    else
    {
        Results.Problem();
    }
})
.Produces(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest);

app.Run();
