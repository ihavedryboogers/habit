using System.Text.Json;

namespace Habit;

public sealed class Store
{
    private readonly string _path;

    public HabitData Data { get; private set; }

    public Store(string? path = null)
    {
        _path = path ?? DefaultPath();
        Data = Load(_path);
    }

    public static string DefaultPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var dir = Path.Combine(home, ".habit");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "data.json");
    }

    private static HabitData Load(string path)
    {
        if (!File.Exists(path))
            return new HabitData();

        var json = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json))
            return new HabitData();

        return JsonSerializer.Deserialize(json, HabitJsonContext.Default.HabitData) ?? new HabitData();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Data, HabitJsonContext.Default.HabitData);
        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, json);
        File.Move(tmp, _path, overwrite: true);
    }

    public HabitEntry? Find(string name) =>
        Data.Habits.FirstOrDefault(h => string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase));
}
