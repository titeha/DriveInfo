using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace FileServices
{
 public sealed class FileHelpers
 {
  #region Imports
  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool FindClose(IntPtr hFindFile);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  private static extern IntPtr FindFirstFileName(string lpFileName, uint dwFlags, ref uint StringLength, StringBuilder LinkName);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  private static extern bool FindNextFileName(IntPtr hFindStream, ref uint StringLength, StringBuilder LinkName);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  private static extern uint GetCompressedFileSize(string lpFileName, out uint lpFileSizeHight);

  internal static readonly IntPtr INVALID_HANDLE_NAME = (IntPtr)(-1);
  internal const int MAX_PATH = 65535; // Max. NTFS path length
  #endregion

  private uint _clusterSize;
  private const ulong _MFTFileSize = 680;
  private readonly StringBuilder _sbPath = new(MAX_PATH);

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

  private static DriveInfo GetDriveByFileInfo(FileInfo file) => new(Path.GetPathRoot(file.FullName)
   ?? throw new ArgumentException($"Неверное имя файла {file.FullName}"));

  /// <summary>
  /// Расчет занимаемого места на диске (томе) файлом с учетом размера кластера
  /// </summary>
  /// <param name="file">Исследуемый файл</param>
  /// <returns>Итоговый размер</returns>
  public ulong GetFileSizeOnVolume(FileInfo file)
  {
   ulong _size = GetRealFileSize(file);

   if (_MFTFileSize > _size)
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
	 yield return _sbPath.Insert(0, _volume).ToString();
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
  public static ulong GetRealFileSize(FileInfo file)
  {
   uint _loSize = GetCompressedFileSize(file.FullName, out uint _hiSize);
   return _hiSize << 32 | _loSize;
  }
 }
}