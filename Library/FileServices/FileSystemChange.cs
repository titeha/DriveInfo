using System.IO;

namespace FileServices
{
  /// <summary>
  /// Вид изменения файловой системы, переданного наблюдателем
  /// </summary>
  public enum FileSystemChangeKind
  {
    /// <summary>Создан файл или каталог</summary>
    Created,

    /// <summary>Удалён файл или каталог</summary>
    Deleted,

    /// <summary>Изменён файл или каталог (размер, запись, атрибуты)</summary>
    Changed,

    /// <summary>Файл или каталог переименован</summary>
    Renamed,

    /// <summary>
    /// Наблюдатель потерял часть событий (переполнение буфера или иная ошибка слежения).
    /// Потребитель должен пересканировать затронутое поддерево, а не доверять дельте.
    /// </summary>
    Overflow
  }

  /// <summary>
  /// Доменное событие изменения файловой системы — независимое от WinAPI/FileSystemWatcher
  /// представление, которое отдаёт <see cref="IDriveWatcher"/>.
  /// </summary>
  public readonly struct FileSystemChange
  {
    public FileSystemChangeKind Kind { get; }

    /// <summary>Полный путь затронутого элемента. Для <see cref="FileSystemChangeKind.Overflow"/> — null.</summary>
    public string? FullPath { get; }

    /// <summary>Прежний полный путь — только для <see cref="FileSystemChangeKind.Renamed"/>, иначе null.</summary>
    public string? OldFullPath { get; }

    public FileSystemChange(FileSystemChangeKind kind, string? fullPath, string? oldFullPath = null)
    {
      Kind = kind;
      FullPath = fullPath;
      OldFullPath = oldFullPath;
    }

    /// <summary>
    /// Преобразует аргументы события FileSystemWatcher в доменное изменение.
    /// Переименование (<see cref="RenamedEventArgs"/>) сохраняет и новый, и прежний путь.
    /// </summary>
    internal static FileSystemChange From(FileSystemEventArgs args) => args switch
    {
      RenamedEventArgs _renamed => new FileSystemChange(FileSystemChangeKind.Renamed, _renamed.FullPath, _renamed.OldFullPath),
      _ => new FileSystemChange(Map(args.ChangeType), args.FullPath)
    };

    /// <summary>Сигнал о потере событий (переполнение буфера / ошибка наблюдателя).</summary>
    internal static FileSystemChange Overflowed() => new FileSystemChange(FileSystemChangeKind.Overflow, null);

    private static FileSystemChangeKind Map(WatcherChangeTypes changeType) => changeType switch
    {
      WatcherChangeTypes.Created => FileSystemChangeKind.Created,
      WatcherChangeTypes.Deleted => FileSystemChangeKind.Deleted,
      WatcherChangeTypes.Renamed => FileSystemChangeKind.Renamed,
      _ => FileSystemChangeKind.Changed
    };
  }
}
