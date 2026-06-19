using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using MvvmUtilites;

using ShowDriveInfo.Models;
using ShowDriveInfo.Services;

namespace ShowDriveInfo.ViewModels
{
  public sealed class MainViewModel : ObservableObject
  {
    private readonly IScanService _scanService;

    private DriveInfo? _selectedDrive;
    private DriveDetails? _currentDrive;
    private DirectoryDetails? _currentNode;
    private bool _isScanning;
    private bool _showChart = true;
    private int _progressCount;
    private string? _progressPath;
    private string? _status;
    private CancellationTokenSource? _cts;

    public IReadOnlyList<DriveInfo> Drives { get; }

    public ICommand ScanCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand NavigateUpCommand { get; }

    public ICommand NavigateIntoCommand { get; }

    public MainViewModel(IScanService scanService)
    {
      _scanService = scanService;
      Drives = DriveInfo.GetDrives().Where(_d => _d.IsReady).ToList();
      _selectedDrive = Drives.FirstOrDefault();

      ScanCommand = new AsyncRelayCommand(ScanAsync, () => CanScan, this);
      CancelCommand = new RelayCommand(() => _cts?.Cancel(), () => IsScanning, this);
      NavigateUpCommand = new RelayCommand(NavigateUp, () => CurrentNode?.Parent is not null, this);
      NavigateIntoCommand = new RelayCommand<IFileSystemNode>(NavigateInto, _n => _n is DirectoryDetails, this);
    }

    public DriveInfo? SelectedDrive
    {
      get => _selectedDrive;
      set => Set(ref _selectedDrive, value);
    }

    public DriveDetails? CurrentDrive
    {
      get => _currentDrive;
      private set => Set(ref _currentDrive, value);
    }

    /// <summary>Текущий уровень для диаграммы и навигации «вглубь».</summary>
    public DirectoryDetails? CurrentNode
    {
      get => _currentNode;
      private set
      {
        if (Set(ref _currentNode, value))
          OnPropertyChanged(nameof(CurrentChildren));
      }
    }

    /// <summary>Дети текущего уровня, отсортированные по занимаемому месту — для диаграммы.</summary>
    public IReadOnlyList<IFileSystemNode> CurrentChildren =>
      _currentNode?.Children.OrderByDescending(_c => _c.OccupiedSize).ToList()
      ?? (IReadOnlyList<IFileSystemNode>)[];

    /// <summary>Корень дерева в виде коллекции из одного элемента — для ItemsSource.</summary>
    public IReadOnlyList<DirectoryDetails> RootNodes =>
      CurrentDrive?.Root is { } _root ? [_root] : [];

    public bool IsScanning
    {
      get => _isScanning;
      private set => Set(ref _isScanning, value);
    }

    public bool ShowChart
    {
      get => _showChart;
      set => Set(ref _showChart, value);
    }

    public int ProgressCount
    {
      get => _progressCount;
      private set => Set(ref _progressCount, value);
    }

    public string? ProgressPath
    {
      get => _progressPath;
      private set => Set(ref _progressPath, value);
    }

    public string? Status
    {
      get => _status;
      private set => Set(ref _status, value);
    }

    private bool CanScan => SelectedDrive is not null && !IsScanning;

    public void NavigateInto(IFileSystemNode node)
    {
      if (node is DirectoryDetails _directory)
        CurrentNode = _directory;
    }

    private void NavigateUp()
    {
      if (CurrentNode?.Parent is { } _parent)
        CurrentNode = _parent;
    }

    private async Task ScanAsync()
    {
      if (SelectedDrive is not { } _drive)
        return;

      _cts = new CancellationTokenSource();
      IsScanning = true;
      ProgressCount = 0;
      ProgressPath = null;
      Status = $"Сканирование {_drive.Name}…";

      var _progress = new Progress<ScanProgress>(_p =>
      {
        ProgressCount = _p.Count;
        ProgressPath = _p.CurrentPath;
      });

      try
      {
        DriveDetails _result = await _scanService.ScanAsync(_drive, _progress, _cts.Token);
        CurrentDrive = _result;
        CurrentNode = _result.Root;
        OnPropertyChanged(nameof(RootNodes));
        Status = $"Готово: {ProgressCount} элементов";
      }
      catch (OperationCanceledException)
      {
        Status = "Сканирование отменено";
      }
      catch (Exception _ex)
      {
        Status = $"Ошибка: {_ex.Message}";
      }
      finally
      {
        IsScanning = false;
        _cts.Dispose();
        _cts = null;
      }
    }
  }
}
