using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Qactive.Tests
{
  internal sealed class TestQactiveProvider : QactiveProvider
  {
    private readonly ReplaySubject<LocalTransport> clients = new ReplaySubject<LocalTransport>();

    public IRemotingFormatter Formatter { get; }

    private TestQactiveProvider(Type sourceType, IRemotingFormatter formatter, LocalEvaluator localEvaluator)
      : base(sourceType, localEvaluator)
    {
      Formatter = formatter;
    }

    private TestQactiveProvider(Type sourceType, IRemotingFormatter formatter, LocalEvaluator localEvaluator, object argument)
      : base(sourceType, localEvaluator, argument)
    {
      Formatter = formatter;
    }

    public static TestQactiveProvider Create<T>(params Type[] knownTypes)
      => new TestQactiveProvider(typeof(T), new BinaryFormatter(), new ImmediateLocalEvaluator(knownTypes));

    public static TestQactiveProvider Create<T>(IRemotingFormatter formatter, params Type[] knownTypes)
      => new TestQactiveProvider(typeof(T), formatter, new ImmediateLocalEvaluator(knownTypes));

    public static TestQactiveProvider Create<T>(IRemotingFormatter formatter, LocalEvaluator localEvaluator)
      => new TestQactiveProvider(typeof(T), formatter, localEvaluator);

    public static TestQactiveProvider Create<T>(IRemotingFormatter formatter, LocalEvaluator localEvaluator, object argument)
      => new TestQactiveProvider(typeof(T), formatter, localEvaluator, argument);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The SocketAsyncEventArgs instance is either disposed before returning or by the observable's Finally operator.")]
    public override IObservable<TResult> Connect<TResult>(Func<QbservableProtocol, Expression> prepareExpression)
      => from transport in Observable.Return(new LocalTransport()).Do(clients.OnNext)
         from protocol in QbservableProtocol.NegotiateClientAsync(transport.Left, Formatter, CancellationToken.None).ToObservable()
         from result in protocol
          .ExecuteClient<TResult>(prepareExpression(protocol), Argument)
          .Finally(protocol.Dispose)
         select result;

    public override IObservable<ClientTermination> Listen(
      QbservableServiceOptions options,
      Func<QbservableProtocol, IParameterizedQbservableProvider> providerFactory)
      => from transport in clients
         from result in Observable.StartAsync(async cancel =>
          {
            var watch = Stopwatch.StartNew();

            var exceptions = new List<ExceptionDispatchInfo>();
            var shutdownReason = QbservableProtocolShutdownReason.None;

            try
            {
              using (var protocol = await QbservableProtocol.NegotiateServerAsync(transport.Right, Formatter, options, cancel).ConfigureAwait(false))
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

            return new ClientTermination(new IPEndPoint(IPAddress.Loopback, 0), new IPEndPoint(IPAddress.Loopback, 0), watch.Elapsed, shutdownReason, exceptions);
          })
         select result;

    protected override object GetCurrentId()
      => null;
  }
}
