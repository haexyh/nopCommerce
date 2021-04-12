using System;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Misc.Scheduler.Services
{
    public class BackupTask: IScheduleTask
    {
        private readonly BackupService _backupService;
        private readonly ILogger _logger;

        public BackupTask(BackupService backupService, ILogger logger)
        {
            _backupService = backupService;
            _logger = logger;
        }
        public async Task ExecuteAsync()
        {
            try
            {
                await _backupService.CreateBackup();
                await _logger.InformationAsync("Backup Successfuly created");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await _logger.ErrorAsync("Backup error", e);
            }


        }
    }
}