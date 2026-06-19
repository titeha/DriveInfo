using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace ShowDriveInfo.Converters
{
  /// <summary>Подпись диска для списка: буква + метка тома.</summary>
  public sealed class DriveToStringConverter : IValueConverter
  {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is not DriveInfo _drive)
        return string.Empty;

      string? _label = null;
      try { _label = _drive.VolumeLabel; }
      catch { /* у некоторых томов метка недоступна */ }

      return DriveDisplayFormatter.Format(_drive.Name, _label);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
      throw new NotSupportedException();
  }
}
