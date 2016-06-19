/* Portions retrieved from https://github.com/Reactive-Extensions/Rx.NET/ on June 19, 2016 by D.S.
 * 
 * Mods:
 * AsyncSubject usage replaced with a new type, AwaitableAsyncSubject, with the same impl as 2.2.5.
 * Added contracts in place of legacy preconditions.
 * Small code style updates.
 * 
 */

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information. 
using System.Diagnostics.Contracts;
using System.Reactive.Subjects;

namespace System.Reactive.Linq
{
  internal static class ObservableExtensions2
  {
    public static AwaitableAsyncSubject<TSource> GetAwaiter<TSource>(this IObservable<TSource> source)
    {
      Contract.Requires(source != null);
      Contract.Ensures(Contract.Result<AwaitableAsyncSubject<TSource>>() != null);

      var s = new AwaitableAsyncSubject<TSource>();
      var d = source.SubscribeSafe(s);
      return s;
    }
  }
}
