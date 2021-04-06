using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Scheduler.Models;
using Nop.Plugin.Misc.Scheduler.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

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

        public SchedulerController(BackupSchedulerSettings backupSchedulerSettings,
            IPermissionService permissionService,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            BackupService backupService)
        {
            _backupSchedulerSettings = backupSchedulerSettings;
            _permissionService = permissionService;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _backupService = backupService;
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
            var file = await _backupService.CreateBackup();
            if (!ModelState.IsValid) return await Configure();

            await updateSettings(model);
            _notificationService.SuccessNotification( await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return View("~/Plugins/Misc.Scheduler/Views/Configure.cshtml", model);
        }

        private async Task updateSettings(ConfigurationModel model)
        {
            _backupSchedulerSettings.ClientSecret = model.ClientSecret;
            _backupSchedulerSettings.ScheduleTime = model.ScheduleTime;
            await _settingService.SaveSettingAsync(_backupSchedulerSettings);
        }
    }
}