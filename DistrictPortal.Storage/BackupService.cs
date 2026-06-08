using System;
using System.IO;

namespace DistrictPortal.Storage.Services;

public static class BackupService
{
    public static string CreateBackup(string sourceFilePath, string backupsFolder)
    {
        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("Файл данных не найден.", sourceFilePath);

        Directory.CreateDirectory(backupsFolder);

        var fileName = $"posts_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        var destPath = Path.Combine(backupsFolder, fileName);

        File.Copy(sourceFilePath, destPath, overwrite: false);

        return destPath; 
    }
}