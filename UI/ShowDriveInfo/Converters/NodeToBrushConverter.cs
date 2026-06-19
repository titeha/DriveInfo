using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using ShowDriveInfo.Models;

namespace ShowDriveInfo.Converters
{
  /// <summary>Подбирает цвет текста узла дерева по его категории.</summary>
  public sealed class NodeToBrushConverter : IValueConverter
  {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is not IFileSystemNode _node)
        return Brushes.Black;

      return NodeCategorizer.Categorize(_node) switch
      {
        FileSystemNodeCategory.AccessDenied => Brushes.DarkOrange,
        FileSystemNodeCategory.Junction => Brushes.MediumPurple,
        FileSystemNodeCategory.System => Brushes.IndianRed,
        FileSystemNodeCategory.Hidden => Brushes.Gray,
        FileSystemNodeCategory.Compressed => Brushes.SteelBlue,
        _ => Brushes.Black
      };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
      throw new NotSupportedException();
  }
}
