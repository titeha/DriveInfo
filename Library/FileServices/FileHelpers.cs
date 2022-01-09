using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;

using static ImportWinAPI.Imports;

namespace FileServices
{
 public class FileHelpers
 {
  private uint _clusterSize = 0;
  private const ulong _MFTFielSize = 680;
  private StringBuilder _sbPath = new StringBuilder(MAX_PATH);

  /// <summary>
  /// Получение размера кластер на диске (томе)
  /// </summary>
  /// <param name="drive">Исследуемый том</param>
  /// <returns>Значение размера кластера</returns>
  public uint GetClusterSize(DriveInfo drive)
  {
   if (0 == _clusterSize)
   {
	string _driveName = drive.Name;

	using var _searcher = new ManagementObjectSearcher($"select BlockSize, NumbersOfBlocks from Win32_volume where DriveLetter = '{_driveName.TrimEnd('\\')}'");
	foreach (ManagementObject _item in _searcher.Get())
	{
	 _clusterSize = (uint)_item["BlockSize"];
	 break;
	}
   }

   return _clusterSize;
  }

  private DriveInfo GetDriveByFileInfo(FileInfo file) => new DriveInfo(Path.GetPathRoot(file.FullName) ?? throw new ArgumentException());

  /// <summary>
  /// Расчет занимаемого места на диске (томе) файлом с учетом размера кластера
  /// </summary>
  /// <param name="file">Исследуемый файл</param>
  /// <returns>Итоговый размер</returns>
  public ulong GetFileSizeOnVolume(FileInfo file)
  {
   ulong _size = GetRealFileSize(file);

   if (_MFTFielSize > _size)
	return _size;

   uint _clusterSize = GetClusterSize(GetDriveByFileInfo(file));
   return (_size + _clusterSize - 1) / _clusterSize * _clusterSize;
  }

  /// <summary>
  /// перечисляет все жесткие ссылки файла
  /// </summary>
  /// <param name="file">Исследуемый файл</param>
  /// <returns>Перечисление найденных жестких ссылок файла, либо пустое множество</returns>
  public IEnumerable<string> GetHardLinks(FileInfo file)
  {
   uint _charCount = (uint)_sbPath.Capacity;
   IntPtr _findHandle;
   string _volume = GetDriveByFileInfo(file).Name[0..^1];

   if (INVALID_HANDLE_NAME != (_findHandle = FindFirstFileName(file.FullName, 0, ref _charCount, _sbPath)))
   {
	do
	{
	 _sbPath.Insert(0, _volume);
	 yield return _sbPath.ToString();
	 _charCount = (uint)_sbPath.Capacity;
	} while (FindNextFileName(_findHandle, ref _charCount, _sbPath));

	FindClose(_findHandle);
   }
  }

  /// <summary>
  /// Получает размер файла функцией GetCompressedFileSize, фактическое значение занимаемое файлом на томе
  /// </summary>
  /// <param name="file">Имя исследуемого файла</param>
  /// <returns>Размер файла в байтах</returns>
  public ulong GetRealFileSize(FileInfo file)
  {
   uint _loSize = GetCompressedFileSize(file.FullName, out uint _hiSize);
   return _hiSize << 32 | _loSize;
  }
 }
}