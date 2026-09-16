namespace Habit;

public static class Interactive
{
    public static bool CanRun() => !Console.IsInputRedirected && !Console.IsOutputRedirected;

    public static void Run(Store store) => new Session(store).Run();

    private sealed class Session
    {
        private const int MinDays = 1;
        private const int MaxDays = 60;

        private readonly Store _store;
        private int _row;
        private int _col;
        private int _dayCount = 7;
        private bool _colorEnabled = true;
        private bool _moveMode;
        private bool _blinkOn;
        private int _prevLineCount;

        public Session(Store store) => _store = store;

        public void Run()
        {
            Console.Clear();
            Console.CursorVisible = false;
            try
            {
                Render();
                while (true)
                {
                    var key = WaitForKey(_moveMode ? 400 : (int?)null);
                    if (key is null)
                    {
                        _blinkOn = !_blinkOn;
                        Render();
                        continue;
                    }

                    if (!Handle(key.Value))
                        return;

                    Render();
                }
            }
            finally
            {
                Console.CursorVisible = true;
                Console.Clear();
            }
        }

        private static ConsoleKey? WaitForKey(int? timeoutMs)
        {
            if (timeoutMs is null)
                return Console.ReadKey(intercept: true).Key;

            var deadline = Environment.TickCount64 + timeoutMs.Value;
            while (Environment.TickCount64 < deadline)
            {
                if (Console.KeyAvailable)
                    return Console.ReadKey(intercept: true).Key;
                Thread.Sleep(20);
            }
            return null;
        }

        private bool Handle(ConsoleKey key)
        {
            var habits = _store.Data.Habits;
            ClampSelection(habits);

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (_moveMode) MoveHabit(-1);
                    else if (habits.Count > 0) _row = Math.Max(0, _row - 1);
                    break;

                case ConsoleKey.DownArrow:
                    if (_moveMode) MoveHabit(1);
                    else if (habits.Count > 0) _row = Math.Min(habits.Count - 1, _row + 1);
                    break;

                case ConsoleKey.LeftArrow:
                    _col = Math.Max(0, _col - 1);
                    break;

                case ConsoleKey.RightArrow:
                    _col = Math.Min(_dayCount - 1, _col + 1);
                    break;

                case ConsoleKey.Enter:
                    if (_moveMode) _moveMode = false;
                    else if (habits.Count > 0) ToggleSelected();
                    break;

                case ConsoleKey.E:
                    if (habits.Count > 0) SetSelected(DayStatus.Empty);
                    break;

                case ConsoleKey.M:
                    if (habits.Count > 1) _moveMode = !_moveMode;
                    break;

                case ConsoleKey.T:
                    _colorEnabled = !_colorEnabled;
                    break;

                case ConsoleKey.D:
                    PromptDayCount();
                    break;

                case ConsoleKey.A:
                    PromptAddHabit();
                    break;

                case ConsoleKey.Delete:
                    if (habits.Count > 0) PromptRemoveHabit(habits[_row]);
                    break;

                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    return false;
            }

            return true;
        }

        private DateOnly ColumnDate(DateOnly today) => today.AddDays(_col - (_dayCount - 1));

        private void ToggleSelected()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var habit = _store.Data.Habits[_row];
            var date = ColumnDate(today);
            var next = Streak.Status(habit, date) == DayStatus.Done ? DayStatus.Failed : DayStatus.Done;
            Streak.SetStatus(habit, date, next);
            _store.Save();
        }

        private void SetSelected(DayStatus status)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            Streak.SetStatus(_store.Data.Habits[_row], ColumnDate(today), status);
            _store.Save();
        }

        private void MoveHabit(int direction)
        {
            var habits = _store.Data.Habits;
            var target = _row + direction;
            if (target < 0 || target >= habits.Count)
                return;

            (habits[_row], habits[target]) = (habits[target], habits[_row]);
            _row = target;
            _store.Save();
        }

        private void ClampSelection(List<HabitEntry> habits)
        {
            _row = habits.Count > 0 ? Math.Clamp(_row, 0, habits.Count - 1) : 0;
            _col = Math.Clamp(_col, 0, _dayCount - 1);
        }

        private void PromptDayCount()
        {
            RunPrompt($"Сколько дней показывать ({MinDays}-{MaxDays}): ", input =>
            {
                if (int.TryParse(input, out var value))
                    _dayCount = Math.Clamp(value, MinDays, MaxDays);
            });
        }

        private void PromptAddHabit()
        {
            RunPrompt("Название новой привычки: ", input =>
            {
                var name = input.Trim();
                if (string.IsNullOrWhiteSpace(name) || _store.Find(name) is not null)
                    return;

                _store.Data.Habits.Add(new HabitEntry
                {
                    Name = name,
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd"),
                });
                _store.Save();
                _row = _store.Data.Habits.Count - 1;
            });
        }

        private void PromptRemoveHabit(HabitEntry habit)
        {
            RunPrompt($"Удалить «{habit.Name}»? (y/n): ", input =>
            {
                if (input.Trim() is "y" or "Y")
                {
                    _store.Data.Habits.Remove(habit);
                    _store.Save();
                }
            });
        }

        private void RunPrompt(string label, Action<string> apply)
        {
            Console.CursorVisible = true;
            Console.Clear();
            Console.Write(label);
            var input = Console.ReadLine() ?? "";
            Console.CursorVisible = false;
            apply(input);
            Console.Clear();
            _prevLineCount = 0;
        }

        private void Render()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var habits = _store.Data.Habits;
            ClampSelection(habits);

            var y = 0;
            WriteLine(y++, () =>
            {
                Console.Write($"habit — {today:yyyy-MM-dd}");
                if (_moveMode)
                {
                    if (_colorEnabled) Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("  [ПЕРЕМЕЩЕНИЕ: ↑↓ двигают привычку]");
                    Console.ResetColor();
                }
            });
            WriteLine(y++, () => { });

            if (habits.Count == 0)
            {
                WriteLine(y++, () => Console.Write("Пока нет привычек. Нажмите 'a', чтобы добавить."));
                WriteLine(y++, () => { });
            }
            else
            {
                var nameWidth = Math.Max(12, habits.Max(h => h.Name.Length) + 1);

                WriteLine(y++, () =>
                {
                    Console.Write(new string(' ', nameWidth + 2));
                    for (var c = 0; c < _dayCount; c++)
                        Console.Write($"{today.AddDays(c - (_dayCount - 1)).Day,3}");
                    Console.Write("   Стрик");
                });

                for (var r = 0; r < habits.Count; r++)
                {
                    var habit = habits[r];
                    var isSelectedRow = r == _row;
                    var blinking = isSelectedRow && _moveMode && _blinkOn;

                    WriteLine(y++, () =>
                    {
                        Console.Write(isSelectedRow ? "> " : "  ");

                        if (blinking && _colorEnabled)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        Console.Write(!_colorEnabled && blinking
                            ? ("*" + habit.Name).PadRight(nameWidth)
                            : habit.Name.PadRight(nameWidth));
                        Console.ResetColor();

                        for (var c = 0; c < _dayCount; c++)
                        {
                            var date = today.AddDays(c - (_dayCount - 1));
                            DrawCell(Streak.Status(habit, date), isSelectedRow && c == _col);
                        }

                        Console.Write($"   {Streak.Current(habit, today)}");
                    });
                }

                WriteLine(y++, () => { });
            }

            WriteLine(y++, () =>
            {
                if (_colorEnabled) Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("↑↓ привычка  ←→ день  Enter отметить  e очистить  m двигать  t цвет  d кол-во дней  a добавить  Delete удалить  q выход");
                Console.ResetColor();
            });

            for (var extra = y; extra < _prevLineCount; extra++)
                ClearLine(extra);

            _prevLineCount = y;
        }

        private void DrawCell(DayStatus status, bool selected)
        {
            var glyph = status switch
            {
                DayStatus.Done => "✓",
                DayStatus.Failed => "✗",
                _ => "·",
            };

            if (!_colorEnabled)
            {
                Console.Write(selected ? $"[{glyph}]" : $" {glyph} ");
                return;
            }

            if (selected)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else if (status == DayStatus.Done)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (status == DayStatus.Failed)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.Write($" {glyph} ");
            Console.ResetColor();
        }

        private static void WriteLine(int row, Action content)
        {
            Console.SetCursorPosition(0, row);
            content();
            Console.Write("\x1b[K");
        }

        private static void ClearLine(int row)
        {
            Console.SetCursorPosition(0, row);
            Console.Write("\x1b[K");
        }
    }
}
