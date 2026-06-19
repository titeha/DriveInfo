using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using ShowDriveInfo.Models;
using ShowDriveInfo.Services;
using ShowDriveInfo.ViewModels;

namespace ShowDriveInfo
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _scanDelay;

    private ScanProgressWindow? _progressWindow;
    private GridLength _savedPieWidth = new(1, GridUnitType.Star);

    public MainWindow()
    {
      InitializeComponent();

      _viewModel = new MainViewModel(new ScanService());
      DataContext = _viewModel;
      _viewModel.PropertyChanged += OnViewModelPropertyChanged;

      // Окно прогресса показываем только если скан длится дольше 3 секунд.
      _scanDelay = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
      _scanDelay.Tick += OnScanDelayTick;

      Closed += (_, _) => _viewModel.Dispose();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(MainViewModel.IsScanning):
          if (_viewModel.IsScanning)
          {
            _scanDelay.Stop();
            _scanDelay.Start();
          }
          else
          {
            _scanDelay.Stop();
            CloseProgress();
          }
          break;

        case nameof(MainViewModel.ShowChart):
          ApplyChartVisibility();
          break;
      }
    }

    private void OnScanDelayTick(object? sender, EventArgs e)
    {
      _scanDelay.Stop();

      if (_viewModel.IsScanning && _progressWindow is null)
        ShowProgress();
    }

    private void ShowProgress()
    {
      _progressWindow = new ScanProgressWindow { Owner = this, DataContext = _viewModel };
      _progressWindow.ShowDialog(); // блокирует до закрытия; async-продолжения скана продолжают работать
      _progressWindow = null;
    }

    private void CloseProgress() => _progressWindow?.Close();

    private void ApplyChartVisibility()
    {
      if (_viewModel.ShowChart)
      {
        PieColumn.Width = _savedPieWidth;
        ChartSplitter.Visibility = Visibility.Visible;
      }
      else
      {
        if (PieColumn.Width.Value > 0)
          _savedPieWidth = PieColumn.Width;

        PieColumn.Width = new GridLength(0);
        ChartSplitter.Visibility = Visibility.Collapsed;
      }
    }

    private void FsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      if (e.NewValue is IFileSystemNode _node)
        _viewModel.NavigateInto(_node);
    }
  }
}
