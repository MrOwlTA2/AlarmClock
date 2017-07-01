using Constellation.Package;
using System.Collections.Generic;

namespace AlarmClock
{
    /// <summary>
    /// StateObject Alarm
    /// </summary>
    [StateObject]
    public class AlarmClock
    {
        /// <summary>
        /// Name of alarm
        /// </summary>
        public string ClockName { get; set; }
        /// <summary>
        /// Hour
        /// </summary>
        public int WakingHour { get; set; }
        /// <summary>
        /// Minute
        /// </summary>
        public int WakingMinute { get; set; }
        /// <summary>
        /// Days
        /// </summary>
        public List<int> WakingDays { get; set; }
        /// <summary>
        /// iSRinging    
        /// </summary>
        public bool IsRinging { get; set; }
        /// <summary>
        /// Snooze   
        /// </summary>
        public bool Snooze { get; set; }
        /// <summary>
        /// Snooze   
        /// </summary>
        public int SnoozedTime { get; set; }
    }
}
