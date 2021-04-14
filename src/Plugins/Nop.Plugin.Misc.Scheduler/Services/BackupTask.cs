using System;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Misc.Scheduler.Services
{
    public class BackupTask: IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly BackupService _backupService;

        public BackupTask( ILogger logger, BackupService backupService)
        {
            _logger = logger;
            _backupService = backupService;
        }
        public async Task ExecuteAsync()
        {
            
            try
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n\n\n\n\nHello");
                Console.ResetColor();
                await _backupService.CreateBackupAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await _logger.ErrorAsync("Backup error", e);
            }


        }
    }
}