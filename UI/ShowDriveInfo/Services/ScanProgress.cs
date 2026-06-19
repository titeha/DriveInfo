namespace ShowDriveInfo.Services
{
  /// <summary>Прогресс сканирования: сколько элементов обработано и текущий путь.</summary>
  public readonly record struct ScanProgress(int Count, string? CurrentPath);
}
