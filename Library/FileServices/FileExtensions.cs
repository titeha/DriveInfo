using System.IO;

namespace FileServices
{
  public static class FileExtensions
  {
    /// <summary>
    /// ѕровер€ет, €вл€етс€ ли исследуемый каталог "точкой монтировани€"
    /// </summary>
    /// <param name="directory">»сследуемый каталог</param>
    /// <returns>»стина, если каталог - точка монтировани€</returns>
    public static bool IsJunction(this DirectoryInfo directory) => FileAttributes.ReparsePoint == (directory.Attributes & FileAttributes.ReparsePoint);
  }
}