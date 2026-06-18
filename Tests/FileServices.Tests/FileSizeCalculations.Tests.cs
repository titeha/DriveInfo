namespace FileServices.Tests
{
  public class FileSizeCalculationsTests
  {
    #region CombineFileSize

    [Fact]
    public void CombineFileSize_LowWordOnly_ReturnsLowWord()
    {
      Assert.Equal(1024UL, FileHelpers.CombineFileSize(0, 1024));
    }

    [Fact]
    public void CombineFileSize_MaxLowWord_ReturnsValueBelow4Gb()
    {
      Assert.Equal(uint.MaxValue, FileHelpers.CombineFileSize(0, uint.MaxValue));
    }

    [Fact]
    public void CombineFileSize_HighWordOne_Returns4Gb()
    {
      // Регрессионный тест: ранее _hiSize << 32 не сдвигалось (счётчик маскировался до 5 бит),
      // из-за чего файлы > 4 ГБ считались неверно.
      Assert.Equal(4294967296UL, FileHelpers.CombineFileSize(1, 0));
    }

    [Fact]
    public void CombineFileSize_HighWordTwo_Returns8Gb()
    {
      Assert.Equal(8589934592UL, FileHelpers.CombineFileSize(2, 0));
    }

    [Fact]
    public void CombineFileSize_BothWords_CombinesCorrectly()
    {
      Assert.Equal(4294967297UL, FileHelpers.CombineFileSize(1, 1));
    }

    [Fact]
    public void CombineFileSize_MaxValues_ReturnsUlongMax()
    {
      Assert.Equal(ulong.MaxValue, FileHelpers.CombineFileSize(uint.MaxValue, uint.MaxValue));
    }

    #endregion

    #region RoundUpToCluster

    [Theory]
    [InlineData(0UL, 4096U, 0UL)]
    [InlineData(1UL, 4096U, 4096UL)]
    [InlineData(4095UL, 4096U, 4096UL)]
    [InlineData(4096UL, 4096U, 4096UL)]
    [InlineData(4097UL, 4096U, 8192UL)]
    [InlineData(5000UL, 4096U, 8192UL)]
    [InlineData(8192UL, 4096U, 8192UL)]
    public void RoundUpToCluster_RoundsUpToNearestCluster(ulong size, uint clusterSize, ulong expected)
    {
      Assert.Equal(expected, FileHelpers.RoundUpToCluster(size, clusterSize));
    }

    [Fact]
    public void RoundUpToCluster_ZeroClusterSize_ReturnsSizeUnchanged()
    {
      // Защита от деления на ноль, когда размер кластера неизвестен (WMI не отдал значение)
      Assert.Equal(5000UL, FileHelpers.RoundUpToCluster(5000, 0));
    }

    #endregion

    #region CalculateSizeOnVolume

    [Fact]
    public void CalculateSizeOnVolume_TinyFile_StoredResidentlyInMft()
    {
      // Файлы меньше порога MFT (680 байт) не занимают отдельный кластер
      Assert.Equal(500UL, FileHelpers.CalculateSizeOnVolume(500, 4096));
    }

    [Fact]
    public void CalculateSizeOnVolume_AtMftThreshold_RoundsToCluster()
    {
      // Ровно на пороге (680) условие резидентности уже не выполняется -> округляем
      Assert.Equal(4096UL, FileHelpers.CalculateSizeOnVolume(680, 4096));
    }

    [Fact]
    public void CalculateSizeOnVolume_JustBelowThreshold_StaysResident()
    {
      Assert.Equal(679UL, FileHelpers.CalculateSizeOnVolume(679, 4096));
    }

    [Fact]
    public void CalculateSizeOnVolume_LargeFile_RoundsUpToCluster()
    {
      Assert.Equal(8192UL, FileHelpers.CalculateSizeOnVolume(5000, 4096));
    }

    #endregion
  }
}
