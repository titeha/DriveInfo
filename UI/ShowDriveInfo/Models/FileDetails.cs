using System.Collections.Generic;
using System.Collections.Specialized;

namespace ShowDriveInfo.Models
{
  public sealed class FileDetails(string name, string fullPath) : IFileSystemNode
  {
    // BitVector32 индексируется битовой маской, а не порядковым номером — маски создаём
    // через CreateMask(), иначе флаги пересекаются (см. историю бага в FSDTO).
    private static readonly int _compressedMask = BitVector32.CreateMask();
    private static readonly int _hiddenMask = BitVector32.CreateMask(_compressedMask);
    private static readonly int _systemMask = BitVector32.CreateMask(_hiddenMask);

    private static readonly IReadOnlyList<IFileSystemNode> _noChildren = new IFileSystemNode[0];

    private BitVector32 _attributes = new BitVector32();

    public string Name { get; } = name;

    public string FullPath { get; } = fullPath;

    public ulong Size { get; internal set; }

    public ulong OccupiedSize { get; internal set; }

    public bool IsCompressed
    {
      get => _attributes[_compressedMask];
      internal set => _attributes[_compressedMask] = value;
    }

    public bool IsHidden
    {
      get => _attributes[_hiddenMask];
      internal set => _attributes[_hiddenMask] = value;
    }

    public bool IsSystem
    {
      get => _attributes[_systemMask];
      internal set => _attributes[_systemMask] = value;
    }

    public bool IsDirectory => false;

    public bool IsJunction => false;

    public IReadOnlyList<IFileSystemNode> Children => _noChildren;
  }
}
