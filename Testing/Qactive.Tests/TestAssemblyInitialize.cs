using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests
{
  [TestClass]
  public sealed class TestAssemblyInitialize
  {
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
      var testListener = new TraceListenerGate(Debug.Listeners.OfType<TextWriterTraceListener>().FirstOrDefault());

      QactiveTraceSources.Qactive.Listeners.Add(testListener);

#if !DEBUG  // Expressions are sent to Debug.WriteLine automatically in DEBUG builds, and the test runner receives them. Adding the listener here causes duplicate entries in the output log.
      QactiveTraceSources.QactiveExpressions.Listeners.Add(testListener);
#endif

      QactiveTraceSources.Qactive.Switch.Level = SourceLevels.Verbose;
      QactiveTraceSources.QactiveExpressions.Switch.Level = SourceLevels.Verbose;
    }

    private sealed class TraceListenerGate : TraceListener
    {
      private readonly object gate = new object();
      private readonly TraceListener listener;

      public override bool IsThreadSafe => true;

      public override string Name { get { return listener.Name; } set { listener.Name = value; } }

      public TraceListenerGate(TraceListener listener)
      {
        this.listener = listener;
      }

      private void Lock(Action action)
      {
        lock (gate)
        {
          action();
        }
      }

      public override void Write(string message) => Lock(() => listener.Write(message));
      public override void WriteLine(string message) => Lock(() => listener.WriteLine(message));
      public override void Close() => Lock(() => listener.Close());
      public override void Fail(string message) => Lock(() => listener.Fail(message));
      public override void Fail(string message, string detailMessage) => Lock(() => listener.Fail(message, detailMessage));
      public override void Flush() => Lock(() => listener.Flush());
      public override void Write(object o) => Lock(() => listener.Write(o));
      public override void Write(object o, string category) => Lock(() => listener.Write(o, category));
      public override void Write(string message, string category) => Lock(() => listener.Write(message, category));
      public override void WriteLine(object o) => Lock(() => listener.WriteLine(o));
      public override void WriteLine(object o, string category) => Lock(() => listener.WriteLine(o, category));
      public override void WriteLine(string message, string category) => Lock(() => listener.WriteLine(message, category));

      public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        => Lock(() => listener.TraceData(eventCache, source, eventType, id, data));

      public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        => Lock(() => listener.TraceData(eventCache, source, eventType, id, data));

      public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        => Lock(() => listener.TraceEvent(eventCache, source, eventType, id));

      public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        => Lock(() => listener.TraceEvent(eventCache, source, eventType, id, format, args));

      public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        => Lock(() => listener.TraceEvent(eventCache, source, eventType, id, message));

      public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        => Lock(() => listener.TraceTransfer(eventCache, source, id, message, relatedActivityId));

      protected override void Dispose(bool disposing)
      {
        if (disposing)
        {
          listener.Dispose();
        }
      }
    }
  }
}