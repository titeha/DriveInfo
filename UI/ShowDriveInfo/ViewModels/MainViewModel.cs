using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

using FileServices;

using MvvmUtilites;

using ShowDriveInfo.Models;
using ShowDriveInfo.Services;

namespace ShowDriveInfo.ViewModels
{
  public sealed class MainViewModel : ObservableObject, IDisposable
  {
    private static readonly TimeSpan _debounceDelay = TimeSpan.FromSeconds(2);

    private readonly IScanService _scanService;
    private readonly Dispatcher _dispatcher;
    private readonly DispatcherTimer _debounce;

    private IDriveWatcher? _watcher;
    private CancellationTokenSource? _cts;
    private Task? _inFlight;
    private bool _scanInProgress;
    private DriveInfo? _watchedDrive;

    private DriveInfo? _selectedDrive;
    private DriveDetails? _currentDrive;
    private DirectoryDetails? _currentNode;
    private bool _isScanning;
    private bool _showChart = true;
    private bool _autoRefresh = true;
    private int _progressCount;
    private string? _progressPath;
    private string? _status;

    public IReadOnlyList<DriveInfo> Drives { get; }

    public ICommand ScanCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand NavigateUpCommand { get; }

    public ICommand NavigateIntoCommand { get; }

    public MainViewModel(IScanService scanService)
    {
      _scanService = scanService;
      _dispatcher = Dispatcher.CurrentDispatcher;
      _debounce = new DispatcherTimer { Interval = _debounceDelay };
      _debounce.Tick += OnDebounceTick;

      Drives = DriveInfo.GetDrives().Where(_d => _d.IsReady).ToList();
      _selectedDrive = Drives.FirstOrDefault();

      ScanCommand = new AsyncRelayCommand(() => RunScanAsync(userInitiated: true), () => CanScan, this);
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

    /// <summary>Авто-перескан тома при изменениях файловой системы.</summary>
    public bool AutoRefresh
    {
      get => _autoRefresh;
      set => Set(ref _autoRefresh, value);
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
      if (node is not DirectoryDetails _directory)
        return;

      // Залоченные области не открываем — только сообщаем в статус.
      if (_directory.IsJunction)
      {
        Status = $"«{_directory.Name}» — точка соединения, не открываем";
        return;
      }

      if (_directory.IsAccessDenied)
      {
        Status = $"«{_directory.Name}» — нет доступа к области";
        return;
      }

      CurrentNode = _directory;
    }

    private void NavigateUp()
    {
      if (CurrentNode?.Parent is { } _parent)
        CurrentNode = _parent;
    }

    private async Task RunScanAsync(bool userInitiated)
    {
      // Ручной скан — по выбранному диску; авто-перескан — по тому, за которым следим.
      DriveInfo? _drive = userInitiated ? SelectedDrive : _watchedDrive;
      if (_drive is null)
        return;

      if (userInitiated)
      {
        // Ручной скан имеет приоритет: прерываем фоновый перескан и ждём его сворачивания.
        _cts?.Cancel();
        if (_inFlight is { } _previous)
        {
          try { await _previous; }
          catch { /* отменённый фоновый перескан */ }
        }
      }
      else if (_scanInProgress)
      {
        return; // фоновый перескан уступает уже идущей работе (дебаунс перезапустит позже)
      }

      _inFlight = ScanCoreAsync(_drive, userInitiated);
      await _inFlight;
    }

    private async Task ScanCoreAsync(DriveInfo drive, bool userInitiated)
    {
      _scanInProgress = true;

      // При авто-перескане стараемся вернуться на тот же уровень, что был открыт.
      string? _keepPath = userInitiated ? null : CurrentNode?.FullPath;

      _cts = new CancellationTokenSource();
      if (userInitiated)
        IsScanning = true;

      ProgressCount = 0;
      ProgressPath = null;
      Status = userInitiated ? $"Сканирование {drive.Name}…" : "Обновление после изменений…";

      var _progress = new Progress<ScanProgress>(_p =>
      {
        ProgressCount = _p.Count;
        ProgressPath = _p.CurrentPath;
      });

      try
      {
        DriveDetails _result = await _scanService.ScanAsync(drive, _progress, _cts.Token);
        CurrentDrive = _result;
        CurrentNode = (_result.Root is { } _root ? ResolvePath(_root, _keepPath) : null) ?? _result.Root;
        OnPropertyChanged(nameof(RootNodes));
        Status = $"Готово: {ProgressCount} элементов";

        _watchedDrive = drive;
        StartWatching(drive);
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
        if (userInitiated)
          IsScanning = false;

        _scanInProgress = false;
        _cts?.Dispose();
        _cts = null;
      }
    }

    /// <summary>Ищет каталог по полному пути в построенном дереве (для восстановления уровня после переcкана).</summary>
    private static DirectoryDetails? ResolvePath(DirectoryDetails root, string? fullPath)
    {
      if (fullPath is null)
        return null;

      if (string.Equals(root.FullPath, fullPath, StringComparison.OrdinalIgnoreCase))
        return root;

      foreach (IFileSystemNode _child in root.Children)
        if (_child is DirectoryDetails _dir && ResolvePath(_dir, fullPath) is { } _found)
          return _found;

      return null;
    }

    private void StartWatching(DriveInfo drive)
    {
      StopWatching();

      _watcher = new DriveWatcher(drive);
      _watcher.Changed += OnDriveChanged;
      _watcher.Start();
    }

    private void StopWatching()
    {
      if (_watcher is null)
        return;

      _watcher.Changed -= OnDriveChanged;
      _watcher.Dispose();
      _watcher = null;
    }

    // Прилетает на потоке пула — перепланируем дебаунс в UI-поток.
    private void OnDriveChanged(object? sender, FileSystemChange change)
    {
      if (!AutoRefresh)
        return;

      _dispatcher.BeginInvoke(() =>
      {
        _debounce.Stop();
        _debounce.Start();
      });
    }

    private void OnDebounceTick(object? sender, EventArgs e)
    {
      _debounce.Stop();

      if (!_scanInProgress && AutoRefresh)
        _ = RunScanAsync(userInitiated: false);
    }

    public void Dispose()
    {
      _debounce.Tick -= OnDebounceTick;
      _debounce.Stop();
      StopWatching();
      _cts?.Dispose();
    }
  }
}
