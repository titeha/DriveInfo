using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace FileServices
{
 public class FileService
 {
  private DriveInfo _drive;
  private FileHelpers _fileHelpers;

  public FileService(DriveInfo drive)
  {
   _drive = drive;
   _fileHelpers = new FileHelpers();
  }

  public IEnumerable<FSDTO> ScanDrive()
  {
   var _currentDirectory = new DirectoryInfo(_drive.Name);
   return ScanDrive(_currentDirectory);
  }

  private IEnumerable<FSDTO> ScanDrive(DirectoryInfo directory)
  {
   foreach (var _file in directory.EnumerateFiles())
   {
	FSDTO _result = new(_file.Name, directory.Name)
	{
	 Size = (ulong)_file.Length
	};
	if ((directory.Attributes & FileAttributes.System) != FileAttributes.System)
	 _result.VolumeSize = _fileHelpers.GetFileSizeOnVolume(_file);
	_result.IsCompressed = (FileAttributes.Compressed & _file.Attributes) == FileAttributes.Compressed;
	_result.IsHidden = (FileAttributes.Hidden & _file.Attributes) == FileAttributes.Hidden;
	_result.IsSystem = (FileAttributes.System & _file.Attributes) == FileAttributes.System;

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
	 foreach (var _item in ScanDrive(_directory))
	  yield return _item;
   }
  }
 }
}