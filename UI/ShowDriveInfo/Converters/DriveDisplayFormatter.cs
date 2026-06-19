namespace ShowDriveInfo.Converters
{
  /// <summary>
  /// Форматирование подписи диска: буква тома и метка (если есть). Чистая логика — под юнит-тесты.
  /// </summary>
  public static class DriveDisplayFormatter
  {
    public static string Format(string name, string? label)
    {
      string _disk = name.TrimEnd('\\');

      return string.IsNullOrWhiteSpace(label) ? _disk : $"{_disk} ({label})";
    }
  }
}
