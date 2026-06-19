using System.Collections.Generic;

using ShowDriveInfo.Converters;
using ShowDriveInfo.Models;

namespace ShowDriveInfo.Tests
{
  public class NodeCategorizerTests
  {
    /// <summary>Тестовая заглушка узла — IFileSystemNode публичный, флаги задаются произвольно.</summary>
    private sealed class FakeNode : IFileSystemNode
    {
      public string Name => "fake";
      public string FullPath => @"C:\fake";
      public ulong Size => 0;
      public ulong OccupiedSize => 0;
      public bool IsDirectory { get; init; }
      public bool IsHidden { get; init; }
      public bool IsSystem { get; init; }
      public bool IsJunction { get; init; }
      public bool IsCompressed { get; init; }
      public IReadOnlyList<IFileSystemNode> Children => [];
    }

    [Fact]
    public void Categorize_PlainNode_IsNormal()
    {
      Assert.Equal(FileSystemNodeCategory.Normal, NodeCategorizer.Categorize(new FakeNode()));
    }

    [Fact]
    public void Categorize_Compressed_IsCompressed()
    {
      Assert.Equal(FileSystemNodeCategory.Compressed, NodeCategorizer.Categorize(new FakeNode { IsCompressed = true }));
    }

    [Fact]
    public void Categorize_Hidden_IsHidden()
    {
      Assert.Equal(FileSystemNodeCategory.Hidden, NodeCategorizer.Categorize(new FakeNode { IsHidden = true }));
    }

    [Fact]
    public void Categorize_System_IsSystem()
    {
      Assert.Equal(FileSystemNodeCategory.System, NodeCategorizer.Categorize(new FakeNode { IsSystem = true }));
    }

    [Fact]
    public void Categorize_Junction_IsJunction()
    {
      Assert.Equal(FileSystemNodeCategory.Junction, NodeCategorizer.Categorize(new FakeNode { IsJunction = true }));
    }

    // Приоритет: junction > system > hidden > compressed

    [Fact]
    public void Categorize_JunctionBeatsSystem()
    {
      var node = new FakeNode { IsJunction = true, IsSystem = true, IsHidden = true };
      Assert.Equal(FileSystemNodeCategory.Junction, NodeCategorizer.Categorize(node));
    }

    [Fact]
    public void Categorize_SystemBeatsHidden()
    {
      var node = new FakeNode { IsSystem = true, IsHidden = true, IsCompressed = true };
      Assert.Equal(FileSystemNodeCategory.System, NodeCategorizer.Categorize(node));
    }

    [Fact]
    public void Categorize_HiddenBeatsCompressed()
    {
      var node = new FakeNode { IsHidden = true, IsCompressed = true };
      Assert.Equal(FileSystemNodeCategory.Hidden, NodeCategorizer.Categorize(node));
    }
  }
}
