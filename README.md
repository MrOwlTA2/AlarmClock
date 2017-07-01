**Alarm clock Package for Constellation**
----------

Set an alarm with Constellation.


**StateObjects**

 - Each alarms set are push as different StateObject with the name : *"Alarm" + name of alarm set*.
 - In these StateObjects, the boolean `IsRinging` becomes `true` when the alarm sounds.
 
**MessageCallbacks**

 - SetAlarm(clockName, wakingHour, wakingMinute, wakingDays) : Set an alarm for given name, hour, minutes and days. *(Caution : Constellation does not handle integer lists. You need to use a dashboard to set this for example with checkbox. To counter this, the default value is `null` and set a punctual alarm)*

 - StopRinging(alarmName) : Re-set the booelan `IsRinging` to false when you want to stop the alarm in progress.
 - SnoozeAlarm(alarmName) : Snooze the alarm in progress. The snooze time can be choose in settings on Constellation.
 - DeleteAlarm(alarmName) : Deletes the desired alarm.
 
 I add a bonus function : 
  - ChooseSleepingTime(sleepingHour, sleepingMinute) : Choose the time you want to sleep, for example for a nap.

**Installation**

You can set the desired snooze time :
     ```Markdown
     <package name="AlarmClock" enable="true" autoStart="true">
					<settings>
					<setting key="snooze" value="xx" />
				</settings>
				</package>
          ```
If you can't see this in the Configuration Editor, the default value is used.


