using System.Text.Json.Serialization;

namespace Habit;

public sealed class HabitEntry
{
    public string Name { get; set; } = "";
    public string CreatedAt { get; set; } = "";

    // date (yyyy-MM-dd) -> "done" | "failed"; absent key means empty/unmarked
    public Dictionary<string, string> Marks { get; set; } = new();
}

public sealed class HabitData
{
    public List<HabitEntry> Habits { get; set; } = new();
}

[JsonSerializable(typeof(HabitData))]
public partial class HabitJsonContext : JsonSerializerContext
{
}
