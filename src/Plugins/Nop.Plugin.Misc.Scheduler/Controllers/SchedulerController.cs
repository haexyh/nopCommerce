using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Tasks;
using Nop.Plugin.Misc.Scheduler.Models;
using Nop.Plugin.Misc.Scheduler.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Tasks;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Misc.Scheduler.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class SchedulerController : BasePluginController
    {
        private readonly BackupSchedulerSettings _backupSchedulerSettings;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly BackupService _backupService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILogger _logger;

        public SchedulerController(BackupSchedulerSettings backupSchedulerSettings,
            IPermissionService permissionService,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            BackupService backupService,
            IScheduleTaskService scheduleTaskService,
            ILogger logger)
        {
            _backupSchedulerSettings = backupSchedulerSettings;
            _permissionService = permissionService;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _backupService = backupService;
            _scheduleTaskService = scheduleTaskService;
            _logger = logger;
        }

        public async Task<IActionResult> Configure()
        {
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance) == false)
                return AccessDeniedView();

            var model = ConfigurationModel.FromSettings(_backupSchedulerSettings);
            return View("~/Plugins/Misc.Scheduler/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid) return await Configure();

            await updateSettings(model);

            const int totalSecondsOfDay = 86_400;
            var task = await _scheduleTaskService.GetTaskByTypeAsync(typeof(BackupTask).FullName);
            task ??= new ScheduleTask()
            {
                Enabled = true,
                Type = typeof(BackupTask).FullName,
                Name = "Backup Service it-suite.ch",
                Seconds = totalSecondsOfDay
            };
            var time = (DateTime.Now.Date + _backupSchedulerSettings.ScheduleTime).ToUniversalTime();
            if (!task.LastStartUtc.HasValue) task.LastStartUtc = time;
            else if(task.LastStartUtc < time.AddHours(1) &&  task.LastStartUtc > time.AddHours(-1) ) task.LastStartUtc = time;
                
            
            if (task.Id == default) await _scheduleTaskService.InsertTaskAsync(task);
            else await _scheduleTaskService.UpdateTaskAsync(task);


            //await _backupService.CreateBackupAsync();
            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return View("~/Plugins/Misc.Scheduler/Views/Configure.cshtml", model);
        }

        private async Task updateSettings(ConfigurationModel model)
        {
            _backupSchedulerSettings.ApiKey = model.ApiKey;
            _backupSchedulerSettings.ScheduleTime = model.ScheduleTime;
            _backupSchedulerSettings.Endpoint = model.Endpoint;
            _backupSchedulerSettings.DataProviderType = model.DataProviderType;
            await _settingService.SaveSettingAsync(_backupSchedulerSettings);
        }
    }
}