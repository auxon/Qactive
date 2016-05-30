using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Qactive
{
#if SERIALIZATION
  [Serializable]
#endif
  internal sealed class DuplexCallbackEnumerable<T> : DuplexCallback, IEnumerableDuplexCallback, IEnumerable<T>
  {
#if SERIALIZATION
    [NonSerialized]
#endif
    private readonly IEnumerable instance;

    public DuplexCallbackEnumerable(string name, int id, object clientId, IEnumerable instance)
      : base(name, id, clientId)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(clientId != null);
      Contract.Requires(instance != null);

      this.instance = instance;
    }

    /// <summary>
    /// Called client-side.
    /// </summary>
    IEnumerator IEnumerableDuplexCallback.GetEnumerator()
      => instance.GetEnumerator();

    /// <summary>
    /// Called server-side.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    public IEnumerator<T> GetEnumerator()
    {
      var protocol = Protocol ?? Sink.Protocol;

      // A try..catch block is required because the Rx SelectMany operator doesn't send an exception from GetEnumerator to OnError.
      try
      {
        var enumeratorId = Sink.GetEnumerator(Name, Id);

        return new DuplexCallbackEnumerator(Name, enumeratorId, protocol, Sink);
      }
      catch (Exception ex)
      {
        protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));

        return Enumerable.Empty<T>().GetEnumerator();
      }
    }

    /// <summary>
    /// Called server-side.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
      => GetEnumerator();

    private sealed class DuplexCallbackEnumerator : IEnumerator<T>
    {
      public T Current => (T)current;

      object System.Collections.IEnumerator.Current => Current;

      private readonly string name;
      private readonly int enumeratorId;
      private readonly IQbservableProtocol protocol;
      private readonly IServerDuplexQbservableProtocolSink sink;
      private object current;

      public DuplexCallbackEnumerator(string name, int enumeratorId, IQbservableProtocol protocol, IServerDuplexQbservableProtocolSink sink)
      {
        Contract.Requires(!string.IsNullOrEmpty(name));
        Contract.Requires(protocol != null);
        Contract.Requires(sink != null);

        this.name = name;
        this.enumeratorId = enumeratorId;
        this.protocol = protocol;
        this.sink = sink;
      }

      [ContractInvariantMethod]
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
      private void ObjectInvariant()
      {
        Contract.Invariant(!string.IsNullOrEmpty(name));
        Contract.Invariant(protocol != null);
        Contract.Invariant(sink != null);
      }

      /// <summary>
      /// Called server-side.
      /// </summary>
      public bool MoveNext()
      {
        var result = sink.MoveNext(name, enumeratorId);

        if (result.Item1)
        {
          current = result.Item2;
        }

        return result.Item1;
      }

      /// <summary>
      /// Called server-side.
      /// </summary>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
      public void Reset()
      {
        // A try..catch block may be required, though Rx doesn't call the Reset method at all.
        try
        {
          sink.ResetEnumerator(name, enumeratorId);
        }
        catch (Exception ex)
        {
          protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));
        }
      }

      /// <summary>
      /// Called server-side.
      /// </summary>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
      public void Dispose()
      {
        // A try..catch block is required because the Rx SelectMany operator doesn't send an exception from Dispose to OnError.
        try
        {
          sink.DisposeEnumerator(name, enumeratorId);
        }
        catch (Exception ex)
        {
          protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));
        }
      }
    }
  }
}