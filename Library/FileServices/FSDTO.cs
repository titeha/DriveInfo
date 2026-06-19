using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace FileServices
{
 public struct FSDTO
 {
  // Индексатор BitVector32 принимает битовую маску, а не индекс. Маски создаём через
  // CreateMask, чтобы каждый флаг занимал свой отдельный бит и не пересекался с другими.
  private static readonly int _compressedMask = BitVector32.CreateMask();
  private static readonly int _directoryMask = BitVector32.CreateMask(_compressedMask);
  private static readonly int _hiddenMask = BitVector32.CreateMask(_directoryMask);
  private static readonly int _junctionMask = BitVector32.CreateMask(_hiddenMask);
  private static readonly int _systemMask = BitVector32.CreateMask(_junctionMask);
  private static readonly int _accessDeniedMask = BitVector32.CreateMask(_systemMask);

  private BitVector32 _attributes;

  public string Name { get; internal set; }

  public string Parent { get; internal set; }

  public ulong Size { get; internal set; }

  public ulong VolumeSize { get; internal set; }

  public IEnumerable<string> HardLinks { get; internal set; }

  public bool IsCompressed
  {
   get => _attributes[_compressedMask];
   internal set => _attributes[_compressedMask] = value;
  }

  public bool IsDirectory
  {
   get => _attributes[_directoryMask];
   internal set => _attributes[_directoryMask] = value;
  }

  public bool IsHidden
  {
   get => _attributes[_hiddenMask];
   internal set => _attributes[_hiddenMask] = value;
  }

  public bool IsJunction
  {
   get => _attributes[_junctionMask];
   internal set => _attributes[_junctionMask] = value;
  }

  public bool IsSystem
  {
   get => _attributes[_systemMask];
   internal set => _attributes[_systemMask] = value;
  }

  /// <summary>Каталог, в который не удалось зайти при сканировании (нет доступа).</summary>
  public bool IsAccessDenied
  {
   get => _attributes[_accessDeniedMask];
   internal set => _attributes[_accessDeniedMask] = value;
  }

  public FSDTO(string name, string parent)
  {
   Name = name;
   Parent = parent;
   Size = 0;
   VolumeSize = 0;
   _attributes = new BitVector32(0);
   HardLinks = Enumerable.Empty<string>();
  }
 }
}