using System;
using System.Collections.Generic;
using System.IO;

namespace FileServices
{
  public class FileService
  {
    private readonly DriveInfo _drive;
    private readonly FileHelpers _fileHelpers;

    public FileService(DriveInfo drive)
    {
      _drive = drive;
      _fileHelpers = new FileHelpers();
    }

    public IEnumerable<FSDTO> ScanDrive()
    {
      DirectoryInfo _currentDirectory = new DirectoryInfo(_drive.Name);
      return ScanDrive(_currentDirectory);
    }

    private IEnumerable<FSDTO> ScanDrive(DirectoryInfo directory)
    {
      foreach (FileInfo _file in SafeEnumerateFiles(directory))
      {
        FSDTO _result = new(_file.Name, directory.FullName)
        {
          Size = (ulong)_file.Length,
          VolumeSize = _fileHelpers.GetFileSizeOnVolume(_file),
          IsCompressed = (FileAttributes.Compressed & _file.Attributes) == FileAttributes.Compressed,
          IsHidden = (FileAttributes.Hidden & _file.Attributes) == FileAttributes.Hidden,
          IsSystem = (FileAttributes.System & _file.Attributes) == FileAttributes.System
        };
        if (!((directory.Attributes & FileAttributes.System) == FileAttributes.System || _result.IsSystem))
          _result.HardLinks = _fileHelpers.GetHardLinks(_file);

        yield return _result;
      }

      foreach (var _directory in SafeEnumerateDirectories(directory))
      {
        bool _junction = _directory.IsJunction();
        // В junction не заходим намеренно; остальные проверяем на доступность, чтобы пометить недоступные.
        bool _accessible = _junction || IsAccessible(_directory);

        FSDTO _result = new(_directory.Name, directory.FullName)
        {
          IsDirectory = true,
          IsJunction = _junction,
          IsAccessDenied = !_junction && !_accessible,
          IsHidden = (FileAttributes.Hidden & _directory.Attributes) == FileAttributes.Hidden,
          IsSystem = (FileAttributes.System & _directory.Attributes) == FileAttributes.System
        };

        yield return _result;

        if (!_junction && _accessible)
          foreach (FSDTO _item in ScanDrive(_directory))
            yield return _item;
      }
    }

    /// <summary>
    /// Проверяет, можно ли перечислить содержимое каталога (открытие хэндла проверяет права доступа).
    /// </summary>
    private static bool IsAccessible(DirectoryInfo directory)
    {
      try
      {
        using IEnumerator<FileSystemInfo> _probe = directory.EnumerateFileSystemInfos().GetEnumerator();
        _probe.MoveNext();
        return true;
      }
      catch (UnauthorizedAccessException)
      {
        return false;
      }
      catch (IOException)
      {
        return false;
      }
    }

    /// <summary>
    /// Перечисляет файлы каталога, молча пропуская каталог при отказе доступа или ошибке ввода-вывода,
    /// чтобы один защищённый каталог не обрывал обход всего диска.
    /// </summary>
    private static IEnumerable<FileInfo> SafeEnumerateFiles(DirectoryInfo directory) =>
      SafeEnumeration.Enumerate(directory.EnumerateFiles);

    /// <summary>
    /// Перечисляет подкаталоги каталога, молча пропуская каталог при отказе доступа или ошибке ввода-вывода.
    /// </summary>
    private static IEnumerable<DirectoryInfo> SafeEnumerateDirectories(DirectoryInfo directory) =>
      SafeEnumeration.Enumerate(directory.EnumerateDirectories);
  }
}