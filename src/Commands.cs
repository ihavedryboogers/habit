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
        Streak.SetStatus(habit, date, DayStatus.Done);
        store.Save();

        var streak = Streak.Current(habit, today);
        Console.WriteLine($"✓ {habit.Name} — {date:yyyy-MM-dd} (стрик: {streak})");
    }

    public static void Fail(Store store, string name, DateOnly date)
    {
        var habit = FindOrThrow(store, name);
        Streak.SetStatus(habit, date, DayStatus.Failed);
        store.Save();
        Console.WriteLine($"✗ {habit.Name} — {date:yyyy-MM-dd}");
    }

    public static void Undone(Store store, string name, DateOnly date)
    {
        var habit = FindOrThrow(store, name);
        Streak.SetStatus(habit, date, DayStatus.Empty);
        store.Save();
        Console.WriteLine($"Отметка снята: {habit.Name} — {date:yyyy-MM-dd}");
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

        foreach (var habit in store.Data.Habits)
        {
            var mark = Streak.Status(habit, today, today) switch
            {
                DayStatus.Done => "✓",
                DayStatus.Failed => "✗",
                _ => "·",
            };
            var streak = Streak.Current(habit, today);
            Console.WriteLine($"{mark} {habit.Name,-24} стрик: {streak}");
        }
    }

    public static void Help()
    {
        Console.WriteLine("""
        habit — трекер привычек

        Использование:
          habit                     интерактивная таблица (стрелки + Enter)
          habit add <название>      добавить привычку
          habit done <название> [дата]     отметить выполненной (по умолчанию — сегодня)
          habit fail <название> [дата]     отметить осознанно пропущенной
          habit undone <название> [дата]   снять отметку (сделать пустой)
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
