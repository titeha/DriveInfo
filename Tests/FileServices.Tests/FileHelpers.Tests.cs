namespace FileServices.Tests
{
  public class FileHelpersTests : IDisposable
  {
    private readonly string _testDirectory;
    private readonly FileHelpers _fileHelpers;

    public FileHelpersTests()
    {
      _testDirectory = Path.Combine(Path.GetTempPath(), $"FileHelpersTests_{Guid.NewGuid()}");
      Directory.CreateDirectory(_testDirectory);
      _fileHelpers = new FileHelpers();
    }

    public void Dispose()
    {
      try
      {
        if (Directory.Exists(_testDirectory))
          Directory.Delete(_testDirectory, true);
      }
      catch
      {
        // Игнорируем ошибки удаления
      }
    }

    [Fact]
    public void FormatVolumeName_WithTrailingSlash_ReturnsCorrectFormat()
    {
      // Arrange & Act
      var result = FileHelpers.FormatVolumeName("C:\\");

      // Assert
      Assert.True(result);
      Assert.Equal("C:", result.Value);
    }

    [Fact]
    public void FormatVolumeName_WithoutTrailingSlash_ReturnsCorrectFormat()
    {
      // Arrange & Act
      var result = FileHelpers.FormatVolumeName("C");

      // Assert
      Assert.True(result);
      Assert.Equal("C:", result.Value);
    }

    [Fact]
    public void FormatVolumeName_AlreadyHasColon_ReturnsUnchanged()
    {
      // Arrange & Act
      var result = FileHelpers.FormatVolumeName("C:");

      // Assert
      Assert.True(result);
      Assert.Equal("C:", result.Value);
    }

    [Fact]
    public void FormatVolumeName_EmptyString_ReturnFailure()
    {
      var result = FileHelpers.FormatVolumeName(string.Empty);

      Assert.False(result);
      Assert.Equal("Имя тома не может быть пустым", result.Error);
    }

    [Fact]
    public void FormatVolumeName_Null_ThrowsArgumentException()
    {
         var result = FileHelpers.FormatVolumeName(null);

      Assert.False(result);
      Assert.Equal("Имя тома не может быть пустым", result.Error);
    }

    [Fact]
    public void GetHardLinks_ForFileWithoutHardLinks_ReturnsSinglePath()
    {
      // Arrange
      var testFile = Path.Combine(_testDirectory, "single.txt");
      File.WriteAllText(testFile, "test");
      var fileInfo = new FileInfo(testFile);

      // Act
      var hardLinks = _fileHelpers.GetHardLinks(fileInfo).ToList();

      // Assert
      Assert.Single(hardLinks);
    }

    [Fact]
    public void GetRealFileSize_ReturnsCorrectSize()
    {
      // Arrange
      var testFile = Path.Combine(_testDirectory, "size.txt");
      var content = new byte[1024]; // 1KB
      File.WriteAllBytes(testFile, content);
      var fileInfo = new FileInfo(testFile);

      // Act
      var size = FileHelpers.GetRealFileSize(fileInfo);

      // Assert
      Assert.Equal(1024UL, size);
    }
  }
}