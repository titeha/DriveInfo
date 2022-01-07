using System.IO;
using System.Management;

using static ImportWinAPI.Imports;

namespace File.Extensions
{
 public static class FileExtensions
 {
  private static readonly Dictionary<string, uint> _clustersSize = new Dictionary<string, uint>(DriveInfo.GetDrives().Count(d => d.IsReady));

  /// <summary>
  /// Получает размер файла функцией GetCompressedFileSize, фактическое значение занимаемое файлом на томе
  /// </summary>
  /// <param name="file">Имя исследуемого файла</param>
  /// <returns>Размер файла в байтах</returns>
  public static long CalculateCompressedFileSize(this FileInfo file)
  {
   uint _loSize = GetCompressedFileSize(file.FullName, out uint _hiSize);
   return (long)_hiSize << 32 | _loSize;
  }

  /// <summary>
  /// Получение размера кластер на диске (томе)
  /// </summary>
  /// <param name="drive">Исследуемый том</param>
  /// <returns>Значение размера кластера</returns>
  public static uint GetClusterSize(this DriveInfo drive)
  {
   string driveName = drive.Name;

   if (!_clustersSize.TryGetValue(driveName, out uint _clusterSize))
   {
	using var _searcher = new ManagementObjectSearcher($"select BlockSize, NumbersOfBlocks from Win32_volume where DriveLetter = '{driveName.TrimEnd('\\')}'");
	foreach (ManagementObject _item in _searcher.Get())
	{
	 _clusterSize = (uint)_item["BlockSize"];
	 break;
	}
	_clustersSize.Add(driveName, _clusterSize);
   }

   return _clusterSize;
  }

  /// <summary>
  /// Расчет занимаемого места на диске (томе) файлом с учетом размера кластера
  /// </summary>
  /// <param name="file">Исследуемый файл</param>
  /// <returns>Итоговый размер</returns>
  public static long GetFileSizeOnVolume(this FileInfo file)
  {
   long _size = file.CalculateCompressedFileSize();
   const uint _MFTRecordSize = 1024;

   if (_size < _MFTRecordSize - file.Name.Length)
	return _MFTRecordSize;
   else
   {
	var _root = Path.GetPathRoot(file.FullName);
	uint _clusterSize = new DriveInfo(_root ?? string.Empty).GetClusterSize();
	return (_size + _clusterSize - 1) / _clusterSize * _clusterSize;
   }
  }

  public static IEnumerable<string> GetHardLinks(this FileInfo file)
  {
   var _hardLinks = new List<string>();

  }

  private static DriveInfo GetDriveByFile(this FileInfo file)
  {
   var _root = Path.GetPathRoot(file.FullName);

  }

  /// <summary>
  /// Проверяет, является ли исследуемый каталог "точкой монтирования"
  /// </summary>
  /// <param name="directory">Исследуемый каталог</param>
  /// <returns>Истина, если каталог - точка монтирования</returns>
  public static bool IsJunction(this DirectoryInfo directory) => FileAttributes.ReparsePoint == (directory.Attributes & FileAttributes.ReparsePoint);
 }
}