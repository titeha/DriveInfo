using System;
using System.Globalization;
using System.Windows.Data;

namespace ShowDriveInfo.Converters
{
  /// <summary>Конвертер байтов (ulong/long/int) в человекочитаемую строку для биндинга.</summary>
  public sealed class BytesToStringConverter : IValueConverter
  {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      ulong _bytes = value switch
      {
        ulong _u => _u,
        long _l => _l >= 0 ? (ulong)_l : 0,
        int _i => _i >= 0 ? (ulong)_i : 0,
        _ => 0
      };

      return ByteSizeFormatter.Format(_bytes);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
      throw new NotSupportedException();
  }
}
