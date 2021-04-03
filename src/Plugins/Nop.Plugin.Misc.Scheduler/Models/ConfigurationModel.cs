using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Scheduler.Models
{
    public record ConfigurationModel: BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Misc.Scheduler.Fields.HelloTest")]
        public string HelloTest { get; set; }
        
        public bool AdditionalFeePercentage { get; set; }
        
    }
}