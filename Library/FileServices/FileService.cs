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
      foreach (FileInfo _file in directory.EnumerateFiles())
      {
        FSDTO _result = new(_file.Name, directory.Name)
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

      foreach (var _directory in directory.EnumerateDirectories())
      {
        FSDTO _result = new(_directory.Name, directory.Name)
        {
          IsDirectory = true,
          IsJunction = _directory.IsJunction(),
          IsHidden = (FileAttributes.Hidden & _directory.Attributes) == FileAttributes.Hidden,
          IsSystem = (FileAttributes.System & _directory.Attributes) == FileAttributes.System
        };

        yield return _result;

        if (!_result.IsJunction)
          foreach (FSDTO _item in ScanDrive(_directory))
            yield return _item;
      }
    }
  }
}