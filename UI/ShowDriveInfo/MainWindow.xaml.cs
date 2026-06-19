using System.Windows;

using ShowDriveInfo.Services;
using ShowDriveInfo.ViewModels;

namespace ShowDriveInfo
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      DataContext = new MainViewModel(new ScanService());
    }
  }
}
