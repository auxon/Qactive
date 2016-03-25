using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qactive
{
  public interface IQactiveProvider : IQbservableProvider
  {
    Type SourceType { get; }

    LocalEvaluator ClientEvaluator { get; }

    object Argument { get; }

    IObservable<TResult> Connect<TResult>(Func<QbservableProtocol, Expression> prepareExpression);

    IObservable<ClientTermination> Listen(QbservableServiceOptions options, Func<QbservableProtocol, IParameterizedQbservableProvider> providerFactory);

    void InitializeSecureServer();
  }
}
