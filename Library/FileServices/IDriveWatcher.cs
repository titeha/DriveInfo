using System;

namespace FileServices
{
  /// <summary>
  /// Источник событий изменения файловой системы для тома.
  /// Отдаёт «сырые» доменные события; решение, что пересчитывать и как обновлять UI
  /// (дебаунс, маршалинг в UI-поток), принимает потребитель в прикладном слое.
  /// </summary>
  public interface IDriveWatcher : IDisposable
  {
    /// <summary>Возникает при каждом изменении в наблюдаемом томе.</summary>
    event EventHandler<FileSystemChange>? Changed;

    /// <summary>Начать слежение.</summary>
    void Start();

    /// <summary>Остановить слежение.</summary>
    void Stop();
  }
}
