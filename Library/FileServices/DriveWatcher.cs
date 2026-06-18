using System;
using System.IO;

namespace FileServices
{
  /// <summary>
  /// Наблюдатель за изменениями тома поверх <see cref="FileSystemWatcher"/>.
  /// Транслирует сырые события в доменные <see cref="FileSystemChange"/>; событие Error
  /// (в т.ч. переполнение буфера) превращает в <see cref="FileSystemChangeKind.Overflow"/>,
  /// чтобы потребитель пересканировал поддерево.
  /// </summary>
  public sealed class DriveWatcher : IDriveWatcher
  {
    // 64 КБ — максимально допустимый буфер; чем больше, тем реже теряются события под нагрузкой.
    private const int _bufferSize = 64 * 1024;

    private readonly FileSystemWatcher _watcher;

    public event EventHandler<FileSystemChange>? Changed;

    public DriveWatcher(DriveInfo drive) : this(drive.Name) { }

    public DriveWatcher(string path)
    {
      _watcher = new FileSystemWatcher(path)
      {
        IncludeSubdirectories = true,
        InternalBufferSize = _bufferSize,
        NotifyFilter = NotifyFilters.FileName
                     | NotifyFilters.DirectoryName
                     | NotifyFilters.Size
                     | NotifyFilters.LastWrite
                     | NotifyFilters.Attributes
      };

      _watcher.Created += OnChanged;
      _watcher.Deleted += OnChanged;
      _watcher.Changed += OnChanged;
      _watcher.Renamed += OnRenamed;
      _watcher.Error += OnError;
    }

    public void Start() => _watcher.EnableRaisingEvents = true;

    public void Stop() => _watcher.EnableRaisingEvents = false;

    private void OnChanged(object sender, FileSystemEventArgs args) => Raise(FileSystemChange.From(args));

    private void OnRenamed(object sender, RenamedEventArgs args) => Raise(FileSystemChange.From(args));

    private void OnError(object sender, ErrorEventArgs args) => Raise(FileSystemChange.Overflowed());

    private void Raise(FileSystemChange change) => Changed?.Invoke(this, change);

    public void Dispose()
    {
      _watcher.Created -= OnChanged;
      _watcher.Deleted -= OnChanged;
      _watcher.Changed -= OnChanged;
      _watcher.Renamed -= OnRenamed;
      _watcher.Error -= OnError;
      _watcher.Dispose();
    }
  }
}
