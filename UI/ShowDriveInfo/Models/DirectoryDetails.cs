using System.Collections.Generic;
using System.Collections.Specialized;

namespace ShowDriveInfo.Models
{
  public sealed class DirectoryDetails : IFileSystemNode
  {
    // BitVector32 индексируется битовой маской, а не порядковым номером — маски создаём
    // через CreateMask(), иначе флаги пересекаются (см. историю бага в FSDTO).
    private static readonly int _hiddenMask = BitVector32.CreateMask();
    private static readonly int _systemMask = BitVector32.CreateMask(_hiddenMask);
    private static readonly int _junctionMask = BitVector32.CreateMask(_systemMask);

    private BitVector32 _attributes;
    private readonly List<IFileSystemNode> _children = new();

    public string Name { get; }

    public string FullPath { get; }

    /// <summary>Родительский каталог (null для корня) — для навигации «вверх».</summary>
    public DirectoryDetails? Parent { get; internal set; }

    public ulong Size { get; internal set; }

    public ulong OccupiedSize { get; internal set; }

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

    public bool IsJunction
    {
      get => _attributes[_junctionMask];
      internal set => _attributes[_junctionMask] = value;
    }

    public bool IsDirectory => true;

    public bool IsCompressed => false;

    public IReadOnlyList<IFileSystemNode> Children => _children;

    public DirectoryDetails(string name, string fullPath)
    {
      Name = name;
      FullPath = fullPath;
      _attributes = new BitVector32();
    }

    internal void Add(IFileSystemNode child) => _children.Add(child);
  }
}
