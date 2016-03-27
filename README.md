<img align="right" src="https://raw.githubusercontent.com/RxDave/Qactive/master/Artifacts/Logo.png" />
# Qactive
A reactive queryable observable framework.

* **Runtime:** .NET Framework 4.6.1.
* **Development:** Visual Studio 2015 and C# 6
* **Dependencies:** [Reactive Extensions (Rx.NET)](https://github.com/Reactive-Extensions/Rx.NET)

## Download from NuGet
> [Qactive.Providers.Tcp](https://www.nuget.org/packages/qactive.providers.tcp) (depends on Qactive and Qactive.Expressions)

> [Qactive](https://www.nuget.org/packages/qactive) (depends on Qactive.Expressions)

> [Qactive.Expressions](https://www.nuget.org/packages/qactive.expressions)

Add a reference to the **Qactive.Providers.Tcp** package in your Visual Studio project. That package references the other packages as dependencies, so NuGet will automatically download all of them for you.

Currently, the TCP provider is the only provider available.

## Overview
Qactive builds on Reactive Extension's queryable observable providers, enabling you to write elegant reactive queries that execute server-side, even though they are written on the client.
Qactive makes the extremely powerful act of querying a reactive service as easy as writing a typical Rx query.

More specifically, Qactive enables you to easily expose `IQbservable<T>` services for clients to query. When a client defines a query and subscribes, a connection is made to the server and the 
serialized query is transmitted to the server as an expression tree. The server deserializes the expression tree and executes it as a standing query. Any output from the query is marshaled back 
to the client over a persistent, full-duplex connection. Members on closures and static members that are local to the client are invoked from within the service automatically via full-duplex 
messaging. Anonymous types are automatically serialized as well.

For more information, see [this series of blog posts](http://davesexton.com/blog/page/TCP-Qbservable-Provider-Series.aspx).

> **Warning:** Qactive allows clients to execute arbitrary code on your server.
> There are security mechanisms in place by default to prevent malicious clients but only to a point, 
> it hasn't been fully considered yet. Do not expose a Qbservable service on a public server without 
> taking the necessary precautions to secure it first.

## Features
* Works immediately with pre-built transport providers.
  * TCP with binary serialization is provided by the optional Qactive.Providers.Tcp package on NuGet.
  * Extensible so that any kind of custom transport and/or serialization mechanism can be used.
* Simple server factory methods for hosting a Qbservable service.
  * Supports hosting any `IObservable<T>` query as a service (_hot_ or _cold_).
  * Supports hosting any `IQbservable<T>` query as a service.
* Simple client factory methods for acquiring a Qbservable service.
  * You must only specify the end point address and the expected return type.  The result is an `IQbservable<T>` that you can query and `Subscribe`.
  * All Qbservable Rx operators are supported.
* Automatically serialized Expression trees.
  * Dynamic expressions and debug info expressions are not supported.  All other types of expressions are supported.
* Automatically serialized anonymous types.
* Immediate evaluation of local members and closures (optional; default behavior)
  * Compiler-generated methods are executed locally and replaced with their return values before the expression is transmitted to the server.  This includes iterator blocks, which are serialized as `List<T>`.
  * Evaluation assumes that local methods are never executed for their side-effects.  Actions (void-returning methods) cause an exception.  Do not depend upon the order in which members are invoked.
* Full duplex communication (optional; default behavior for `IObservable<T>` closures)
  * Must opt-in on server.
  * May opt-in on client for full duplex communication of all local members; automatic for `IObservable<T>` closures.
  * Duplex communication automatically supports iterator blocks.
* Designed with extensibility in mind; e.g., supports custom Qbservable service providers, protocols and sinks.

## Example
The following example creates a _cold_ observable sequence that generates a new notification every second and exposes it as an `IQbservable<long>` service over TCP port 3205 on the local computer.

### Server
```c#
IObservable<long> source = Observable.Interval(TimeSpan.FromSeconds(1));

var service = source.ServeQbservableTcp(new IPEndPoint(IPAddress.Loopback, 3205));

using (service.Subscribe(
  client => Console.WriteLine("Client shutdown."),
  ex => Console.WriteLine("Fatal error: {0}", ex.Message),
  () => Console.WriteLine("This will never be printed because a service host never completes.")))
{
  Console.ReadKey();
}
```
The following example creates a LINQ query over the `IQbservable<long>` service that is created by the previous example.  Subscribing to the query on the client causes the query to be serialized to the server and executed there.  In other words, the `where` clause is actually executed on the server so that the client only receives the data that it requested without having to do any filtering itself.  The client will receive the first six values, one per second.  The server then filters out the next 2 values - it does not send them to the client.  Finally, the remaining values are sent to the client until either the client or the server disposes of the subscription.

### Client
```c#
var client = new TcpQbservableClient<long>(new IPEndPoint(IPAddress.Loopback, 3205));

IQbservable<long> query =
  from value in client.Query()
  where value <= 5 || value >= 8
  select value;

using (query.Subscribe(
  value => Console.WriteLine("Client observed: " + value),
  ex => Console.WriteLine("Error: {0}", ex.Message),
  () => Console.WriteLine("Completed")))
{
  Console.ReadKey();
}
```
## Getting Started
Qactive is a set of .NET class libraries that you can reference in your projects. NuGet is recommended.

### To run the examples:
1. Run _QbservableServer.exe_.
  1. The server will start hosting example Qbservable services as soon as the console application begins.
  1. Pressing a key at any time will stop the server.
1. Run _QbservableClient.exe_.
  1. You can run several client console applications at the same time.
1. When the client console application starts, press any key to connect to the server.  The client will begin running the first example.
1. Press any key to stop the current example and start the following example.

### To build the source code:
1. Set the *QbservableServer* project as the startup project.
1. Build and run. The server will start as soon as the console application begins.
1. Set the *QbservableClient* project as the startup project.
1. Build and run. You can run several client console applications at the same time.
1. When the client console application starts, press any key to connect to the server.

> **Tip:** To see the original and rewritten expression trees, run the client application with the debugger attached and look at the **Output** window.

## Planning
1. Research building on top of WCF to support advanced configuration, customization, extensibility, standardization and additional transports such as WebSockets over HTTP.
1. Improve expression tree serialization; e.g., fix bugs and write unit tests.
1. Consider security.
1. Consider memory/performance.
1. Support querying from Silverlight 5 and Windows Phone 7.5 apps.  (This may require WebSockets though.)
1. Support querying from RxJS clients.
