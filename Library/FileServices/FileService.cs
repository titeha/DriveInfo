using System.Collections.Generic;
using System.IO;

namespace FileServices
{
 public class FileService
 {
  private DriveInfo _drive;

  public FileService(DriveInfo drive)
  {
   _drive = drive;
  }

  public IEnumerable<FSDTO> ScanDrive()
  {

  }
 }
}