using ShowDriveInfo.Converters;

namespace ShowDriveInfo.Tests
{
  public class ByteSizeFormatterTests
  {
    [Theory]
    [InlineData(0UL, "0 Б")]
    [InlineData(512UL, "512 Б")]
    [InlineData(1023UL, "1023 Б")]
    [InlineData(1024UL, "1 КБ")]
    [InlineData(1536UL, "1.5 КБ")]
    [InlineData(1048576UL, "1 МБ")]
    [InlineData(1572864UL, "1.5 МБ")]
    [InlineData(1073741824UL, "1 ГБ")]
    [InlineData(1610612736UL, "1.5 ГБ")]
    [InlineData(1099511627776UL, "1 ТБ")]
    [InlineData(1125899906842624UL, "1 ПБ")]
    public void Format_ProducesHumanReadable(ulong bytes, string expected)
    {
      Assert.Equal(expected, ByteSizeFormatter.Format(bytes));
    }

    [Fact]
    public void Format_VeryLarge_StaysInPetabytes()
    {
      // Выше ПБ единицы не растут — остаёмся в ПБ
      Assert.EndsWith(" ПБ", ByteSizeFormatter.Format(ulong.MaxValue));
    }
  }
}
