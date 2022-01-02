using System;
using System.IO;

namespace File.Extensions
{
 public static class FileExtensions
 {
  public static bool IsJunction(this DirectoryInfo directory) => FileAttributes.ReparsePoint == directory.Attributes.ReparsePoint & FileAttributes.ReparsePoint;
 }
}