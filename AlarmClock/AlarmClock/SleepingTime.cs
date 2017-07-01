using Constellation.Package;

namespace AlarmClock
{
    /// <summary>
    /// StateObject SleepingTime
    /// </summary>
    [StateObject]
    public class SleepingTime
    {
        /// <summary>
        /// Number of hour
        /// </summary>
        public int SleepingHour { get; set; }
        /// <summary>
        /// Minute
        /// </summary>
        public int SleepingMinute { get; set; }
        /// <summary>
        /// iSRinging    
        /// </summary>
        public bool IsRinging { get; set; }
    }

}
