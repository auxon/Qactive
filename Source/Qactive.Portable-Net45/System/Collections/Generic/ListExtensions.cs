using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Qactive.System.Collections.Generic
{
  internal static class ListExtensions
  {
    public static IReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list)
    {
      Contract.Requires(list != null);
      Contract.Ensures(Contract.Result<IReadOnlyCollection<T>>() != null);

      return new ReadOnlyCollection<T>(list);
    }
  }
}
