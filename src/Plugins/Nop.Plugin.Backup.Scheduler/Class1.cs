using Nop.Core;
using Nop.Services.Plugins;

namespace Nop.Plugin.Backup.Scheduler
{
    public class Class1 : BasePlugin
    {
        private readonly IWebHelper _webHelper;

        public Class1(IWebHelper webHelper)
        {
            _webHelper = webHelper;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/BackupScheduler/Configure";
        }
    }
}