using System.IO;

namespace ShowDriveInfo.Models
{
  public sealed class DriveDetails
  {
    private readonly DriveType _driveType;

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

    /// <summary>Занятое место тома (точное значение от ОС).</summary>
    public ulong Used => Size - Free;

    /// <summary>Корень просканированного дерева (null, пока скан не выполнен).</summary>
    public DirectoryDetails? Root { get; internal set; }

    /// <summary>
    /// «Прочее/системное»: разница между занятым местом тома и просканированной суммой
    /// (защищённые области, метаданные ФС, недоступное).
    /// </summary>
    public ulong Other { get; internal set; }

    public DriveDetails(DriveInfo drive)
    {
      Name = drive.Name;
      VolumeLabel = drive.VolumeLabel;
      DriveFormat = drive.DriveFormat;
      _driveType = drive.DriveType;
      Size = (ulong)drive.TotalSize;
      Free = (ulong)drive.TotalFreeSpace;
      AvailableFree = (ulong)drive.AvailableFreeSpace;
    }
  }
}
