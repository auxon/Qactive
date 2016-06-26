using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
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

    public static IQbservable<T> DebugWriteLine<T>(this IQbservable<T> source, [CallerMemberName]string label = null)
      => source.Provider.CreateQuery<T>(
          Expression.Call(((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                          source.Expression,
                          Expression.Constant(label)));

    public static IQbservable<T> DebugWriteLine<T>(this IQbservable<T> source, string format, [CallerMemberName]string label = null)
      => source.Provider.CreateQuery<T>(
          Expression.Call(((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                          source.Expression,
                          Expression.Constant(format),
                          Expression.Constant(label)));
  }
}
