using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ItSuite.Rest.Aws.S3File;
using MailKit.Security;
using MySql.Data.MySqlClient;
using MySQLBackupNetCore;
using Nop.Core.Domain.Logging;
using Nop.Data;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Scheduler.Services
{
    public class BackupService
    {
        private readonly ILogger _logger;
        private readonly BackupSchedulerSettings _backupSchedulerSettings;
        private readonly S3FileClient _s3FileClient;

        private const string CHARSET = "charset";
        private const string CONVERT_TO_ZERO_DATE_TIME = "convertzerodatetime";

        public BackupService(ILogger logger, BackupSchedulerSettings backupSchedulerSettings)
        {
            _logger = logger;
            _backupSchedulerSettings = backupSchedulerSettings;
            _s3FileClient = new S3FileClient(_backupSchedulerSettings.Endpoint, _backupSchedulerSettings.ApiKey,
                _backupSchedulerSettings.InstanceGuid);
        }

        public async Task CreateBackup()
        {
            try
            {
                var baseFilename = $"{DateTime.Now:yyyyMMddhhmmss}_Backup";

                var settings = await DataSettingsManager.LoadSettingsAsync();
                var fullConnectionString =
                    createExtendedConnectionString(settings.ConnectionString, settings.DataProvider);

                await using var connection = new MySqlConnection(fullConnectionString);
                await using var command = new MySqlCommand();
                using var mySqlBackup = new MySqlBackup(command);

                command.Connection = connection;
                await connection.OpenAsync();

                await using (var memoryStream = new MemoryStream())
                {
                    using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
                    var fileInArchive = archive.CreateEntry($"{baseFilename}.sql", CompressionLevel.Optimal);
                    await using (var entryStream = fileInArchive.Open())
                        mySqlBackup.ExportToStream(entryStream);
                    
                   var response =  await _s3FileClient.UploadZip($"{baseFilename}.zip", memoryStream);
                }

                await connection.CloseAsync();
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(nameof(CreateBackup), e);
            }
        }

        private string createExtendedConnectionString(string connectionString, DataProviderType dataProviderType) =>
            dataProviderType switch
            {
                DataProviderType.MySql => createMySqlConnectionString(connectionString),
                _ => throw new NotImplementedException(),
            };

        private string createMySqlConnectionString(string connectionString)
        {
            // todo better
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