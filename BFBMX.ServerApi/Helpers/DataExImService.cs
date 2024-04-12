using BFBMX.Service.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BFBMX.ServerApi.Helpers;

/// <summary>
/// Manages importing and exporting data between files and in-memory database.
/// </summary>
public class DataExImService : IDataExImService
{
    private readonly IServerEnvFactory _serverEnvFactory;
    private readonly ILogger<DataExImService> _logger;

    public DataExImService(ILogger<DataExImService> logger, IServerEnvFactory serverEnvFactory)
    {
        _logger = logger;
        _serverEnvFactory = serverEnvFactory;
    }

    public List<WinlinkMessageModel> ImportFileData()
    {
        List<WinlinkMessageModel> result = new();

        string backupFilePath = Path.Combine(_serverEnvFactory.GetServerBackupFileNameAndPath());

        _logger.LogInformation("Attempting to read backup file {backupFilePath}.", backupFilePath);

        if (File.Exists(backupFilePath))
        {
#pragma warning disable IDE0063 // Use simple 'using' statement
            using (StreamReader backupFile = File.OpenText(backupFilePath))
            {
                JsonSerializerOptions options = new()
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    PropertyNameCaseInsensitive = true
                };

                WinlinkMessageModel[]? streamedJson = JsonSerializer.Deserialize<WinlinkMessageModel[]>(backupFile.ReadToEnd(), options);

                if (streamedJson is not null)
                {
                    result.AddRange(streamedJson);
                }
            }
#pragma warning restore IDE0063 // Use simple 'using' statement
        }

        _logger.LogInformation("{num} Winlink Messages read from {backupFilePath}.", result.Count, backupFilePath);
        return result;
    }

    public int ExportDataToFile(List<WinlinkMessageModel> data)
    {
        int itemsCount = 0;

        if (data.Count > 0)
        {
            string fileNameAndPath = _serverEnvFactory.GetServerBackupFileNameAndPath();
            _logger.LogInformation("Backup Filename {filenameAndPath} set.", fileNameAndPath);

            try
            {
                File.Create(fileNameAndPath).Dispose();
                _logger.LogInformation("{filenameAndPath} opened for writing.", fileNameAndPath);

                JsonSerializerOptions options = new()
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize<List<WinlinkMessageModel>>(data, options);
                File.WriteAllText(fileNameAndPath, json);
                itemsCount = data.Count;
                _logger.LogInformation("{num} items written to {filenameAndPath}.", itemsCount, fileNameAndPath);
            }
            catch (UnauthorizedAccessException uAex)
            {
                _logger.LogError("Unauthorized access to {fileNameAndPath}. Operation HALTED.", fileNameAndPath);
                _logger.LogError("Unauthorized access exception message {exMsg}", uAex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Some other exception was thrown! {exMsg} {exstack}", ex.Message, ex.StackTrace);
            }
        }

        return itemsCount;
    }
}
