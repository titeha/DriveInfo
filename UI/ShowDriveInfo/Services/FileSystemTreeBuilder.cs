using System;
using System.Collections.Generic;
using System.IO;

using ShowDriveInfo.Models;

namespace ShowDriveInfo.Services
{
  /// <summary>
  /// Собирает иерархию каталогов/файлов из плоского потока <see cref="ScanItem"/>.
  /// Чистая логика без ввода-вывода — поток должен приходить в порядке обхода
  /// (родитель раньше потомков, как отдаёт FileService.ScanDrive).
  /// </summary>
  public static class FileSystemTreeBuilder
  {
    /// <summary>
    /// Строит дерево с корнем <paramref name="rootPath"/> и суммирует размеры снизу вверх.
    /// Жёсткие ссылки на один и тот же файл учитываются в суммах один раз.
    /// </summary>
    public static DirectoryDetails Build(string rootPath, string rootName, IEnumerable<ScanItem> items)
    {
      var _root = new DirectoryDetails(rootName, rootPath);
      var _byPath = new Dictionary<string, DirectoryDetails>(StringComparer.OrdinalIgnoreCase)
      {
        [rootPath] = _root
      };
      var _seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      foreach (ScanItem _item in items)
      {
        // Родитель уже должен быть создан (поток идёт сверху вниз); сирот пропускаем.
        if (!_byPath.TryGetValue(_item.ParentPath, out DirectoryDetails? _parent))
          continue;

        string _fullPath = Path.Combine(_item.ParentPath, _item.Name);

        if (_item.IsDirectory)
        {
          var _dir = new DirectoryDetails(_item.Name, _fullPath)
          {
            Parent = _parent,
            IsHidden = _item.IsHidden,
            IsSystem = _item.IsSystem,
            IsJunction = _item.IsJunction
          };
          _byPath[_fullPath] = _dir;
          _parent.Add(_dir);
        }
        else
        {
          var _file = new FileDetails(_item.Name, _fullPath)
          {
            Size = _item.Size,
            OccupiedSize = _item.OccupiedSize,
            IsCompressed = _item.IsCompressed,
            IsHidden = _item.IsHidden,
            IsSystem = _item.IsSystem
          };
          _parent.Add(_file);

          // Дедуп: байты общего файла прибавляем к предкам лишь при первой встрече.
          if (_seen.Add(HardLinkKey(_fullPath, _item.HardLinks)))
            Accumulate(_parent, _item.Size, _item.OccupiedSize);
        }
      }

      return _root;
    }

    /// <summary>
    /// «Прочее/системное» = занятое место тома минус просканированная сумма (не уходит в минус).
    /// </summary>
    public static ulong ComputeOther(ulong usedSpace, ulong scannedOccupied) =>
      usedSpace > scannedOccupied ? usedSpace - scannedOccupied : 0;

    private static void Accumulate(DirectoryDetails? node, ulong size, ulong occupied)
    {
      for (; node is not null; node = node.Parent)
      {
        node.Size += size;
        node.OccupiedSize += occupied;
      }
    }

    /// <summary>
    /// Ключ группы жёстких ссылок — наименьший путь среди самого файла и всех его ссылок.
    /// Так все ссылки на один файл дают один ключ.
    /// </summary>
    private static string HardLinkKey(string fullPath, IReadOnlyCollection<string> hardLinks)
    {
      string _key = fullPath;

      foreach (string _link in hardLinks)
        if (string.Compare(_link, _key, StringComparison.OrdinalIgnoreCase) < 0)
          _key = _link;

      return _key.ToLowerInvariant();
    }
  }
}
