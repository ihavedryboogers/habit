using Habit;

var store = new Store();
var today = DateOnly.FromDateTime(DateTime.Now);

if (args.Length == 0)
{
    if (Interactive.CanRun())
        Interactive.Run(store);
    else
        Commands.List(store, today);
    return 0;
}

var command = args[0];
var rest = args.Skip(1).ToArray();

try
{
    switch (command)
    {
        case "add":
            Commands.Add(store, RequireArg(rest, 0, "название привычки"));
            break;

        case "done":
            Commands.Done(store, today, RequireArg(rest, 0, "название привычки"), OptionalDate(rest, 1, today));
            break;

        case "fail":
            Commands.Fail(store, RequireArg(rest, 0, "название привычки"), OptionalDate(rest, 1, today));
            break;

        case "undone":
            Commands.Undone(store, RequireArg(rest, 0, "название привычки"), OptionalDate(rest, 1, today));
            break;

        case "remove":
            Commands.Remove(store, RequireArg(rest, 0, "название привычки"));
            break;

        case "list":
        case "ls":
            Commands.List(store, today);
            break;

        case "-h":
        case "--help":
        case "help":
            Commands.Help();
            break;

        default:
            Console.Error.WriteLine($"Неизвестная команда: {command}");
            Commands.Help();
            return 1;
    }
}
catch (UsageException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

return 0;

static string RequireArg(string[] rest, int index, string what)
{
    if (index >= rest.Length || string.IsNullOrWhiteSpace(rest[index]))
        throw new UsageException($"Нужно указать {what}.");
    return rest[index];
}

static DateOnly OptionalDate(string[] rest, int index, DateOnly fallback)
{
    if (index >= rest.Length || string.IsNullOrWhiteSpace(rest[index]))
        return fallback;

    if (!DateOnly.TryParse(rest[index], out var date))
        throw new UsageException($"Некорректная дата: {rest[index]} (ожидается yyyy-MM-dd).");

    return date;
}
