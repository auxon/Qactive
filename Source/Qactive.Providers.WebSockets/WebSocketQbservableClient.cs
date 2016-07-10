using System;
using System.Diagnostics.Contracts;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;

namespace Qactive
{
  public sealed class WebSocketQbservableClient<TSource>
  {
    private readonly Uri uri;
    private readonly IRemotingFormatter formatter;
    private readonly LocalEvaluator localEvaluator;

    public WebSocketQbservableClient(Uri uri)
      : this(uri, WebSocketQactiveDefaults.CreateDefaultFormatter())
    {
      Contract.Requires(uri != null);
    }

    public WebSocketQbservableClient(Uri uri, params Type[] knownTypes)
      : this(uri, WebSocketQactiveDefaults.CreateDefaultFormatter(), knownTypes)
    {
      Contract.Requires(uri != null);
    }

    public WebSocketQbservableClient(Uri uri, LocalEvaluator localEvaluator)
      : this(uri, WebSocketQactiveDefaults.CreateDefaultFormatter(), localEvaluator)
    {
      Contract.Requires(uri != null);
      Contract.Requires(localEvaluator != null);
    }

    public WebSocketQbservableClient(Uri uri, IRemotingFormatter formatter)
      : this(uri, formatter, new ImmediateLocalEvaluator())
    {
      Contract.Requires(uri != null);
      Contract.Requires(formatter != null);
    }

    public WebSocketQbservableClient(Uri uri, IRemotingFormatter formatter, params Type[] knownTypes)
      : this(uri, formatter, new ImmediateLocalEvaluator(knownTypes))
    {
      Contract.Requires(uri != null);
      Contract.Requires(formatter != null);
    }

    public WebSocketQbservableClient(Uri uri, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
    {
      Contract.Requires(uri != null);
      Contract.Requires(formatter != null);
      Contract.Requires(localEvaluator != null);

      this.uri = uri;
      this.formatter = formatter;
      this.localEvaluator = localEvaluator;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(uri != null);
      Contract.Invariant(formatter != null);
      Contract.Invariant(localEvaluator != null);
    }

    public IQbservable<TSource> Query()
    {
      Contract.Ensures(Contract.Result<IQbservable<TSource>>() != null);

      return WebSocketQactiveProvider.Client(typeof(TSource), uri, Nop.Action, formatter, localEvaluator).CreateQuery<TSource>();
    }

    public IQbservable<TSource> Query(object argument)
    {
      Contract.Ensures(Contract.Result<IQbservable<TSource>>() != null);

      return WebSocketQactiveProvider.Client(typeof(TSource), uri, Nop.Action, formatter, localEvaluator, argument).CreateQuery<TSource>();
    }

    public IQbservable<TSource> Query(Action<WebSocket> prepareSocket)
    {
      Contract.Requires(prepareSocket != null);
      Contract.Ensures(Contract.Result<IQbservable<TSource>>() != null);

      return WebSocketQactiveProvider.Client(typeof(TSource), uri, prepareSocket, formatter, localEvaluator).CreateQuery<TSource>();
    }

    public IQbservable<TSource> Query(Action<WebSocket> prepareSocket, object argument)
    {
      Contract.Requires(prepareSocket != null);
      Contract.Ensures(Contract.Result<IQbservable<TSource>>() != null);

      return WebSocketQactiveProvider.Client(typeof(TSource), uri, prepareSocket, formatter, localEvaluator, argument).CreateQuery<TSource>();
    }
  }
}