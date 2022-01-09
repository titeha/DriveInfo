using System;
using System.Runtime.InteropServices;

namespace Permissions
{
 public static class Permission
 {
  #region Imports
  [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool LookupPrivilegeValue(string? lpSystemName, string lpName, out LUID lpLuid);

  [DllImport("advapi32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

  [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
  private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TOKEN_PRIVILEGES_SINGLE newst, int len, IntPtr prev, IntPtr relen);

  [DllImport("kernel32.dll", ExactSpelling = true)]
  private static extern IntPtr GetCurrentProcess();

  [StructLayout(LayoutKind.Sequential)]
  internal struct LUID
  {
   public uint LowPart;
   public int HighPart;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct TOKEN_PRIVILEGES_SINGLE
  {
   public uint PrivilegeCount;
   public LUID Luid;
   public uint Attributes;
  }

  internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
  internal const int TOKEN_QUERY = 0x00000008;
  internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
  internal const int ERROR_NOT_ALL_ASSIGNED = 1300;
  #endregion

  // Взято с:
  // https://ru.stackoverflow.com/questions/448240/%D0%9F%D0%BE%D0%BB%D1%83%D1%87%D0%B8%D1%82%D1%8C-%D0%B4%D0%BE%D1%81%D1%82%D1%83%D0%BF-%D0%BA-%D0%BF%D0%B0%D0%BF%D0%BA%D0%B5
  /// <summary>
  /// Пытается получить привилегии Backup для текущего сеанса
  /// </summary>
  /// <returns>Истинна, если получилось получить привилегии Backup</returns>
  public static bool RequestSeBackupPrivileges()
  {
   if (!LookupPrivilegeValue(null, "SeBackupPrivilege", out LUID _luid))
	return false;

   TOKEN_PRIVILEGES_SINGLE _tp = new()
   {
	PrivilegeCount = 1,
	Luid = _luid,
	Attributes = SE_PRIVILEGE_ENABLED
   };

   return OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr _hToken)
	&& AdjustTokenPrivileges(_hToken, false, ref _tp, 0, IntPtr.Zero, IntPtr.Zero)
	&& ERROR_NOT_ALL_ASSIGNED != Marshal.GetLastWin32Error();
  }
 }
}