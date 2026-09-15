namespace Habit;

public static class Streak
{
    public static int Current(HabitEntry habit, DateOnly today)
    {
        var dates = ParsedDates(habit);
        var cursor = today;
        if (!dates.Contains(cursor))
            cursor = cursor.AddDays(-1);

        int streak = 0;
        while (dates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }
        return streak;
    }

    public static HashSet<DateOnly> ParsedDates(HabitEntry habit) =>
        habit.Dates.Select(DateOnly.Parse).ToHashSet();
}
