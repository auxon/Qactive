using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Qactive
{
  [Serializable]
  internal sealed class DuplexCallbackEnumerable<T> : DuplexCallback, IEnumerable<T>
  {
    public DuplexCallbackEnumerable(int id)
      : base(id)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    public IEnumerator<T> GetEnumerator()
    {
      var protocol = Protocol ?? Sink.Protocol;

      // A try..catch block is required because the Rx SelectMany operator doesn't send an exception from GetEnumerator to OnError.
      try
      {
        var enumeratorId = Sink.GetEnumerator(Id);

        return new DuplexCallbackEnumerator(enumeratorId, protocol, Sink);
      }
      catch (Exception ex)
      {
        protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));

        return Enumerable.Empty<T>().GetEnumerator();
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private sealed class DuplexCallbackEnumerator : IEnumerator<T>
    {
      public T Current => (T)current;

      object System.Collections.IEnumerator.Current => Current;

      private readonly int enumeratorId;
      private readonly IQbservableProtocol protocol;
      private readonly IServerDuplexQbservableProtocolSink sink;
      private object current;

      public DuplexCallbackEnumerator(int enumeratorId, IQbservableProtocol protocol, IServerDuplexQbservableProtocolSink sink)
      {
        Contract.Requires(protocol != null);
        Contract.Requires(sink != null);

        this.enumeratorId = enumeratorId;
        this.protocol = protocol;
        this.sink = sink;
      }

      [ContractInvariantMethod]
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
      private void ObjectInvariant()
      {
        Contract.Invariant(protocol != null);
        Contract.Invariant(sink != null);
      }

      public bool MoveNext()
      {
        var result = sink.MoveNext(enumeratorId);

        if (result.Item1)
        {
          current = result.Item2;
        }

        return result.Item1;
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
      public void Reset()
      {
        // A try..catch block may be required, though Rx doesn't call the Reset method at all.
        try
        {
          sink.ResetEnumerator(enumeratorId);
        }
        catch (Exception ex)
        {
          protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));
        }
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
      public void Dispose()
      {
        // A try..catch block is required because the Rx SelectMany operator doesn't send an exception from Dispose to OnError.
        try
        {
          sink.DisposeEnumerator(enumeratorId);
        }
        catch (Exception ex)
        {
          protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));
        }
      }
    }
  }
}