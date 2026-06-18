namespace FileServices.Tests
{
  public class FileExtensionsTests
  {
    [Fact]
    public void IsReparsePoint_WithReparsePointFlag_ReturnsTrue()
    {
      Assert.True(FileExtensions.IsReparsePoint(FileAttributes.ReparsePoint));
    }

    [Fact]
    public void IsReparsePoint_DirectoryWithReparsePoint_ReturnsTrue()
    {
      Assert.True(FileExtensions.IsReparsePoint(FileAttributes.Directory | FileAttributes.ReparsePoint));
    }

    [Fact]
    public void IsReparsePoint_PlainDirectory_ReturnsFalse()
    {
      Assert.False(FileExtensions.IsReparsePoint(FileAttributes.Directory));
    }

    [Fact]
    public void IsReparsePoint_NormalFile_ReturnsFalse()
    {
      Assert.False(FileExtensions.IsReparsePoint(FileAttributes.Normal));
    }

    [Fact]
    public void IsReparsePoint_MixedFlagsWithoutReparse_ReturnsFalse()
    {
      Assert.False(FileExtensions.IsReparsePoint(FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory));
    }
  }
}
