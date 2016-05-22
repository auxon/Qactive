using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace Qactive.Tests
{
  internal static class ObservableExtensions
  {
    public static IObservable<T> DebugWriteLine<T>(this IObservable<T> source, [CallerMemberName]string label = null)
      => DebugWriteLine(source, null, label);

    public static IObservable<T> DebugWriteLine<T>(this IObservable<T> source, string format, [CallerMemberName]string label = null)
      => source.Do(
        value => Debug.WriteLine((label == null ? null : label + " ") + "OnNext: " + (format ?? "{0}"), value),
        ex => Debug.WriteLine((label == null ? null : label + " ") + "OnError: " + ex.Message),
        () => Debug.WriteLine((label == null ? null : label + " ") + "OnCompleted"));
  }
}
