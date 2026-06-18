namespace FileServices.Tests
{
  public class FileSystemChangeTests
  {
    private const string _dir = @"C:\dir";

    [Fact]
    public void From_Created_MapsToCreatedWithFullPath()
    {
      var args = new FileSystemEventArgs(WatcherChangeTypes.Created, _dir, "file.txt");

      var change = FileSystemChange.From(args);

      Assert.Equal(FileSystemChangeKind.Created, change.Kind);
      Assert.Equal(Path.Combine(_dir, "file.txt"), change.FullPath);
      Assert.Null(change.OldFullPath);
    }

    [Fact]
    public void From_Deleted_MapsToDeleted()
    {
      var args = new FileSystemEventArgs(WatcherChangeTypes.Deleted, _dir, "gone.txt");

      var change = FileSystemChange.From(args);

      Assert.Equal(FileSystemChangeKind.Deleted, change.Kind);
      Assert.Equal(Path.Combine(_dir, "gone.txt"), change.FullPath);
    }

    [Fact]
    public void From_Changed_MapsToChanged()
    {
      var args = new FileSystemEventArgs(WatcherChangeTypes.Changed, _dir, "edit.txt");

      var change = FileSystemChange.From(args);

      Assert.Equal(FileSystemChangeKind.Changed, change.Kind);
      Assert.Equal(Path.Combine(_dir, "edit.txt"), change.FullPath);
    }

    [Fact]
    public void From_Renamed_KeepsBothPaths()
    {
      var args = new RenamedEventArgs(WatcherChangeTypes.Renamed, _dir, "new.txt", "old.txt");

      var change = FileSystemChange.From(args);

      Assert.Equal(FileSystemChangeKind.Renamed, change.Kind);
      Assert.Equal(Path.Combine(_dir, "new.txt"), change.FullPath);
      Assert.Equal(Path.Combine(_dir, "old.txt"), change.OldFullPath);
    }

    [Fact]
    public void Overflowed_HasOverflowKindAndNoPaths()
    {
      var change = FileSystemChange.Overflowed();

      Assert.Equal(FileSystemChangeKind.Overflow, change.Kind);
      Assert.Null(change.FullPath);
      Assert.Null(change.OldFullPath);
    }

    [Fact]
    public void Constructor_DefaultsOldPathToNull()
    {
      var change = new FileSystemChange(FileSystemChangeKind.Created, @"C:\a.txt");

      Assert.Null(change.OldFullPath);
    }
  }
}
