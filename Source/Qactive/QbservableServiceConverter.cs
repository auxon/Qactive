using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;

namespace Qactive
{
  // This class must be public otherwise CreateService fails inside of a new AppDomain - see CreateServiceProxy comments
  [Serializable]
  public sealed class QbservableServiceConverter<TSource, TResult>
  {
    private readonly Func<IObservable<TSource>, IObservable<TResult>> service;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    public QbservableServiceConverter(Func<IObservable<TSource>, IObservable<TResult>> service)
    {
      Contract.Requires(service != null);

      this.service = service;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(service != null);
    }

    public IQbservable<TResult> Convert(IObservable<TSource> request)
    {
      Contract.Requires(request != null);
      Contract.Ensures(Contract.Result<IQbservable<TResult>>() != null);

      return service(request).AsQbservable();
    }
  }
}