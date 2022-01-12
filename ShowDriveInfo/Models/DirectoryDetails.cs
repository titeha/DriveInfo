using System.Collections.Generic;
using System.Collections.Specialized;

namespace ShowDriveInfo.Models
{
 public class DirectoryDetails
 {
  private ulong _size;
  private ulong _occupiedSize;
  private DirectoryDetails _parent;
  private BitVector32 _attributes;

  public List<DirectoryDetails> Directories { get; }

  public List<FileDetails> Files { get; }

  public string Name { get; }

  public ulong OccupiedSize
  {
   get
   {
	if (_occupiedSize == 0)
	 _occupiedSize = CalcOccupiedSize();

	return _occupiedSize;
   }
  }

  public ulong Size
  {
   get
   {
	if (_size == 0)
	 _size = CalcSize();

	return _size;
   }
  }

  public bool IsHidden
  {
   get => _attributes[0];
   internal set => _attributes[0] = value;
  }

  public bool IsSystem
  {
   get => _attributes[1];
   internal set => _attributes[1] = value;
  }

  public bool IsJunction
  {
   get => _attributes[2];
   internal set => _attributes[2] = value;
  }

  public DirectoryDetails(string name, DirectoryDetails parent)
  {
   Name = name;
   _parent = parent;
   Directories = new List<DirectoryDetails>();
   Files = new List<FileDetails>();
   _attributes = new BitVector32();
  }

  private ulong CalcOccupiedSize()
  {
   ulong _calculatedOccupiedSize = 0;

   foreach (FileDetails _file in Files)
	_calculatedOccupiedSize += _file.OccupiedSize;

   foreach (DirectoryDetails _directory in Directories)
	_calculatedOccupiedSize += _directory.OccupiedSize;

   return _calculatedOccupiedSize;
  }

  private ulong CalcSize()
  {
   ulong _calulatedSize = 0;

   foreach (FileDetails _file in Files)
	_calulatedSize += _file.Size;

   foreach (DirectoryDetails _directory in Directories)
	_calulatedSize += _directory.Size;

   return _calulatedSize;
  }
 }
}