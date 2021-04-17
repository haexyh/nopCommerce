using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ItSuite.Rest.Aws.S3File;
using MySql.Data.MySqlClient;
using MySQLBackupNetCore;
using Nop.Core.Domain.Logging;
using Nop.Data;
using Nop.Services.Logging;
using RestSharp;

namespace Nop.Plugin.Misc.Scheduler.Services
{
    public class BackupService
    {
        private readonly ILogger _logger;
        private readonly BackupSchedulerSettings _backupSchedulerSettings;

        private const string CHARSET = "charset";
        private const string CONVERT_TO_ZERO_DATE_TIME = "convertzerodatetime";

        public BackupService(ILogger logger, BackupSchedulerSettings backupSchedulerSettings)
        {
            _logger = logger;
            _backupSchedulerSettings = backupSchedulerSettings;
        }

        public async Task CreateBackupAsync()
        {
            try
            {
                using var s3Service = new S3FileClient(_backupSchedulerSettings.Endpoint, _backupSchedulerSettings.ApiKey, _backupSchedulerSettings.InstanceGuid);
                var settings = await DataSettingsManager.LoadSettingsAsync();
                var fullConnectionString =
                    createExtendedConnectionString(settings.ConnectionString, settings.DataProvider);
                var tempFileNameWithoutExtension = await createBackup(fullConnectionString);
                var tempZipArchive = await createZipArchive(tempFileNameWithoutExtension);
                var bytes = await File.ReadAllBytesAsync(tempZipArchive);
                var response = await s3Service.UploadZip($"{DateTime.Now:yyyyMMddhhmmss}_Backup.zip", bytes);
                await clear(response, tempZipArchive);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(nameof(CreateBackupAsync), e);
            }
        }

        private async Task clear(IRestResponse response, string tempZipArchive)
        {
            var message = string.IsNullOrWhiteSpace(response.ErrorMessage) ? "Successful" : response.ErrorMessage;
            await _logger.InsertLogAsync(response.IsSuccessful ? LogLevel.Information : LogLevel.Fatal, message);
            if (response.IsSuccessful && File.Exists(tempZipArchive)) File.Delete(tempZipArchive);
        }

        private async Task<string> createBackup(string connectionString)
        {
            var path = Path.GetTempPath();
            var baseFilename = $"{DateTime.Now:hmmss}_dev";
            var fullPath = $"{path}/{baseFilename}";

            await using var connection = new MySqlConnection(connectionString);
            await using var command = new MySqlCommand();
            using var mySqlBackup = new MySqlBackup(command);
            command.Connection = connection;
            await connection.OpenAsync();
            mySqlBackup.ExportToFile($"{fullPath}.sql");
            await connection.CloseAsync();
            return fullPath;
        }

        private async Task<string> createZipArchive(string filePath)
        {
            await using var createFileStream = new FileStream($"{filePath}.zip", FileMode.Create);
            using var zipArchive = new ZipArchive(createFileStream, ZipArchiveMode.Create);
            zipArchive.CreateEntryFromFile($"{filePath}.sql", $"{DateTime.Now:yyMMddhhmmss}.sql");
            return $"{filePath}.zip";
        }

        private string createExtendedConnectionString(string connectionString, DataProviderType dataProviderType) =>
            dataProviderType switch
            {
                DataProviderType.MySql => createMySqlConnectionString(connectionString),
                _ => throw new NotImplementedException(),
            };

        private string createMySqlConnectionString(string connectionString)
        {
            if (!connectionString.EndsWith(';')) connectionString += ';';
            var properties = splitConnectionString(connectionString);
            if (properties.All(i => !i.StartsWith(CHARSET))) connectionString += "charset=utf8;";

            if (properties.SingleOrDefault(i => i.StartsWith(CONVERT_TO_ZERO_DATE_TIME))?.EndsWith("true") == true)
            {
                var newData = connectionString.Split(';')
                    .Where(i => !i.StartsWith(CONVERT_TO_ZERO_DATE_TIME))
                    .ToArray();
                connectionString = string.Concat(newData, "convertzerodatetime=true;");
            }

            return connectionString;
        }

        private static string[] splitConnectionString(string connectionString)
        {
            if (!connectionString.EndsWith(';')) connectionString += ';';

            var properties = connectionString.Split(';', StringSplitOptions.TrimEntries)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => i.ToLower())
                .ToArray();

            return properties;
        }
    }
}