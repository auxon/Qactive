using System;
using System.Diagnostics.Contracts;

namespace Qactive
{
  internal sealed class NamedDisposable : IDisposable
  {
    public string Name { get; }

    private readonly IDisposable disposable;

    public NamedDisposable(string name, IDisposable disposable)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(disposable != null);

      Name = name;
      this.disposable = disposable;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(!string.IsNullOrEmpty(Name));
      Contract.Invariant(disposable != null);
    }

    public void Dispose()
      => disposable.Dispose();
  }
}
