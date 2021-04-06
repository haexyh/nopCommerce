using System;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Services;

namespace Nop.Plugin.Misc.Scheduler.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
        }

        [NopResourceDisplayName("Plugins.Misc.Scheduler.Fields.StorageProvider")]
        public StorageProvider StorageProvider { get; set; } = StorageProvider.GoogleOne;

        //[NopResourceDisplayName("Plugins.Misc.Scheduler.Fields.StorageEndPoint")]
        //public string StorageEndPoint { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Scheduler.Fields.ClientSecret")]
        public string ClientSecret { get; set; }

        //public StorageProviderConfiguration StorageProviderConfiguration { get; set; }
        [NopResourceDisplayName("Plugins.Misc.Scheduler.Fields.ScheduleTime")]
        public TimeSpan ScheduleTime { get; set; }

        public static ConfigurationModel FromSettings(BackupSchedulerSettings settings)
        {
            return new ConfigurationModel()
            {
                StorageProvider = settings.StorageProvider,
                ClientSecret = settings.ClientSecret,
                ScheduleTime = settings.ScheduleTime
            };
        }
    }
}