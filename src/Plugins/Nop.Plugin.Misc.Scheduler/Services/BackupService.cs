using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public BackupService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<string> CreateBackup()
        {
            try
            {
                var file = $"~/{DateTime.Now.TimeOfDay:hh\\_mm}_dev_backup.sql";
                var settings = await DataSettingsManager.LoadSettingsAsync();
                var fullConnectionString = createExtendetConnectionString(settings.ConnectionString, settings);

                await using var connection = new MySqlConnection(fullConnectionString);
                await using var command = new MySqlCommand();
                using var mySqlBackup = new MySqlBackup(command);
                command.Connection = connection;
                await connection.OpenAsync();
                mySqlBackup.ExportToFile(file);
                await connection.CloseAsync();

                return file;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        private string createExtendetConnectionString(string connectionString, DataSettings dataSettings) =>
            dataSettings.DataProvider switch
            {
                DataProviderType.MySql => createMySqlConnectionString(connectionString),
                _ => throw new NotImplementedException(),
            };

        private string createMySqlConnectionString(string connectionString)
        {
            // todo better
            const string charset = "charset";
            const string convertToZeroDateTime = "convertzerodatetime";
            
            var properties = splitConnectionString(connectionString);

            if (properties.All(i => !i.StartsWith(charset))) connectionString += "charset=utf8;";

            if (properties.SingleOrDefault(i => !i.StartsWith(convertToZeroDateTime))?.EndsWith("true") == true)
            {
                var newData = connectionString.Split(';')
                    .Where(i => !i.StartsWith(convertToZeroDateTime))
                    .ToArray();
                connectionString = string.Join("", newData, "convertzerodatetime=true;");
            }

            return connectionString;
        }

        private static string[] splitConnectionString(string connectionString)
        {
            if (!connectionString.EndsWith(';')) connectionString += ';';
            
            var properties = connectionString.Split(';', StringSplitOptions.TrimEntries)
                .Select(i => i.ToLower())
                .ToArray();
            return properties;
        }
    }
}