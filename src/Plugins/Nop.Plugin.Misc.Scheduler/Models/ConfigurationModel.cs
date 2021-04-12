using System;
using Nop.Data;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Scheduler.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel() {}

        [NopResourceDisplayName("Plugins.Misc.Scheduler.Fields.Endpoint")]
        public string Endpoint { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Scheduler.Fields.ApiKey")]
        public string ApiKey { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Scheduler.Fields.ScheduleTime")]
        public TimeSpan ScheduleTime { get; set; }
        
        [NopResourceDisplayName("Plugins.Misc.Scheduler.Fields.DataProviderType")]
        public DataProviderType DataProviderType { get; set; }

        public static ConfigurationModel FromSettings(BackupSchedulerSettings settings)
        {
            return new ConfigurationModel()
            {
                DataProviderType = settings.DataProviderType,
                Endpoint = settings.Endpoint,
                ApiKey = settings.ApiKey,
                ScheduleTime = settings.ScheduleTime
            };
        }
    }
}