using System.Runtime.InteropServices;
using System.Text;

namespace ImportWinAPI
{
 public static class Imports
 {
  #region Функции
  #region kernel32.dll
  [DllImport("kernel32.dll", SetLastError = true)]
  public static extern bool FindClose(IntPtr hFindFile);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  public static extern IntPtr FindFirstFileName(string lpFileName, uint dwFlags, ref uint StringLength, StringBuilder LinkName);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  public static extern bool FindNextFileName(IntPtr hFindStream, ref uint StringLength, StringBuilder LinkName);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  public static extern uint GetCompressedFileSize(string lpFileName, out uint lpFileSizeHight);

  [DllImport("kernel32.dll", ExactSpelling = true)]
  public static extern IntPtr GetCurrentProcess();

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  public static extern bool GetVolumePathName(string lpszFileName, [Out] StringBuilder lpszVolumePathName, uint cchBufferLength);
  #endregion

  #region advapi32.dll
  [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static extern bool LookupPrivilegeValue(string? lpSystemName, string lpName, out LUID lpLuid);

  [DllImport("advapi32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

  [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
  public static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TOKEN_PRIVILEGES_SINGLE newst, int len, IntPtr prev, IntPtr relen);
  #endregion
  #endregion

  #region Структуры
  [StructLayout(LayoutKind.Sequential)]
  public struct LUID
  {
   public uint LowPart;
   public int HighPart;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct TOKEN_PRIVILEGES_SINGLE
  {
   public UInt32 PrivilegeCount;
   public LUID Luid;
   public UInt32 Attributes;
  }
  #endregion

  #region Константы
  public static readonly IntPtr INVALID_HANDLE_NAME = (IntPtr)(-1);
  public const int MAX_PATH = 65535; // Max. NTFS path length
  public const int SE_PRIVILEGE_ENABLED = 0x00000002;
  public const int TOKEN_QUERY = 0x00000008;
  public const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
  public const int ERROR_NOT_ALL_ASSIGNED = 1300;
  #endregion
 }
}