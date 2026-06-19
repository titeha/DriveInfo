using ShowDriveInfo.Models;

namespace ShowDriveInfo.Converters
{
  /// <summary>Категория узла для подсветки в дереве.</summary>
  public enum FileSystemNodeCategory
  {
    Normal,
    Compressed,
    Hidden,
    System,
    Junction
  }

  /// <summary>
  /// Определяет категорию узла по его атрибутам. Чистая логика с явным приоритетом —
  /// вынесена отдельно для юнит-тестов.
  /// </summary>
  public static class NodeCategorizer
  {
    public static FileSystemNodeCategory Categorize(IFileSystemNode node)
    {
      if (node.IsJunction)
        return FileSystemNodeCategory.Junction;

      if (node.IsSystem)
        return FileSystemNodeCategory.System;

      if (node.IsHidden)
        return FileSystemNodeCategory.Hidden;

      if (node.IsCompressed)
        return FileSystemNodeCategory.Compressed;

      return FileSystemNodeCategory.Normal;
    }
  }
}
