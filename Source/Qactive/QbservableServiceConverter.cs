using System;
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
      this.service = service;
    }

    public IQbservable<TResult> Convert(IObservable<TSource> request)
    {
      return service(request).AsQbservable();
    }
  }
}