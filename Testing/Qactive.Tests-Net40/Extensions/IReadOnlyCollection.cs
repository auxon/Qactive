using System.Collections;
using System.Collections.Generic;

namespace Qactive
{
    public interface IReadOnlyCollection<out T> : IEnumerable<T>, IEnumerable
    {
        int Count { get; }
        new IEnumerable<T> GetEnumerator();
    }

    internal interface IReadOnlyCollection<T> : IEnumerable<T>
  {
    int Count { get; }
  }
}
