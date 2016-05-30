using System.Collections;
using System.Diagnostics.Contracts;

namespace Qactive
{
  internal sealed class NamedEnumerator : IEnumerator
  {
    public string Name { get; }

    public object Current
      => enumerator.Current;

    public IEnumerator Decorated
      => enumerator;

    private readonly IEnumerator enumerator;

    public NamedEnumerator(string name, IEnumerator enumerator)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(enumerator != null);

      Name = name;
      this.enumerator = enumerator;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(!string.IsNullOrEmpty(Name));
      Contract.Invariant(enumerator != null);
    }

    public bool MoveNext()
      => enumerator.MoveNext();

    public void Reset()
      => enumerator.Reset();
  }
}
