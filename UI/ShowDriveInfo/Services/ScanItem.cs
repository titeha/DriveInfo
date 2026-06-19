using System.Collections.Generic;

namespace ShowDriveInfo.Services
{
  /// <summary>
  /// Плоская запись результата сканирования — вход для построителя дерева.
  /// Отвязана от внутреннего FSDTO библиотеки, чтобы мост тестировался изолированно.
  /// </summary>
  public sealed record ScanItem(
    string Name,
    string ParentPath,
    bool IsDirectory,
    ulong Size,
    ulong OccupiedSize,
    bool IsCompressed,
    bool IsHidden,
    bool IsSystem,
    bool IsJunction,
    bool IsAccessDenied,
    IReadOnlyCollection<string> HardLinks);
}
