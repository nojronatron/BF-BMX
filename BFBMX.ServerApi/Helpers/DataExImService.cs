using BFBMX.Service.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BFBMX.ServerApi.Helpers;

/// <summary>
/// Manages importing and exporting data between files and in-memory database.
/// </summary>
public static class DataExImService
{
    public static List<WinlinkMessageModel> ImportFileData()
    {
        List<WinlinkMessageModel> result = new();
        string? userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
        string? bfBmxFolderName = Environment.GetEnvironmentVariable("BFBMX_FOLDER_NAME");
        string? fileName = Environment.GetEnvironmentVariable("BFBMX_BACKUP_FILE_NAME");

        if (string.IsNullOrWhiteSpace(bfBmxFolderName) 
            || string.IsNullOrWhiteSpace(fileName)
            || string.IsNullOrWhiteSpace(userProfilePath))
        {
            return result;
        }
        else
        {
            string backupFilePath = Path.Combine(userProfilePath, "Documents", bfBmxFolderName, fileName);

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
        }
        return result;
    }

    public static async Task<List<WinlinkMessageModel>> ImportFileDataAsync()
    {
        return await Task.Run(() =>
        {
            return DataExImService.ImportFileData();
        });
    }

    public static bool ExportDataToFile(List<WinlinkMessageModel> data)
    {
        // todo: return a count of items backed up
        bool result = false;

        if (data.Count < 1)
        {
            return result;
        }

        string? userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
        string? bfBmxFolderName = Environment.GetEnvironmentVariable("BFBMX_FOLDER_NAME");
        string? fileName = Environment.GetEnvironmentVariable("BFBMX_BACKUP_FILE_NAME");

        if (string.IsNullOrWhiteSpace(bfBmxFolderName)
            || string.IsNullOrWhiteSpace(fileName)
            || string.IsNullOrWhiteSpace(userProfilePath))
        {
            return result;
        }

        try
        {
            string filePath = Path.Combine(userProfilePath, "Documents", bfBmxFolderName, fileName);
            File.Create(filePath).Dispose();

            JsonSerializerOptions options = new()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize<List<WinlinkMessageModel>>(data, options);
            File.WriteAllText(filePath, json);
            result = true;
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

        return result;
    }

    public static async Task<bool> ExportDataToFileAsync(List<WinlinkMessageModel> data)
    {
        // todo: return a count of items backed up
        return await Task.Run(() =>
        {
            return DataExImService.ExportDataToFile(data);
        });
    }
}
