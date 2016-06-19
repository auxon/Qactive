using System.Collections.Generic;

namespace Qactive.Tests
{
  internal interface IReadOnlyCollection<T> : IEnumerable<T>
  {
    int Count { get; }
  }
}
