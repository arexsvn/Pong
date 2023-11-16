using System;
using System.Collections.Generic;

public static class TimeUtils
{
	readonly static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	public const int MINUTES_IN_HOUR = 60;
	public const int SECONDS_IN_DAY = 86400;

	public static bool withinTimeRange(long start, long end)
	{
		long time = currentTime;
		return start < time && time < end;
	}

	public static long currentTime
	{
		get { return Convert.ToInt64((DateTime.UtcNow - _epoch).TotalSeconds); }
	}

	public static double currentTimeDouble
	{
		get { return (DateTime.UtcNow - _epoch).TotalSeconds; }
	}

	public static long elapsedTime(long startTimeInSeconds)
	{
		return currentTime - startTimeInSeconds;
	}

	public static double elapsedTimeDouble(double startTimeInSeconds)
	{
		return currentTimeDouble - startTimeInSeconds;
	}

	public static long remainingTime(long endTimeInSeconds)
	{
		return endTimeInSeconds - currentTime;
	}
		
	public static string formatTime(long seconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
		string timeText = null;

		if (timeSpan.Days > 1)
		{
			timeText = string.Format("{0:D2}d:{1:D2}h:{2:D2}m", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes);
		}
		else
		{
			timeText = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		}

		return timeText;
	}

	public static string formatTimeDescriptive(long seconds, LocaleManager localeManager)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
		List<string> timeParts = new List<string>();

		if (timeSpan.Days > 1)
		{
			timeParts.Add(timeSpan.Days.ToString());
			timeParts.Add(localeManager.lookup("date_time_long_day_plural"));
		}
		else if (timeSpan.Days == 1)
		{
			timeParts.Add(timeSpan.Days.ToString());
			timeParts.Add(localeManager.lookup("date_time_long_day"));
		}

		if (timeSpan.Hours > 1)
		{
			timeParts.Add(timeSpan.Hours.ToString());
			timeParts.Add(localeManager.lookup("date_time_long_hour_plural"));
		}
		else if (timeSpan.Hours == 1)
		{
			timeParts.Add(timeSpan.Hours.ToString());
			timeParts.Add(localeManager.lookup("date_time_long_hour"));
		}

		if (timeSpan.Minutes > 1)
		{
			timeParts.Add(timeSpan.Minutes.ToString());
			timeParts.Add(localeManager.lookup("date_time_long_minute_plural"));
		}
		else if (timeSpan.Minutes == 1)
		{
			timeParts.Add(timeSpan.Minutes.ToString());
			timeParts.Add(localeManager.lookup("date_time_long_minute"));
		}

		if (timeSpan.Seconds > 1)
		{
			timeParts.Add(timeSpan.Seconds.ToString());
			timeParts.Add(localeManager.lookup("date_time_long_second_plural"));
		}
		else if (timeSpan.Seconds == 1)
		{
			timeParts.Add(timeSpan.Seconds.ToString());
			timeParts.Add(localeManager.lookup("date_time_long_second"));
		}

		return String.Join(" ", timeParts.ToArray());;
	}
}