using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Scheduler.Models
{
    public record ConfigurationModel: BaseNopModel
    {
        public string HelloTest { get; set; }
        
        public bool AdditionalFeePercentage { get; set; }
        
    }
}