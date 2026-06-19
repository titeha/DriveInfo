// Ignore Spelling: Mvvm

// Локальная копия будущей библиотеки MvvmUtilites (см. G:\Projects\Windows\TimeManager\Core\MvvmUtilites).
// Когда библиотека будет оформлена как пакет — удалить эти файлы и подключить пакет (namespace сохранён).

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MvvmUtilites;

public class ObservableObject : INotifyPropertyChanged
{
  public event PropertyChangedEventHandler? PropertyChanged;

  private readonly List<WeakReference<CommandBase>> _commands = [];

  public void RegisterCommand(CommandBase command) => _commands.Add(new(command));

  /// <summary>
  /// Уведомляет об изменении одного свойства.
  /// </summary>
  /// <param name="propertyName">Имя свойства (по умолчанию определяется автоматически).</param>
  protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    UpdateCommandsState();
  }

  /// <summary>
  /// Уведомляет об изменении нескольких свойств.
  /// </summary>
  /// <param name="propertyNames">Имена свойств.</param>
  protected void OnPropertyChanged(params string[] propertyNames)
  {
    if (propertyNames == null || propertyNames.Length == 0)
      return;

    foreach (var propertyName in propertyNames)
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    UpdateCommandsState();
  }

  /// <summary>
  /// Уведомляет об изменении всех свойств объекта.
  /// </summary>
  protected void OnAllPropertiesChanged()
  {
    // Получаем все свойства типа (через рефлексию).
    foreach (var property in GetType().GetProperties())
      OnPropertyChanged(property.Name);

    UpdateCommandsState();
  }

  /// <summary>
  /// Устанавливает значение свойства и уведомляет об изменении.
  /// </summary>
  /// <typeparam name="T">Тип свойства.</typeparam>
  /// <param name="field">Поле свойства.</param>
  /// <param name="value">Новое значение.</param>
  /// <param name="propertyName">Имя свойства (по умолчанию определяется автоматически).</param>
  /// <returns>True, если значение изменилось.</returns>
  protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
  {
    if (EqualityComparer<T>.Default.Equals(field, value))
      return false;

    field = value;
    OnPropertyChanged(propertyName);
    return true;
  }

  private void UpdateCommandsState()
  {
    for (int i = _commands.Count - 1; i >= 0; i--)
      if (_commands[i].TryGetTarget(out var command))
        command.RaiseCanExecuteChanged();
      else
        _commands.RemoveAt(i);
  }
}
