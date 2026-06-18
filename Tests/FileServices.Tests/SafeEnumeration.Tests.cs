namespace FileServices.Tests
{
  public class SafeEnumerationTests
  {
    [Fact]
    public void Enumerate_NormalSequence_ReturnsAllItems()
    {
      var result = SafeEnumeration.Enumerate(() => new[] { 1, 2, 3 }.AsEnumerable()).ToList();

      Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void Enumerate_EmptySequence_ReturnsEmpty()
    {
      var result = SafeEnumeration.Enumerate(() => Enumerable.Empty<int>()).ToList();

      Assert.Empty(result);
    }

    [Fact]
    public void Enumerate_FactoryThrowsUnauthorized_ReturnsEmpty()
    {
      var result = SafeEnumeration.Enumerate<int>(() => throw new UnauthorizedAccessException()).ToList();

      Assert.Empty(result);
    }

    [Fact]
    public void Enumerate_FactoryThrowsIO_ReturnsEmpty()
    {
      var result = SafeEnumeration.Enumerate<int>(() => throw new IOException()).ToList();

      Assert.Empty(result);
    }

    [Fact]
    public void Enumerate_MoveNextThrowsUnauthorizedMidway_ReturnsItemsBeforeThrow()
    {
      // Перечислитель отдаёт 1, 2, а затем бросает отказ доступа — уже выданные элементы должны сохраниться
      var result = SafeEnumeration.Enumerate(() => ThrowAfter(2, new UnauthorizedAccessException())).ToList();

      Assert.Equal(new[] { 1, 2 }, result);
    }

    [Fact]
    public void Enumerate_MoveNextThrowsIOMidway_ReturnsItemsBeforeThrow()
    {
      var result = SafeEnumeration.Enumerate(() => ThrowAfter(3, new IOException())).ToList();

      Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void Enumerate_UnexpectedException_Propagates()
    {
      // Прочие исключения не глотаем — они сигнализируют о настоящих ошибках
      Assert.Throws<InvalidOperationException>(() =>
        SafeEnumeration.Enumerate<int>(() => throw new InvalidOperationException()).ToList());
    }

    /// <summary>
    /// Отдаёт числа 1..count, после чего бросает указанное исключение при следующем продвижении.
    /// </summary>
    private static IEnumerable<int> ThrowAfter(int count, Exception exception)
    {
      for (int _i = 1; _i <= count; _i++)
        yield return _i;

      throw exception;
    }
  }
}
