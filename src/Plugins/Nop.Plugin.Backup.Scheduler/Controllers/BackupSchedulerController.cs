using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Backup.Scheduler.Controllers
{
    [AuthorizeAdmin()]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class BackupSchedulerController : BasePluginController
    {

        public async Task<IActionResult> Configure()
        {
            // todo remove
            await Task.Delay(1);
            var model = new Models.ConfigurationModel() {AdditionalFeePercentage = false, HelloTest = "hoi",};
            return View("~/Plugins/Backup.Scheduler/Views/Configure.cshtml", model);
        }
    }
}