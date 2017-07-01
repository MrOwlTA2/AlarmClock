using Constellation;
using Constellation.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AlarmClock
{
    public class Program : PackageBase
    {
        //Snooze time for default (in minutes)
        private const int DEFAULT_NUMBER_OF_SECONDS = 0;

        private Dictionary<string, AlarmClock> alarms = new Dictionary<string, AlarmClock>();

        static void Main(string[] args)
        {
            PackageHost.Start<Program>(args);
        }

        public override void OnStart()
        {
            PackageHost.LastStateObjectsReceived += (s, e) =>
            {
                foreach (StateObject so in e.StateObjects)
                {
                    // Re-Pusher “bêtement” les StateObjects
                    PackageHost.PushStateObject(so.Name, so.DynamicValue, so.Type, so.Metadatas, so.Lifetime);
                    alarms.Add(so.Name, (AlarmClock)so.DynamicValue);
                }
                PackageHost.WriteInfo("Package started. {0} alarm(s) loaded", alarms.Count);
            };

            Task.Factory.StartNew(() =>
            {
                while (PackageHost.IsRunning)
                {
                    try
                    {
                        foreach (string clockName in alarms.Keys.ToList())
                        {
                            AlarmClock alarm = alarms[clockName];
                            //Ring loop
                            if ((alarm.WakingDays == null) || (alarm.WakingDays.Count == 0))
                            {
                                if ((DateTime.Now.Hour == alarm.WakingHour) && (DateTime.Now.Minute == alarm.WakingMinute) && (alarm.IsRinging == false))
                                {
                                    alarm.IsRinging = true;
                                    PackageHost.PushStateObject<AlarmClock>("Alarm" + alarm.ClockName, alarm);
                                    DeleteAlarm(alarm.ClockName);
                                }

                            }
                            else
                            {
                                for (int i = 0; i < alarm.WakingDays.Count; i++)
                                {
                                    if ((int)DateTime.Now.DayOfWeek == alarm.WakingDays[i])
                                    {
                                        if ((DateTime.Now.Hour == alarm.WakingHour) && (DateTime.Now.Minute == alarm.WakingMinute) && (alarm.IsRinging == false))
                                        {
                                            alarm.IsRinging = true;
                                            PackageHost.PushStateObject<AlarmClock>("Alarm" + alarm.ClockName, alarm);
                                            PackageHost.WriteInfo("\n Ca a sonné, tu as pas entendu ?");
                                        }

                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        PackageHost.WriteError(ex.Message);
                    }

                    Thread.Sleep(1000);
                }
            });
        }

        /// <summary>
        /// Alarm
        /// </summary>
        /// <param name="clockName">Clock name</param>
        /// <param name="wakingHour">Waking hour</param>
        /// <param name="wakingMinute">Waking minute</param>
        /// <param name="wakingDays">Waking days</param>
        [MessageCallback]
        public void SetAlarm(string clockName, int wakingHour, int wakingMinute, List<int> wakingDays = null)
        {
            // creation de l'alarm
            var alarm = new AlarmClock()
            {
                ClockName = clockName,
                WakingHour = wakingHour,
                WakingMinute = wakingMinute,
                WakingDays = wakingDays,
                IsRinging = false,
                Snooze = false
            };
            //StateObjects publication 
            PublishAlarm(alarm);
        }

        /// <summary>
        /// Stop the alarm 
        /// </summary>
        /// <param name="alarmName">The name of alarm to stop</param>
        [MessageCallback]
        public void StopRinging(string alarmName)
        {
            if (alarms.ContainsKey(alarmName))
            {
                alarms[alarmName].IsRinging = false;
                PublishAlarm(alarms[alarmName]);
            }
            else
            {
                PackageHost.WriteError("Not exist");
            }
        }

        /// <summary>
        /// Snooze the alarm.
        /// </summary>
        [MessageCallback]
        public void SnoozeAlarm(string alarmName)
        {

            DateTime clock = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DEFAULT_NUMBER_OF_SECONDS);
            TimeSpan durationSnooze = new TimeSpan(0, PackageHost.GetSettingValue<int>("snooze"), DEFAULT_NUMBER_OF_SECONDS);
            DateTime snoozedClock = clock.Add(durationSnooze);


            var alarmSnoozed = new AlarmClock()
            {
                ClockName = alarmName,
                WakingHour = snoozedClock.Hour,
                WakingMinute = snoozedClock.Minute,
                WakingDays = null,
                IsRinging = false,
                Snooze = true,
                SnoozedTime = PackageHost.GetSettingValue<int>("snooze")

            };


            PackageHost.PushStateObject<AlarmClock>(alarmName + "Snoozed", alarmSnoozed);
            lock (alarms)
            {
                alarms[alarmName] = alarmSnoozed;
            }
        }

        /// <summary>
        /// Delete specified alarm
        /// </summary>
        /// <param name="alarmName">Name of the alarm</param>
        [MessageCallback]
        public void DeleteAlarm(string alarmName)
        {
            PackageHost.ControlManager.PurgeStateObjects("raspberrypi", "AlarmClock", "Alarm" + alarmName, "*");
            if (alarms.ContainsKey(alarmName))
            {
                lock (alarms)
                {
                    alarms.Remove(alarmName);
                }
            }
        }

        //Bonus
        /// <summary>
        /// Choose the time you want to sleep
        /// </summary>
        /// <param name="sleepingHour">Number of hours of sleep desired</param>
        /// <param name="sleepingMinute">and minutes ?</param>
        [MessageCallback]
        public void ChooseSleepingTime(int sleepingHour = 0, int sleepingMinute = 0)
        {
            //StateObject publication
            PackageHost.PushStateObject<SleepingTime>("Sleeping Time", new SleepingTime() { SleepingHour = sleepingHour, SleepingMinute = sleepingMinute, IsRinging = false });

            //(Optionnal) Inform about the choosen time
            PackageHost.WriteInfo("Vous voulez dormir {0} heures et {1} minutes", sleepingHour, sleepingMinute);

            //Calculation of wake-up time
            DateTime actualTime = DateTime.Now;
            TimeSpan duration = new TimeSpan(sleepingHour, sleepingMinute, DEFAULT_NUMBER_OF_SECONDS);
            DateTime clock = actualTime.Add(duration);
            PackageHost.WriteInfo("Heure de réveil : {0}", clock);

            while (DateTime.Now < clock)
            {
                Thread.Sleep(1000);
            }
            PackageHost.PushStateObject<SleepingTime>("Sleeping Time", new SleepingTime() { SleepingHour = sleepingHour, SleepingMinute = sleepingMinute, IsRinging = true });
            Thread.Sleep(1000);
            PackageHost.ControlManager.PurgeStateObjects("raspberrypi", "AlarmClock", "Sleeping Time", "*");
            PackageHost.WriteInfo("\n Ca a sonné, tu as pas entendu ?");
        }

        /// <summary>
        /// Publish the alarm
        /// </summary>
        /// <param name="alarm">The alarm to publish</param>
        private void PublishAlarm(AlarmClock alarm)
        {
            PackageHost.PushStateObject("Alarm" + alarm.ClockName, alarm);
            lock (alarms)
            {
                alarms[alarm.ClockName] = alarm;
            }
        }
    }
}
