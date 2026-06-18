using System;
using System.Collections.Generic;
using System.IO;

namespace FileServices
{
  internal static class SafeEnumeration
  {
    /// <summary>
    /// Лениво перечисляет последовательность, молча прекращая перечисление при отказе доступа
    /// (<see cref="UnauthorizedAccessException"/>) или ошибке ввода-вывода (<see cref="IOException"/>) —
    /// как при получении перечислителя, так и при продвижении по нему. Уже выданные элементы сохраняются.
    /// </summary>
    /// <typeparam name="T">Тип элементов последовательности</typeparam>
    /// <param name="source">Фабрика исходной последовательности</param>
    /// <returns>Безопасное перечисление</returns>
    public static IEnumerable<T> Enumerate<T>(Func<IEnumerable<T>> source)
    {
      IEnumerator<T> _enumerator;
      try
      {
        _enumerator = source().GetEnumerator();
      }
      catch (UnauthorizedAccessException)
      {
        yield break;
      }
      catch (IOException)
      {
        yield break;
      }

      using (_enumerator)
      {
        while (true)
        {
          T _current;
          try
          {
            if (!_enumerator.MoveNext())
              break;

            _current = _enumerator.Current;
          }
          catch (UnauthorizedAccessException)
          {
            break;
          }
          catch (IOException)
          {
            break;
          }

          yield return _current;
        }
      }
    }
  }
}
