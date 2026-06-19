using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

using ResultType;

using StringFunctions;

namespace FileServices;

public sealed class FileHelpers
{
  #region Imports
  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool FindClose(IntPtr hFindFile);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  private static extern IntPtr FindFirstFileName(string lpFileName, uint dwFlags, ref uint StringLength, StringBuilder LinkName);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  private static extern bool FindNextFileName(IntPtr hFindStream, ref uint StringLength, StringBuilder LinkName);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  private static extern uint GetCompressedFileSize(string lpFileName, out uint lpFileSizeHight);

  internal static readonly IntPtr INVALID_HANDLE_NAME = (IntPtr)(-1);
  internal const int MAX_PATH = 65535; // Max. NTFS path length
  internal const uint INVALID_FILE_SIZE = 0xFFFFFFFF;
  internal const int NO_ERROR = 0;
  #endregion

  private const ulong _MFTFileSize = 680;
  private readonly Dictionary<string, uint> _clusterSizes = new();

  /// <summary>
  /// Форматирует имя тома, добавляя двоеточие если необходимо
  /// </summary>
  /// <param name="driveName">Имя тома (например, "C:\" или "C")</param>
  /// <returns>Имя тома в формате "C:"</returns>
  internal static Result<string> FormatVolumeName(string driveName)
  {
    if (driveName.IsNullOrEmpty())
      return Result.Failure<string>("Имя тома не может быть пустым");

    var trimmed = driveName.TrimEnd('\\');
    return Result.Success(trimmed.EndsWith(':') ? trimmed : trimmed + ":");
  }

  /// <summary>
  /// Получение размера кластер на диске (томе)
  /// </summary>
  /// <param name="drive">Исследуемый том</param>
  /// <returns>Значение размера кластера</returns>
  public uint GetClusterSize(DriveInfo drive)
  {
    string _key = drive.Name.TrimEnd('\\');

    // Кэшируем размер кластера по тому: один экземпляр FileHelpers может обслуживать файлы
    // с разных дисков, поэтому единственное поле здесь врало бы при смене тома.
    if (_clusterSizes.TryGetValue(_key, out uint _cached))
      return _cached;

    uint _clusterSize = 0;
    using var _searcher = new ManagementObjectSearcher($"select BlockSize from Win32_volume where DriveLetter = '{_key}'");
    foreach (ManagementObject _item in _searcher.Get().Cast<ManagementObject>())
    {
      _clusterSize = ParseBlockSize(_item["BlockSize"]);
      break;
    }

    _clusterSizes[_key] = _clusterSize;
    return _clusterSize;
  }

  /// <summary>
  /// Преобразует значение BlockSize из WMI в uint. В WMI Win32_Volume.BlockSize имеет тип uint64,
  /// поэтому прямое приведение упакованного ulong к uint бросает InvalidCastException — используем Convert.
  /// </summary>
  internal static uint ParseBlockSize(object? value) => value is null ? 0u : Convert.ToUInt32(value);

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
    return CalculateSizeOnVolume(_size, _clusterSize);
  }

  /// <summary>
  /// Вычисляет занимаемое файлом место на томе по его фактическому размеру и размеру кластера.
  /// Маленькие файлы хранятся резидентно в записи MFT и отдельного кластера не занимают.
  /// </summary>
  /// <param name="realSize">Фактический размер файла в байтах</param>
  /// <param name="clusterSize">Размер кластера тома в байтах</param>
  /// <returns>Размер, занимаемый файлом на томе</returns>
  internal static ulong CalculateSizeOnVolume(ulong realSize, uint clusterSize)
  {
    if (_MFTFileSize > realSize)
      return realSize;

    return RoundUpToCluster(realSize, clusterSize);
  }

  /// <summary>
  /// Округляет размер вверх до целого числа кластеров
  /// </summary>
  /// <param name="size">Размер в байтах</param>
  /// <param name="clusterSize">Размер кластера в байтах</param>
  /// <returns>Размер, кратный размеру кластера (или исходный размер, если кластер неизвестен)</returns>
  internal static ulong RoundUpToCluster(ulong size, uint clusterSize)
  {
    if (clusterSize == 0)
      return size;

    return (size + clusterSize - 1) / clusterSize * clusterSize;
  }

  /// <summary>
  /// Собирает 64-битный размер из старшего и младшего 32-битных слов, возвращаемых WinAPI
  /// </summary>
  /// <param name="highWord">Старшее 32-битное слово</param>
  /// <param name="lowWord">Младшее 32-битное слово</param>
  /// <returns>Размер в байтах</returns>
  internal static ulong CombineFileSize(uint highWord, uint lowWord) => (ulong)highWord << 32 | lowWord;

  /// <summary>
  /// перечисляет все жесткие ссылки файла
  /// </summary>
  /// <param name="file">Исследуемый файл</param>
  /// <returns>Перечисление найденных жестких ссылок файла, либо пустое множество</returns>
  public IEnumerable<string> GetHardLinks(FileInfo file)
  {
    // Локальный буфер на каждый вызов: общее поле было бы непотокобезопасным
    StringBuilder _sbPath = new(MAX_PATH);
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

    // INVALID_FILE_SIZE сам по себе может быть валидным младшим словом — поэтому
    // ошибкой считаем только когда GetLastError при этом не NO_ERROR.
    if (_loSize == INVALID_FILE_SIZE && Marshal.GetLastWin32Error() != NO_ERROR)
      return (ulong)file.Length;

    return CombineFileSize(_hiSize, _loSize);
  }
}