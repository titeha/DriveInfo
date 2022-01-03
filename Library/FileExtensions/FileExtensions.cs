using System.IO;
using System.Management;

using static ImportWinAPI.Imports;

namespace File.Extensions
{
 public static class FileExtensions
 {
  private static Dictionary<string, uint> _clustersSize = new Dictionary<string, uint>(DriveInfo.GetDrives().Count(d => d.IsReady));
  public static bool IsJunction(this DirectoryInfo directory) => FileAttributes.ReparsePoint == (directory.Attributes & FileAttributes.ReparsePoint);

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

  public static long CalculateCompressedFileSize(this FileInfo file)
  {
   uint _loSize = GetCompressedFileSize(file.FullName, out uint _hiSize);
   return (long)_hiSize << 32 | _loSize;
  }

  public static long GetFileSizeOnVolume(this FileInfo file)
  {
   long _size = CalculateCompressedFileSize(file);
   const uint _MFTRecordSize = 1024;

   if (_size < _MFTRecordSize - file.Name.Length)
	return _MFTRecordSize;
   else
   {
	uint _clusterSize = new DriveInfo(file.Directory.Root.Name).GetClusterSize();
	return (_size + _clusterSize - 1) / _clusterSize * _clusterSize;
   }
  }
 }
}