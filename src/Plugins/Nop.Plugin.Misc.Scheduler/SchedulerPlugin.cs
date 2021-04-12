using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.Scheduler.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.Scheduler
{
    public class SchedulerPlugin : BasePlugin, IMiscPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public SchedulerPlugin(IWebHelper webHelper,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            _webHelper = webHelper;
            _settingService = settingService;
            _localizationService = localizationService;
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
            await base.UninstallAsync();
        }
    }
}