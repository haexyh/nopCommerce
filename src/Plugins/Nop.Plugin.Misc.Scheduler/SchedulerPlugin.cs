using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.Scheduler.Models;
using Nop.Plugin.Misc.Scheduler.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Misc.Scheduler
{
    public class SchedulerPlugin : BasePlugin, IMiscPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILogger _logger;
        private readonly BackupService _backupService;

        public SchedulerPlugin(IWebHelper webHelper,
            ISettingService settingService,
            ILocalizationService localizationService,
            IScheduleTaskService scheduleTaskService,
            ILogger logger,
            BackupService backupService)
        {
            _webHelper = webHelper;
            _settingService = settingService;
            _localizationService = localizationService;
            _scheduleTaskService = scheduleTaskService;
            _logger = logger;
            _backupService = backupService;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Scheduler/Configure";
        }

        /// <summary>
        /// Installs the plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            var settings = new BackupSchedulerSettings()
            {
                DataProviderType = DataProviderType.MySql,
                ScheduleTime = TimeSpan.FromHours(2),
                InstanceGuid = Guid.NewGuid(),
            };
            
            await _settingService.SaveSettingAsync(settings);
            await _localizationService.AddLocaleResourceAsync(createLocalization());
            await base.InstallAsync();
        }

        private IDictionary<string, string> createLocalization()
        {
            return new Dictionary<string, string>()
            {
                {"Plugins.Misc.Scheduler.Fields.Endpoint", "Endpoint"},
                {"Plugins.Misc.Scheduler.Fields.ApiKey", "API Key"},
                {"Plugins.Misc.Scheduler.Fields.ScheduleTime", "Backup time"},
                {"Plugins.Misc.Scheduler.Fields.DataProviderType", "Database type"},
            };
        }

        /// <summary>
        /// Uninstalls the plugin
        /// </summary>
        /// <returns></returns>
        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<BackupSchedulerSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.Scheduler");
            var tasks = await _scheduleTaskService.GetTaskByTypeAsync(typeof(BackupTask).FullName);
            if (tasks != default) await _scheduleTaskService.DeleteTaskAsync(tasks);
            await base.UninstallAsync();
        }
    }
}