using System.Collections.Specialized;

namespace FileServices
{
 public struct FSDTO
 {
  private BitVector32 _attributes;

  public string Name { get; internal set; }

  public string Parent { get; internal set; }

  public ulong Size { get; internal set; }

  public ulong VolumeSize { get; internal set; }

  public bool IsCompressed
  {
   get => _attributes[0];
   internal set => _attributes[0] = value;
  }

  public bool IsDirectory
  {
   get => _attributes[1];
   internal set => _attributes[1] = value;
  }

  public bool IsHidden
  {
   get => _attributes[2];
   internal set => _attributes[2] = value;
  }

  public bool IsJunction
  {
   get => _attributes[3];
   internal set => _attributes[3] = value;
  }

  public bool IsSystem
  {
   get => _attributes[4];
   internal set => _attributes[4] = value;
  }

  public FSDTO(string name, string parent)
  {
   Name = name;
   Parent = parent;
   Size = 0;
   VolumeSize = 0;
   _attributes = new BitVector32(0);
  }
 }
}