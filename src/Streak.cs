namespace Habit;

public enum DayStatus
{
    Empty,
    Done,
    Failed,
}

public static class Streak
{
    public static DayStatus Status(HabitEntry habit, DateOnly date)
    {
        if (habit.Marks.TryGetValue(date.ToString("yyyy-MM-dd"), out var value))
            return value == "failed" ? DayStatus.Failed : DayStatus.Done;

        return DayStatus.Empty;
    }

    public static void SetStatus(HabitEntry habit, DateOnly date, DayStatus status)
    {
        var key = date.ToString("yyyy-MM-dd");
        if (status == DayStatus.Empty)
            habit.Marks.Remove(key);
        else
            habit.Marks[key] = status == DayStatus.Failed ? "failed" : "done";
    }

    public static int Current(HabitEntry habit, DateOnly today)
    {
        var cursor = today;
        if (Status(habit, cursor) != DayStatus.Done)
            cursor = cursor.AddDays(-1);

        var streak = 0;
        while (Status(habit, cursor) == DayStatus.Done)
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }
        return streak;
    }
}
