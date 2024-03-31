using BFBMX.ServerApi.Collections;
using BFBMX.ServerApi.EF;
using BFBMX.ServerApi.Helpers;
using BFBMX.Service.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Locate backup file and import it if found otherwise assume new and continue
bool restoreSucceeded = bibReportPayloadsCollection.RestoreFromBackupFile();
string restoreMsg = restoreSucceeded ? "Backup file restored." : "No backup file found or no data in file.";
app.Logger.LogWarning("API Server auto-restore process: {msg}", restoreMsg);

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
    var addedRecordsToCollection = bibReportPayloadsCollection.AddEntityToCollection(request);

    var loggedInTabDelimitedFormat = bibRecordLogger.LogFlaggedRecordsTabDelimited(request);

    var wroteToJsonAuditFile = bibRecordLogger.LogWinlinkMessagePayloadToJsonAuditFile(request);

    // return 200 OK or 400 Bad Request
    if (addedRecordsToCollection && loggedInTabDelimitedFormat && wroteToJsonAuditFile)
    {
        Results.Ok("Stored Winlink Message and all attached bib records to DB, TabDelimited file, and Audit file.");
    }
    else if (!addedRecordsToCollection)
    {
        Results.Problem("Unable to store Winlink Message headers and bib contents.");
    }
    else if (!loggedInTabDelimitedFormat)
    {
        Results.Problem("Unable to log bib records for Access importation.");
    }
    else
    {
        Results.Problem("Unable to completely store and log message with all bib records.");
    }
})
.Produces(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest);

// trigger a backup of the local DB to a remote location
app.MapPost("/TriggerBackup", () =>
{
    try
    {
        // todo: result includes a Count of backed up items
        if (bibReportPayloadsCollection.BackupCollection())
        {
            Results.Ok("Backup succeeded.");
        }
        else
        {
            Results.Accepted("No backup file or no data to backup.");
        }
    }
    catch (Exception)
    {
        Results.Problem("Server error during backup.");
    }
})
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status202Accepted)
.ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();
