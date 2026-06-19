using ShowDriveInfo.Converters;

namespace ShowDriveInfo.Tests
{
  public class DriveDisplayFormatterTests
  {
    [Theory]
    [InlineData(@"C:\", "Windows", "C: (Windows)")]
    [InlineData(@"D:\", "Data", "D: (Data)")]
    [InlineData(@"E:\", "", "E:")]
    [InlineData(@"F:\", null, "F:")]
    [InlineData(@"G:\", "   ", "G:")]
    public void Format_CombinesLetterAndLabel(string name, string? label, string expected)
    {
      Assert.Equal(expected, DriveDisplayFormatter.Format(name, label));
    }
  }
}
