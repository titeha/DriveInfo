using ShowDriveInfo.Models;
using ShowDriveInfo.Services;

namespace ShowDriveInfo.Tests
{
  public class FileSystemTreeBuilderTests
  {
    private const string _root = @"C:\";

    private static ScanItem Dir(string name, string parent) =>
      new(name, parent, IsDirectory: true, Size: 0, OccupiedSize: 0,
          IsCompressed: false, IsHidden: false, IsSystem: false, IsJunction: false,
          HardLinks: []);

    private static ScanItem Fil(string name, string parent, ulong size, ulong occupied, string[]? hardLinks = null) =>
      new(name, parent, IsDirectory: false, Size: size, OccupiedSize: occupied,
          IsCompressed: false, IsHidden: false, IsSystem: false, IsJunction: false,
          HardLinks: hardLinks ?? []);

    [Fact]
    public void Build_SimpleTree_BuildsHierarchy()
    {
      var items = new[]
      {
        Fil("a.txt", @"C:\", 100, 128),
        Dir("sub", @"C:\"),
        Fil("b.txt", @"C:\sub", 50, 64)
      };

      var root = FileSystemTreeBuilder.Build(_root, _root, items);

      Assert.Equal(2, root.Children.Count);
      var sub = root.Children.OfType<DirectoryDetails>().Single();
      Assert.Equal("sub", sub.Name);
      Assert.Single(sub.Children);
      Assert.Equal("b.txt", sub.Children[0].Name);
    }

    [Fact]
    public void Build_SimpleTree_RollsUpSizesBottomUp()
    {
      var items = new[]
      {
        Fil("a.txt", @"C:\", 100, 128),
        Dir("sub", @"C:\"),
        Fil("b.txt", @"C:\sub", 50, 64)
      };

      var root = FileSystemTreeBuilder.Build(_root, _root, items);
      var sub = root.Children.OfType<DirectoryDetails>().Single();

      Assert.Equal(150UL, root.Size);
      Assert.Equal(192UL, root.OccupiedSize);
      Assert.Equal(50UL, sub.Size);
      Assert.Equal(64UL, sub.OccupiedSize);
    }

    [Fact]
    public void Build_HardLinkedFile_CountedOnceInRollup()
    {
      // Два каталожных входа на один и тот же файл (общая группа жёстких ссылок)
      var links = new[] { @"C:\a", @"C:\sub\b" };
      var items = new[]
      {
        Fil("a", @"C:\", 100, 64, links),
        Dir("sub", @"C:\"),
        Fil("b", @"C:\sub", 100, 64, links)
      };

      var root = FileSystemTreeBuilder.Build(_root, _root, items);
      var sub = root.Children.OfType<DirectoryDetails>().Single();

      // Байты учтены лишь у первого встреченного (под корнем), второй экземпляр — ноль в сумме
      Assert.Equal(64UL, root.OccupiedSize);
      Assert.Equal(0UL, sub.OccupiedSize);
    }

    [Fact]
    public void Build_HardLinkedFile_StillKeepsOwnDisplaySize()
    {
      var links = new[] { @"C:\a", @"C:\sub\b" };
      var items = new[]
      {
        Fil("a", @"C:\", 100, 64, links),
        Dir("sub", @"C:\"),
        Fil("b", @"C:\sub", 100, 64, links)
      };

      var root = FileSystemTreeBuilder.Build(_root, _root, items);
      var sub = root.Children.OfType<DirectoryDetails>().Single();

      // У самого узла-дубликата размер для отображения сохраняется
      Assert.Equal(64UL, sub.Children[0].OccupiedSize);
    }

    [Fact]
    public void Build_OrphanWithMissingParent_IsSkipped()
    {
      var items = new[]
      {
        Fil("lost.txt", @"C:\nonexistent", 100, 128)
      };

      var root = FileSystemTreeBuilder.Build(_root, _root, items);

      Assert.Empty(root.Children);
      Assert.Equal(0UL, root.OccupiedSize);
    }

    [Fact]
    public void Build_PreservesDirectoryAttributes()
    {
      var hiddenDir = new ScanItem("secret", @"C:\", IsDirectory: true, Size: 0, OccupiedSize: 0,
        IsCompressed: false, IsHidden: true, IsSystem: true, IsJunction: false,
        HardLinks: System.Array.Empty<string>());

      var root = FileSystemTreeBuilder.Build(_root, _root, new[] { hiddenDir });
      var dir = root.Children.OfType<DirectoryDetails>().Single();

      Assert.True(dir.IsHidden);
      Assert.True(dir.IsSystem);
      Assert.False(dir.IsJunction);
    }

    [Theory]
    [InlineData(1000UL, 600UL, 400UL)]
    [InlineData(500UL, 800UL, 0UL)]
    [InlineData(700UL, 700UL, 0UL)]
    public void ComputeOther_ReturnsDifferenceOrZero(ulong used, ulong scanned, ulong expected)
    {
      Assert.Equal(expected, FileSystemTreeBuilder.ComputeOther(used, scanned));
    }
  }
}
