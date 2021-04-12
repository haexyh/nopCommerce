using System;
using Nop.Core.Configuration;
using Nop.Data;

namespace Nop.Plugin.Misc.Scheduler
{
    public class BackupSchedulerSettings: ISettings
    {
        public DataProviderType  DataProviderType { get; set; } 
        public string ApiKey { get; set; }
        public string Endpoint { get; set; } 
        public TimeSpan ScheduleTime { get; set; }
        public Guid InstanceGuid { get; set; }
         
    }
}