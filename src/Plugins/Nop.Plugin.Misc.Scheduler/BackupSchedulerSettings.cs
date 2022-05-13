using System;
using System.ComponentModel.DataAnnotations;
using Nop.Core.Configuration;
using Nop.Data;

namespace Nop.Plugin.Misc.Scheduler
{
    public class BackupSchedulerSettings: ISettings
    {
        [Required]
        public DataProviderType  DataProviderType { get; set; } 
        [Required]
        [MaxLength(40)]
        [MinLength(40)]
        public string ApiKey { get; set; }
        [Required]
        public string Endpoint { get; set; } 
        [Required]
        public TimeSpan ScheduleTime { get; set; }
        [Required]
        public string HostName { get; set; }
         
    }
}