using System;
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
                StorageProvider = StorageProvider.GoogleOne,
                DataProviderType = DataProviderType.MySql,
                ScheduleTime = TimeSpan.FromHours(2),
            };
            await _settingService.SaveSettingAsync(settings);
            // localiation
            // await _localizationService.AddLocalResourceAsync();
            await base.InstallAsync();
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