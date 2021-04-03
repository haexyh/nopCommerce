using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Scheduler.Models;
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

        public async Task<IActionResult> Configure()
        {
            // todo remove
            await Task.Delay(1);
            var model = new ConfigurationModel() {AdditionalFeePercentage = false, HelloTest = "hoi",};
            return View("~/Plugins/Misc.Scheduler/Views/Configure.cshtml", model);
        }
    }
}