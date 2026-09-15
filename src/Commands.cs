namespace Habit;

public static class Commands
{
    public static void Add(Store store, string name)
    {
        if (store.Find(name) is not null)
        {
            Console.WriteLine($"Привычка «{name}» уже существует.");
            return;
        }

        store.Data.Habits.Add(new HabitEntry
        {
            Name = name,
            CreatedAt = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd"),
        });
        store.Save();
        Console.WriteLine($"Добавлена привычка «{name}».");
    }

    public static void Done(Store store, DateOnly today, string name, DateOnly date)
    {
        var habit = FindOrThrow(store, name);
        var dateStr = date.ToString("yyyy-MM-dd");

        if (!habit.Dates.Contains(dateStr))
        {
            habit.Dates.Add(dateStr);
            habit.Dates.Sort(StringComparer.Ordinal);
            store.Save();
        }

        var streak = Streak.Current(habit, today);
        Console.WriteLine($"✓ {habit.Name} — {dateStr} (стрик: {streak})");
    }

    public static void Undone(Store store, string name, DateOnly date)
    {
        var habit = FindOrThrow(store, name);
        var dateStr = date.ToString("yyyy-MM-dd");

        if (habit.Dates.Remove(dateStr))
        {
            store.Save();
            Console.WriteLine($"Отметка снята: {habit.Name} — {dateStr}");
        }
        else
        {
            Console.WriteLine($"Отметки на {dateStr} и не было.");
        }
    }

    public static void Remove(Store store, string name)
    {
        var habit = FindOrThrow(store, name);
        store.Data.Habits.Remove(habit);
        store.Save();
        Console.WriteLine($"Удалена привычка «{habit.Name}».");
    }

    public static void List(Store store, DateOnly today)
    {
        if (store.Data.Habits.Count == 0)
        {
            Console.WriteLine("Пока нет привычек. Добавьте: habit add \"Название\"");
            return;
        }

        var todayStr = today.ToString("yyyy-MM-dd");
        foreach (var habit in store.Data.Habits)
        {
            var done = habit.Dates.Contains(todayStr);
            var mark = done ? "✓" : "·";
            var streak = Streak.Current(habit, today);
            var streakText = streak > 0 ? $"стрик: {streak}" : "стрик: 0";
            Console.WriteLine($"{mark} {habit.Name,-24} {streakText}");
        }
    }

    public static void Help()
    {
        Console.WriteLine("""
        habit — трекер привычек

        Использование:
          habit                     показать список привычек на сегодня
          habit add <название>      добавить привычку
          habit done <название> [дата]     отметить выполненной (по умолчанию — сегодня)
          habit undone <название> [дата]   снять отметку
          habit remove <название>   удалить привычку
          habit list                показать список привычек
          habit help                эта справка

        Дата указывается в формате yyyy-MM-dd.
        Данные хранятся в ~/.habit/data.json
        """);
    }

    private static HabitEntry FindOrThrow(Store store, string name)
    {
        return store.Find(name)
            ?? throw new UsageException($"Привычка «{name}» не найдена.");
    }
}
