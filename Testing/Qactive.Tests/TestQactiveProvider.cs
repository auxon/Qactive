using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.ExceptionServices;

namespace Qactive.Tests
{
  internal sealed class TestQactiveProvider : QactiveProvider
  {
    private readonly ReplaySubject<DuplexSubject> clients = new ReplaySubject<DuplexSubject>();

    protected override object Id => GetHashCode();

    private TestQactiveProvider(Type sourceType, LocalEvaluator localEvaluator)
      : base(sourceType, localEvaluator)
    {
    }

    private TestQactiveProvider(Type sourceType, LocalEvaluator localEvaluator, object argument)
      : base(sourceType, localEvaluator, argument)
    {
    }

    public static TestQactiveProvider Create<T>(params Type[] knownTypes)
      => new TestQactiveProvider(typeof(T), new ImmediateLocalEvaluator((knownTypes ?? Enumerable.Empty<Type>()).Concat(new[] { typeof(ObservableExtensions) }).ToArray()));

    public static TestQactiveProvider Create<T>(LocalEvaluator localEvaluator)
      => new TestQactiveProvider(typeof(T), localEvaluator);

    public static TestQactiveProvider Create<T>(LocalEvaluator localEvaluator, object argument)
      => new TestQactiveProvider(typeof(T), localEvaluator, argument);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The SocketAsyncEventArgs instance is either disposed before returning or by the observable's Finally operator.")]
    public override IObservable<TResult> Connect<TResult>(Func<IQbservableProtocol, Expression> prepareExpression)
      => from transport in Observable.Return(new DuplexSubject()).Do(clients.OnNext)
         let protocol = new TestQbservableProtocol(transport.GetHashCode(), transport.NextLeft, transport.Right)
         from result in protocol
          .ExecuteClient<TResult>(prepareExpression(protocol), Argument)
          .Finally(protocol.Dispose)
         select result;

    public override IObservable<ClientTermination> Listen(
      QbservableServiceOptions options,
      Func<IQbservableProtocol, IParameterizedQbservableProvider> providerFactory)
      => from transport in clients
         from result in Observable.FromAsync(async () =>
         {
           // TODO: Most of this code is boiler-plate and should be moved into the core library.
           var watch = Stopwatch.StartNew();

           var exceptions = new List<ExceptionDispatchInfo>();
           var shutdownReason = QbservableProtocolShutdownReason.None;

           try
           {
             using (var protocol = new TestQbservableProtocol(transport.GetHashCode(), transport.NextRight, transport.Left, options))
             {
               var provider = providerFactory(protocol);

               try
               {
                 await protocol.ExecuteServerAsync(provider).ConfigureAwait(false);
               }
               catch (OperationCanceledException)
               {
               }
               catch (Exception ex)
               {
                 exceptions.Add(ExceptionDispatchInfo.Capture(ex));
               }

               var protocolExceptions = protocol.Exceptions;

               if (protocolExceptions != null)
               {
                 foreach (var exception in protocolExceptions)
                 {
                   exceptions.Add(exception);
                 }
               }

               shutdownReason = protocol.ShutdownReason;
             }
           }
           catch (OperationCanceledException)
           {
             shutdownReason = QbservableProtocolShutdownReason.ProtocolNegotiationCanceled;
           }
           catch (Exception ex)
           {
             shutdownReason = QbservableProtocolShutdownReason.ProtocolNegotiationError;

             exceptions.Add(ExceptionDispatchInfo.Capture(ex));
           }

           return new ClientTermination(watch.Elapsed, shutdownReason, exceptions);
         })
         select result;
  }
}
