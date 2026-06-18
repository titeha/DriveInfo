namespace FileServices.Tests
{
  public class FSDTOTests
  {
    [Fact]
    public void Constructor_SetsNameAndParent()
    {
      var dto = new FSDTO("file.txt", "C:\\dir");

      Assert.Equal("file.txt", dto.Name);
      Assert.Equal("C:\\dir", dto.Parent);
    }

    [Fact]
    public void Constructor_DefaultsAreUnset()
    {
      var dto = new FSDTO("file.txt", "C:\\dir");

      Assert.Equal(0UL, dto.Size);
      Assert.Equal(0UL, dto.VolumeSize);
      Assert.Empty(dto.HardLinks);
      Assert.False(dto.IsCompressed);
      Assert.False(dto.IsDirectory);
      Assert.False(dto.IsHidden);
      Assert.False(dto.IsJunction);
      Assert.False(dto.IsSystem);
    }

    // Каждый флаг упакован в отдельный бит BitVector32 — установка одного не должна задевать остальные

    [Fact]
    public void IsCompressed_SetTrue_DoesNotAffectOtherFlags()
    {
      var dto = new FSDTO("f", "p") { IsCompressed = true };

      Assert.True(dto.IsCompressed);
      Assert.False(dto.IsDirectory);
      Assert.False(dto.IsHidden);
      Assert.False(dto.IsJunction);
      Assert.False(dto.IsSystem);
    }

    [Fact]
    public void IsDirectory_SetTrue_DoesNotAffectOtherFlags()
    {
      var dto = new FSDTO("f", "p") { IsDirectory = true };

      Assert.False(dto.IsCompressed);
      Assert.True(dto.IsDirectory);
      Assert.False(dto.IsHidden);
      Assert.False(dto.IsJunction);
      Assert.False(dto.IsSystem);
    }

    [Fact]
    public void IsHidden_SetTrue_DoesNotAffectOtherFlags()
    {
      var dto = new FSDTO("f", "p") { IsHidden = true };

      Assert.False(dto.IsCompressed);
      Assert.False(dto.IsDirectory);
      Assert.True(dto.IsHidden);
      Assert.False(dto.IsJunction);
      Assert.False(dto.IsSystem);
    }

    [Fact]
    public void IsJunction_SetTrue_DoesNotAffectOtherFlags()
    {
      var dto = new FSDTO("f", "p") { IsJunction = true };

      Assert.False(dto.IsCompressed);
      Assert.False(dto.IsDirectory);
      Assert.False(dto.IsHidden);
      Assert.True(dto.IsJunction);
      Assert.False(dto.IsSystem);
    }

    [Fact]
    public void IsSystem_SetTrue_DoesNotAffectOtherFlags()
    {
      var dto = new FSDTO("f", "p") { IsSystem = true };

      Assert.False(dto.IsCompressed);
      Assert.False(dto.IsDirectory);
      Assert.False(dto.IsHidden);
      Assert.False(dto.IsJunction);
      Assert.True(dto.IsSystem);
    }

    [Fact]
    public void AllFlags_SetTrue_AllReadBackTrue()
    {
      var dto = new FSDTO("f", "p")
      {
        IsCompressed = true,
        IsDirectory = true,
        IsHidden = true,
        IsJunction = true,
        IsSystem = true
      };

      Assert.True(dto.IsCompressed);
      Assert.True(dto.IsDirectory);
      Assert.True(dto.IsHidden);
      Assert.True(dto.IsJunction);
      Assert.True(dto.IsSystem);
    }
  }
}
