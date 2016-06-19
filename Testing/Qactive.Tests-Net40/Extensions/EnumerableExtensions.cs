using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Qactive.Tests
{
  internal static class EnumerableExtensions
  {
    public static IReadOnlyCollection<T> AsInterface<T>(this ReadOnlyCollection<T> source)
    {
      return new ReadOnlyCollectionExtension<T>(source);
    }

    public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> source)
    {
      return new ReadOnlyCollectionExtension<T>(source);
    }

    private sealed class ReadOnlyCollectionExtension<T> : IReadOnlyCollection<T>
    {
      private readonly ICollection<T> source;

      public ReadOnlyCollectionExtension(ICollection<T> source)
      {
        this.source = source;
      }

      public int Count => source.Count;

      public IEnumerator<T> GetEnumerator() => source.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
  }
}
