using System;
using Google.Protobuf.WellKnownTypes;
using Nop.Core.Configuration;
using Nop.Data;
using Nop.Plugin.Misc.Scheduler.Models;

namespace Nop.Plugin.Misc.Scheduler
{
    public class BackupSchedulerSettings: ISettings
    {
        public StorageProvider StorageProvider { get; set; }
        public DataProviderType  DataProviderType { get; set; } 
        public string ClientSecret { get; set; }
        public TimeSpan ScheduleTime { get; set; }
         
    }
}