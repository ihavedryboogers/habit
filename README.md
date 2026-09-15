# habit

Консольный трекер привычек. Отмечайте полезные дела, сделанные за день, и следите за стриками — прямо из терминала.

## Установка

```sh
brew tap ihavedryboogers/habit
brew install habit
```

## Использование

```sh
habit add "Спорт"              # добавить привычку
habit done "Спорт"             # отметить выполненной сегодня
habit done "Спорт" 2026-09-14  # отметить выполненной за конкретную дату
habit undone "Спорт"           # снять отметку за сегодня
habit list                     # список привычек, статус на сегодня и текущий стрик
habit remove "Спорт"           # удалить привычку
habit                          # то же, что и `habit list`
```

Данные хранятся локально в `~/.habit/data.json`.

## Сборка из исходников

Нужен [.NET SDK 10](https://dotnet.microsoft.com/download).

```sh
dotnet build
dotnet run -- list
```

Публикация автономного бинарника:

```sh
dotnet publish -c Release -r osx-arm64   # или osx-x64
```
