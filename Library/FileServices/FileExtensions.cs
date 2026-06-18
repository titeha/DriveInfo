using System.IO;

namespace FileServices
{
  public static class FileExtensions
  {
    /// <summary>
    /// Проверяет, помечен ли набор атрибутов как точка повторной обработки (reparse point)
    /// </summary>
    /// <param name="attributes">Атрибуты файла или каталога</param>
    /// <returns>True, если установлен атрибут ReparsePoint</returns>
    internal static bool IsReparsePoint(FileAttributes attributes) => (attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;

    /// <summary>
    /// Проверяет, является ли каталог точкой соединения (junction point)
    /// </summary>
    /// <param name="directory">Проверяемый каталог</param>
    /// <returns>True, если каталог — точка соединения</returns>
    public static bool IsJunction(this DirectoryInfo directory) => IsReparsePoint(directory.Attributes);
  }
}
