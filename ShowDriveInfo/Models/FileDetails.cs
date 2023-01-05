using System.Collections.Specialized;

namespace ShowDriveInfo.Models
{
  public class FileDetails
  {
    private BitVector32 _attributes;

    public ulong OccupiedSize { get; internal set; }

    public ulong Size { get; internal set; }

    public string Name { get; }

    public bool IsCompressed
    {
      get => _attributes[0];
      internal set => _attributes[0] = value;
    }

    public bool IsHidden
    {
      get => _attributes[1];
      internal set => _attributes[1] = value;
    }

    public bool IsSystem
    {
      get => _attributes[2];
      internal set => _attributes[2] = value;
    }

    public FileDetails(string name)
    {
      Name = name;
      _attributes = new BitVector32();
    }
  }
}