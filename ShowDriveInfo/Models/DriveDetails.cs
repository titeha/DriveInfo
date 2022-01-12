using System.Collections.Generic;
using System.IO;

namespace ShowDriveInfo.Models
{
 public class DriveDetails
 {
  private readonly DriveType _driveType;
  private readonly SortedSet<string> _hardLinks;

  public string Name { get; }

  public string VolumeLabel { get; init; }

  public string DriveFormat { get; init; }

  public string TypeOfDrive => _driveType switch
  {
   DriveType.NoRootDirectory => "Без корневого элемента",
   DriveType.Removable => "Переносимый",
   DriveType.Fixed => "Жесткий диск",
   DriveType.Network => "Сетевой",
   DriveType.CDRom => "CD Rom",
   DriveType.Ram => "Том в ОЗУ",
   DriveType.Unknown or _ => "Неизвестный",
  };

  public ulong Size { get; }

  public ulong Free { get; }

  public ulong AvailableFree { get; }

  public List<DirectoryDetails> Directories { get; }

  public List<FileDetails> Files { get; }

  public DriveDetails(DriveInfo drive)
  {
   Name = drive.Name;
   VolumeLabel = drive.VolumeLabel;
   DriveFormat = drive.DriveFormat;
   _driveType = drive.DriveType;
   Size = (ulong)drive.TotalSize;
   Free = (ulong)drive.TotalFreeSpace;
   AvailableFree = (ulong)drive.AvailableFreeSpace;
   Directories = new List<DirectoryDetails>();
   Files = new List<FileDetails>();
   _hardLinks = new SortedSet<string>();
  }
 }
}