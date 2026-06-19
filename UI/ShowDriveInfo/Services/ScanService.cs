using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FileServices;

using ShowDriveInfo.Models;

namespace ShowDriveInfo.Services
{
  public interface IScanService
  {
    /// <summary>Сканирует том в фоне, отдавая прогресс и поддерживая отмену.</summary>
    Task<DriveDetails> ScanAsync(DriveInfo drive, IProgress<ScanProgress>? progress, CancellationToken cancellationToken);
  }

  public sealed class ScanService : IScanService
  {
    private const int _reportEvery = 1000;

    public Task<DriveDetails> ScanAsync(DriveInfo drive, IProgress<ScanProgress>? progress, CancellationToken cancellationToken) =>
      Task.Run(() =>
      {
        var _details = new DriveDetails(drive);
        var _fileService = new FileService(drive);

        DirectoryDetails _root = FileSystemTreeBuilder.Build(
          drive.Name,
          drive.Name,
          Enumerate(_fileService, progress, cancellationToken));

        _details.Root = _root;
        _details.Other = FileSystemTreeBuilder.ComputeOther(_details.Used, _root.OccupiedSize);
        return _details;
      }, cancellationToken);

    /// <summary>Лениво проецирует поток FSDTO в ScanItem, попутно сообщая прогресс и проверяя отмену.</summary>
    private static IEnumerable<ScanItem> Enumerate(FileService fileService, IProgress<ScanProgress>? progress, CancellationToken cancellationToken)
    {
      int _count = 0;

      foreach (FSDTO _dto in fileService.ScanDrive())
      {
        cancellationToken.ThrowIfCancellationRequested();

        if (++_count % _reportEvery == 0)
          progress?.Report(new ScanProgress(_count, _dto.Parent));

        yield return ToScanItem(_dto);
      }

      progress?.Report(new ScanProgress(_count, null));
    }

    private static ScanItem ToScanItem(FSDTO dto) => new(
      dto.Name,
      dto.Parent,
      dto.IsDirectory,
      dto.Size,
      dto.VolumeSize,
      dto.IsCompressed,
      dto.IsHidden,
      dto.IsSystem,
      dto.IsJunction,
      dto.HardLinks.ToArray());
  }
}
