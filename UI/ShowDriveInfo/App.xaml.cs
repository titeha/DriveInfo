using System.Windows;

//using static Permissions.Permission;

namespace ShowDriveInfo
{
 /// <summary>
 /// Interaction logic for App.xaml
 /// </summary>
 public partial class App : Application
 {
  private bool _hasSeBackupPrivileges;

  public bool HasSeBackupPrivileges => _hasSeBackupPrivileges;

  protected override void OnStartup(StartupEventArgs e)
  {
   base.OnStartup(e);
   //_hasSeBackupPrivileges = RequestSeBackupPrivileges();
  }
 }
}