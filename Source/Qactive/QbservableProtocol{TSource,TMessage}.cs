using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Qactive.Properties;

namespace Qactive
{
  public abstract class QbservableProtocol<TSource, TMessage> : QbservableProtocol<TSource>
    where TMessage : IProtocolMessage
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    protected IList<QbservableProtocolSink<TSource, TMessage>> Sinks => sinks;

    private readonly List<QbservableProtocolSink<TSource, TMessage>> sinks = new List<QbservableProtocolSink<TSource, TMessage>>();

    protected QbservableProtocol(TSource source, IRemotingFormatter formatter, CancellationToken cancel)
      : base(source, formatter, cancel)
    {
      Contract.Ensures(IsClient);
    }

    protected QbservableProtocol(TSource source, IRemotingFormatter formatter, QbservableServiceOptions serviceOptions, CancellationToken cancel)
      : base(source, formatter, serviceOptions, cancel)
    {
      Contract.Ensures(!IsClient);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    protected virtual IEnumerable<QbservableProtocolSink<TSource, TMessage>> CreateClientSinks()
    {
      // for derived types
      yield break;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Reviewed")]
    protected virtual IEnumerable<QbservableProtocolSink<TSource, TMessage>> CreateServerSinks()
    {
      // for derived types
      yield break;
    }

    protected abstract ClientDuplexQbservableProtocolSink<TSource, TMessage> CreateClientDuplexSink();

    protected abstract ServerDuplexQbservableProtocolSink<TSource, TMessage> CreateServerDuplexSink();

    internal sealed override IClientDuplexQbservableProtocolSink CreateClientDuplexSinkInternal()
      => CreateClientDuplexSink();

    internal sealed override IServerDuplexQbservableProtocolSink CreateServerDuplexSinkInternal()
      => CreateServerDuplexSink();

    internal sealed override async Task ServerReceiveAsync()
    {
      while (!Cancel.IsCancellationRequested)
      {
        var message = await ReceiveMessageAsync().ConfigureAwait(false);

        if (ServerHandleClientShutdown(message))
        {
          break;
        }
        else if (!message.Handled)
        {
          throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ProtocolUnknownMessageKindFormat, message));
        }
      }
    }

    protected abstract bool ServerHandleClientShutdown(TMessage message);

    protected async Task SendMessageAsync(TMessage message)
    {
      message = await ApplySinksForSending(message).ConfigureAwait(false);

      await SendMessageCoreAsync(message).ConfigureAwait(false);
    }

    protected async Task<TMessage> ReceiveMessageAsync()
    {
      var message = await ReceiveMessageCoreAsync().ConfigureAwait(false);

      return await ApplySinksForReceiving(message).ConfigureAwait(false);
    }

    protected abstract Task SendMessageCoreAsync(TMessage message);

    protected abstract Task<TMessage> ReceiveMessageCoreAsync();

    internal sealed override async Task InitializeSinksAsync()
    {
      if (IsClient)
      {
        sinks.AddRange(CreateClientSinks());
      }
      else
      {
        sinks.AddRange(CreateServerSinks());

        if (ServiceOptions.EnableDuplex)
        {
          sinks.Add(CreateServerDuplexSink());
        }
      }

      foreach (var sink in sinks)
      {
        await sink.InitializeAsync(this, Cancel).ConfigureAwait(false);
      }
    }

    private async Task<TMessage> ApplySinksForSending(TMessage message)
    {
      foreach (var sink in sinks)
      {
        message = await sink.SendingAsync(message, Cancel).ConfigureAwait(false);
      }

      return message;
    }

    private async Task<TMessage> ApplySinksForReceiving(TMessage message)
    {
      foreach (var sink in sinks)
      {
        message = await sink.ReceivingAsync(message, Cancel).ConfigureAwait(false);
      }

      return message;
    }

    public sealed override TSink FindSink<TSink>()
    {
      return sinks.OfType<TSink>().FirstOrDefault();
    }

    public sealed override TSink GetOrAddSink<TSink>(Func<TSink> createSink)
    {
      var sink = FindSink<TSink>();

      if (sink == null)
      {
        sink = createSink();
        sinks.Add((QbservableProtocolSink<TSource, TMessage>)(object)sink);
      }

      return sink;
    }
  }
}