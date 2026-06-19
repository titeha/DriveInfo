// Локальная копия будущей библиотеки MvvmUtilites (см. G:\Projects\Windows\TimeManager\Core\MvvmUtilites).
// Когда библиотека будет оформлена как пакет — удалить эти файлы и подключить пакет (namespace сохранён).

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmUtilites;

public abstract class CommandBase : ICommand
{
  protected ObservableObject? _owner;

  public event EventHandler? CanExecuteChanged;

  protected CommandBase(ObservableObject? owner)
  {
    _owner = owner;

    _owner?.RegisterCommand(this);
  }

  /// <summary>
  /// Вызывает событие CanExecuteChanged.
  /// </summary>
  public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

  /// <summary>
  /// Определяет, может ли команда выполняться.
  /// </summary>
  public abstract bool CanExecute(object? parameter);

  /// <summary>
  /// Выполняет команду.
  /// </summary>
  public abstract void Execute(object? parameter);
}

public class RelayCommand(Action execute, Func<bool>? canExecute = null, ObservableObject? owner = null) : CommandBase(owner)
{
  #region Поля
  private readonly Action _execute= execute ?? throw new ArgumentNullException(nameof(execute));
  private readonly Func<bool>? _canExecute= canExecute;
  #endregion

  #region Конструктор
  public RelayCommand(Action execute) : this(execute, null) { }
  #endregion

  #region Реализация интерфейса ICommand
  [DebuggerStepThrough]
  public override bool CanExecute(object? param) => _canExecute == null || _canExecute();

  public override void Execute(object? param) => _execute();
  #endregion
}

public class RelayCommand<T>(Action<T> execute, Predicate<T>? canExecute = null, ObservableObject? owner = null) : CommandBase(owner)
{
  #region Поля
  private readonly Action<T> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
  private readonly Predicate<T>? _canExecute = canExecute;

  #endregion

  #region Конструктор
  public RelayCommand(Action<T> execute) : this(execute, null) { }
  #endregion

  #region Реализация интерфейса ICommand
  [DebuggerStepThrough]
  public override bool CanExecute(object? parameter) => _canExecute == null || _canExecute((T)parameter!);

  public override void Execute(object? parameter) => _execute((T)parameter!);
  #endregion
}

public class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null, ObservableObject? owner = null) : CommandBase(owner)
{
  #region Поля
  private readonly Func<Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
  private readonly Func<bool>? _canExecute = canExecute;
  private bool _isExecuting;
  #endregion

  #region Конструктор
  public AsyncRelayCommand(Func<Task> execute) : this(execute, null) { }
  #endregion

  #region Реализация интерфейса ICommand
  [DebuggerStepThrough]
  public override bool CanExecute(object? parameter) => !_isExecuting && (_canExecute == null || _canExecute());

  public override async void Execute(object? parameter) => await ExecuteAsync();

  public async Task ExecuteAsync()
  {
    if (CanExecute(null))
    {
      try
      {
        _isExecuting = true;
        RaiseCanExecuteChanged();

        await _execute();
      }
      finally
      {
        _isExecuting = false;
        RaiseCanExecuteChanged();
      }
    }
  }
  #endregion
}

public class AsyncRelayCommand<T>(Func<T, Task> execute, Predicate<T>? canExecute = null, ObservableObject? owner = null) : CommandBase(owner)
{
  #region Поля
  private readonly Func<T, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
  private readonly Predicate<T>? _canExecute = canExecute;
  private bool _isExecuting;
  #endregion

  #region Конструктор
  public AsyncRelayCommand(Func<T, Task> execute) : this(execute, null) { }
  #endregion

  #region Реализация интерфейса ICommand
  [DebuggerStepThrough]
  public override bool CanExecute(object? parameter) => !_isExecuting && (_canExecute == null || _canExecute((T)parameter!));

  public override async void Execute(object? parameter) => await ExecuteAsync((T)parameter!);

  public async Task ExecuteAsync(T parameter)
  {
    if (CanExecute(parameter))
    {
      try
      {
        _isExecuting = true;
        RaiseCanExecuteChanged();

        await _execute(parameter);
      }
      finally
      {
        _isExecuting = false;
        RaiseCanExecuteChanged();
      }
    }
  }
  #endregion
}
