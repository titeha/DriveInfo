namespace FileServices.Tests
{
  public class ParseBlockSizeTests
  {
    [Fact]
    public void ParseBlockSize_UInt64Value_ConvertsToUInt32()
    {
      // WMI Win32_Volume.BlockSize приходит как uint64 — прямой каст (uint) ронял InvalidCastException
      object _wmiValue = (ulong)4096;

      Assert.Equal(4096u, FileHelpers.ParseBlockSize(_wmiValue));
    }

    [Fact]
    public void ParseBlockSize_UInt32Value_ReturnsSame()
    {
      object _value = (uint)512;

      Assert.Equal(512u, FileHelpers.ParseBlockSize(_value));
    }

    [Fact]
    public void ParseBlockSize_Int32Value_Converts()
    {
      object _value = 8192;

      Assert.Equal(8192u, FileHelpers.ParseBlockSize(_value));
    }

    [Fact]
    public void ParseBlockSize_Null_ReturnsZero()
    {
      Assert.Equal(0u, FileHelpers.ParseBlockSize(null));
    }
  }
}
