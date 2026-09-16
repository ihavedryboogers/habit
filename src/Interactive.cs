namespace Habit;

public static class Interactive
{
    private const int Days = 7;

    public static bool CanRun() => !Console.IsInputRedirected && !Console.IsOutputRedirected;

    public static void Run(Store store)
    {
        var row = 0;
        var col = Days - 1;

        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var habits = store.Data.Habits;

                if (habits.Count > 0)
                {
                    row = Math.Clamp(row, 0, habits.Count - 1);
                    col = Math.Clamp(col, 0, Days - 1);
                }

                Draw(habits, today, row, col);

                var key = Console.ReadKey(intercept: true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (habits.Count > 0) row = Math.Max(0, row - 1);
                        break;

                    case ConsoleKey.DownArrow:
                        if (habits.Count > 0) row = Math.Min(habits.Count - 1, row + 1);
                        break;

                    case ConsoleKey.LeftArrow:
                        col = Math.Max(0, col - 1);
                        break;

                    case ConsoleKey.RightArrow:
                        col = Math.Min(Days - 1, col + 1);
                        break;

                    case ConsoleKey.Enter:
                        if (habits.Count > 0)
                        {
                            var date = today.AddDays(col - (Days - 1));
                            Toggle(store, habits[row], date);
                        }
                        break;

                    case ConsoleKey.A:
                        AddHabit(store);
                        row = store.Data.Habits.Count - 1;
                        col = Days - 1;
                        break;

                    case ConsoleKey.D:
                        if (habits.Count > 0)
                            RemoveHabit(store, habits[row]);
                        break;

                    case ConsoleKey.Q:
                    case ConsoleKey.Escape:
                        return;
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
            Console.Clear();
        }
    }

    private static void Toggle(Store store, HabitEntry habit, DateOnly date)
    {
        var dateStr = date.ToString("yyyy-MM-dd");
        if (!habit.Dates.Remove(dateStr))
        {
            habit.Dates.Add(dateStr);
            habit.Dates.Sort(StringComparer.Ordinal);
        }
        store.Save();
    }

    private static void AddHabit(Store store)
    {
        Console.CursorVisible = true;
        Console.Clear();
        Console.Write("Название новой привычки: ");
        var name = Console.ReadLine()?.Trim();
        Console.CursorVisible = false;

        if (string.IsNullOrWhiteSpace(name) || store.Find(name) is not null)
            return;

        store.Data.Habits.Add(new HabitEntry
        {
            Name = name,
            CreatedAt = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd"),
        });
        store.Save();
    }

    private static void RemoveHabit(Store store, HabitEntry habit)
    {
        Console.CursorVisible = true;
        Console.Clear();
        Console.Write($"Удалить «{habit.Name}»? (y/n): ");
        var confirm = Console.ReadKey(intercept: true).KeyChar;
        Console.CursorVisible = false;

        if (confirm is 'y' or 'Y')
        {
            store.Data.Habits.Remove(habit);
            store.Save();
        }
    }

    private static void Draw(List<HabitEntry> habits, DateOnly today, int row, int col)
    {
        Console.Clear();
        Console.WriteLine($"habit — {today:yyyy-MM-dd}");
        Console.WriteLine();

        if (habits.Count == 0)
        {
            Console.WriteLine("Пока нет привычек. Нажмите 'a', чтобы добавить.");
            Console.WriteLine();
            PrintHint();
            return;
        }

        var nameWidth = Math.Max(12, habits.Max(h => h.Name.Length) + 1);

        Console.Write(new string(' ', nameWidth + 2));
        for (var c = 0; c < Days; c++)
        {
            var date = today.AddDays(c - (Days - 1));
            Console.Write($" {date.Day,2}");
        }
        Console.WriteLine("   Стрик");

        for (var r = 0; r < habits.Count; r++)
        {
            var habit = habits[r];
            var dates = Streak.ParsedDates(habit);

            Console.Write(r == row ? "> " : "  ");
            Console.Write(habit.Name.PadRight(nameWidth));

            for (var c = 0; c < Days; c++)
            {
                var date = today.AddDays(c - (Days - 1));
                var done = dates.Contains(date);
                var selected = r == row && c == col;

                Console.Write(" ");
                if (selected)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else if (done)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.Write($" {(done ? "✓" : "·")}");
                Console.ResetColor();
            }

            Console.WriteLine($"   {Streak.Current(habit, today)}");
        }

        Console.WriteLine();
        PrintHint();
    }

    private static void PrintHint()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("↑↓ выбрать привычку  ←→ выбрать день  Enter отметить  a добавить  d удалить  q выход");
        Console.ResetColor();
    }
}
