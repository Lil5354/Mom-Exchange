using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class MonitoringSettingsViewModel
    {
        public bool EnableSystemMonitoring { get; set; }
        public bool EnableErrorTracking { get; set; }
        public bool EnablePerformanceMonitoring { get; set; }
        public bool EnableUserActivityLogging { get; set; }
        
        [Range(1, 365, ErrorMessage = "Thời gian lưu log phải từ 1 đến 365 ngày")]
        public int LogRetentionDays { get; set; }
        
        [Range(1, 1000, ErrorMessage = "Số lượng log tối đa phải từ 1 đến 1000")]
        public int MaxLogEntries { get; set; }
        
        public bool EnableRealTimeLogging { get; set; }
        public bool EnableLogCompression { get; set; }
        
        public string LogLevel { get; set; } // "Debug", "Info", "Warning", "Error"
        
        public bool EnableEmailAlerts { get; set; }
        public string AlertEmailAddress { get; set; }
        
        public bool EnableDiskSpaceMonitoring { get; set; }
        public int DiskSpaceThresholdPercent { get; set; }
        
        public bool EnableMemoryMonitoring { get; set; }
        public int MemoryThresholdPercent { get; set; }
        
        public bool EnableCPUMonitoring { get; set; }
        public int CPUThresholdPercent { get; set; }
        
        public DateTime LastSystemCheck { get; set; }
        public string SystemHealthStatus { get; set; }
        
        public List<SystemLog> RecentLogs { get; set; }
        
        public MonitoringSettingsViewModel()
        {
            RecentLogs = new List<SystemLog>();
        }
    }
    
    public class SystemLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string UserId { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
    }
}






