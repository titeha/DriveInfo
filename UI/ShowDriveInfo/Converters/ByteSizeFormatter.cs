using System.Globalization;

namespace ShowDriveInfo.Converters
{
  /// <summary>
  /// Форматирование размера в байтах в человекочитаемый вид (Б/КБ/МБ/ГБ/ТБ/ПБ).
  /// Чистая логика — вынесена отдельно для юнит-тестов.
  /// </summary>
  public static class ByteSizeFormatter
  {
    private static readonly string[] _units = { "Б", "КБ", "МБ", "ГБ", "ТБ", "ПБ" };

    public static string Format(ulong bytes)
    {
      if (bytes < 1024)
        return $"{bytes} {_units[0]}";

      double _size = bytes;
      int _unit = 0;
      while (_size >= 1024 && _unit < _units.Length - 1)
      {
        _size /= 1024;
        _unit++;
      }

      return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", _size, _units[_unit]);
    }
  }
}
