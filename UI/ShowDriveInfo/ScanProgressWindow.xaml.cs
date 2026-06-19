using System.Windows;

namespace ShowDriveInfo
{
  /// <summary>
  /// Модальное окно прогресса сканирования (бесконечная индикация + отмена).
  /// DataContext назначает вызывающая сторона (MainViewModel).
  /// </summary>
  public partial class ScanProgressWindow : Window
  {
    public ScanProgressWindow()
    {
      InitializeComponent();
    }
  }
}
