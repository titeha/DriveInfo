using System.Collections.Generic;

namespace ShowDriveInfo.Models
{
  /// <summary>
  /// Общий узел дерева файловой системы — единое представление для дерева и диаграммы.
  /// </summary>
  public interface IFileSystemNode
  {
    string Name { get; }

    string FullPath { get; }

    /// <summary>Логический размер (сумма длин файлов).</summary>
    ulong Size { get; }

    /// <summary>Занимаемое место на диске с учётом кластеров (дедуплицировано по жёстким ссылкам).</summary>
    ulong OccupiedSize { get; }

    bool IsDirectory { get; }

    bool IsHidden { get; }

    bool IsSystem { get; }

    bool IsJunction { get; }

    bool IsCompressed { get; }

    IReadOnlyList<IFileSystemNode> Children { get; }
  }
}
