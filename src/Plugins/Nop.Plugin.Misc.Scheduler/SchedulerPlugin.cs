using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.Scheduler
{
    public class SchedulerPlugin : BasePlugin
    {
        private readonly IWebHelper _webHelper;
        public SchedulerPlugin(IWebHelper webHelper)
        {
            _webHelper = webHelper;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/BackupScheduler/Configure";
        }

        public override Task InstallAsync()
        {
            return base.InstallAsync();
        }
    }
}