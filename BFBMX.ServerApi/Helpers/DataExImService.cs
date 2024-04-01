using BFBMX.Service.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BFBMX.ServerApi.Helpers;

/// <summary>
/// Manages importing and exporting data between files and in-memory database.
/// </summary>
public class DataExImService : IDataExImService
{
    private readonly ILogger<DataExImService> _logger;
    public string DocumentsDirectoryName => "Documents";

    public DataExImService(ILogger<DataExImService> logger)
    {
        _logger = logger;
    }

    public List<WinlinkMessageModel> ImportFileData()
    {
        List<WinlinkMessageModel> result = new();
        string? userProfilePath = ServerEnvFactory.GetuserProfilePath();
        string? bfBmxFolderName = ServerEnvFactory.GetServerFolderName();
        string? fileName = ServerEnvFactory.GetServerBackupFilename();

        string backupFilePath = Path.Combine(userProfilePath, DocumentsDirectoryName, bfBmxFolderName, fileName);

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

        return result;
    }

    public async Task<List<WinlinkMessageModel>> ImportFileDataAsync()
    {
        return await Task.Run(() =>
        {
            return ImportFileData();
        });
    }

    public int ExportDataToFile(List<WinlinkMessageModel> data)
    {
        int itemsCount = 0;
        string? userProfilePath = ServerEnvFactory.GetuserProfilePath();
        string? bfBmxFolderName = ServerEnvFactory.GetServerFolderName();
        string? fileName = ServerEnvFactory.GetServerBackupFilename();

        if (data.Count > 0)
        {
            try
            {
                string filePath = Path.Combine(userProfilePath, DocumentsDirectoryName, bfBmxFolderName, fileName);
                File.Create(filePath).Dispose();

                JsonSerializerOptions options = new()
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize<List<WinlinkMessageModel>>(data, options);
                File.WriteAllText(filePath, json);
                itemsCount = data.Count;
            }
            catch (UnauthorizedAccessException ex)
            {
                string msg = $"Unauthorized access to path {userProfilePath}, or {bfBmxFolderName}, or {fileName}.";
                Debug.WriteLine(msg);
                Debug.WriteLine($"Related exception message: {ex.Message}");
            }
            catch (Exception ex)
            {
                string words = "File access was authorized but an error occurred while logging BibRecords to the file";
                string msg = $"{words} {userProfilePath}\\{bfBmxFolderName}\\{fileName}: {ex.Message}";
                Debug.WriteLine(msg);
            }
        }

        return itemsCount;
    }
}
