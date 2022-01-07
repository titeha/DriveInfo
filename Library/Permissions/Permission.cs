using System;
using System.Runtime.InteropServices;

using static ImportWinAPI.Imports;

namespace Permissions
{
 public static class Permission
 {
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