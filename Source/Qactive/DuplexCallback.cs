using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Qactive.Properties;

namespace Qactive
{
  [Serializable]
  internal class DuplexCallback
  {
    public bool CanInvoke => sink != null && protocol != null;

    protected IServerDuplexQbservableProtocolSink Sink => sink;

    protected IQbservableProtocol Protocol => protocol;

    protected int Id => id;

    private static readonly MethodInfo serverInvokeMethod = typeof(DuplexCallback)
      .GetMethods()
      .Where(m => m.IsGenericMethod && m.Name == "ServerInvoke")
      .First();

    private static readonly MethodInfo serverInvokeVoidMethod = typeof(DuplexCallback)
      .GetMethods()
      .Where(m => !m.IsGenericMethod && m.Name == "ServerInvoke")
      .First();

    [NonSerialized]
    private IServerDuplexQbservableProtocolSink sink;
    [NonSerialized]
    private IQbservableProtocol protocol;
    private readonly int id;

    protected DuplexCallback(int id)
    {
      this.id = id;
    }

    private DuplexCallback(IQbservableProtocol protocol, Func<int, object[], object> callback)
    {
      this.id = protocol
        .GetOrAddSink(protocol.CreateClientDuplexSink)
        .RegisterInvokeCallback(arguments => callback(this.id, arguments));
    }

    public static Expression Create(IQbservableProtocol protocol, object instance, PropertyInfo property)
    {
      return CreateInvoke(
        new DuplexCallback(protocol, (_, __) => ConvertIfSequence(protocol, property.GetValue(instance))),
        property.PropertyType);
    }

    public static Expression Create(IQbservableProtocol protocol, object instance, FieldInfo field)
    {
      return CreateInvoke(
        new DuplexCallback(protocol, (_, __) => ConvertIfSequence(protocol, field.GetValue(instance))),
        field.FieldType);
    }

    public static Expression Create(IQbservableProtocol protocol, object instance, MethodInfo method, IEnumerable<Expression> argExpressions)
    {
      return CreateInvoke(
        new DuplexCallback(protocol, (_, arguments) => ConvertIfSequence(protocol, method.Invoke(instance, arguments))),
        method.ReturnType,
        argExpressions);
    }

    public static Expression CreateEnumerable(IQbservableProtocol protocol, object instance, Type dataType, Type type)
    {
      return Expression.Constant(
        CreateRemoteEnumerable(protocol, (IEnumerable)instance, dataType),
        type);
    }

    public static Expression CreateObservable(IQbservableProtocol protocol, object instance, Type dataType, Type type)
    {
      return Expression.Constant(
        CreateRemoteObservable(protocol, instance, dataType),
        type);
    }

    private static Expression CreateInvoke(DuplexCallback callback, Type returnType, IEnumerable<Expression> arguments = null)
    {
      return Expression.Call(
        Expression.Constant(callback),
        returnType == typeof(void) ? DuplexCallback.serverInvokeVoidMethod : DuplexCallback.serverInvokeMethod.MakeGenericMethod(returnType),
        Expression.NewArrayInit(
          typeof(object),
          (arguments == null ? new Expression[0] : arguments.Select(a => (Expression)Expression.Convert(a, typeof(object))))));
    }

    private static object ConvertIfSequence(IQbservableProtocol protocol, object instance)
    {
      if (instance != null)
      {
        var type = instance.GetType();

        if (!type.IsSerializable)
        {
          var observableType = type.GetGenericInterfaceFromDefinition(typeof(IObservable<>));

          if (observableType != null)
          {
            return CreateRemoteObservable(protocol, instance, observableType.GetGenericArguments()[0]);
          }

          var enumerableType = type.GetGenericInterfaceFromDefinition(typeof(IEnumerable<>));
          var enumerable = instance as IEnumerable;

          if (enumerableType != null)
          {
            return CreateRemoteEnumerable(protocol, enumerable, enumerableType.GetGenericArguments()[0]);
          }
          else if (enumerable != null)
          {
            return CreateRemoteEnumerable(protocol, enumerable.Cast<object>(), typeof(object));
          }
        }
      }

      return instance;
    }

    private static object CreateRemoteEnumerable(IQbservableProtocol protocol, IEnumerable instance, Type dataType)
    {
      var sink = protocol.GetOrAddSink(protocol.CreateClientDuplexSink);

      int id = 0;
      id = sink.RegisterEnumerableCallback(instance.GetEnumerator);

      return Activator.CreateInstance(typeof(DuplexCallbackEnumerable<>).MakeGenericType(dataType), id);
    }

    private static object CreateRemoteObservable(IQbservableProtocol protocol, object instance, Type dataType)
    {
      var sink = protocol.GetOrAddSink(protocol.CreateClientDuplexSink);

      int id = 0;
      id = sink.RegisterObservableCallback(serverId => Subscribe(sink, new DuplexCallbackId(id, serverId), instance, dataType));

      return Activator.CreateInstance(typeof(DuplexCallbackObservable<>).MakeGenericType(dataType), id);
    }

    private static IDisposable Subscribe(IClientDuplexQbservableProtocolSink sink, DuplexCallbackId id, object instance, Type dataType)
    {
      return dataType.UpCast(instance).Subscribe(
        value => sink.SendOnNext(id, value),
        ex => sink.SendOnError(id, ExceptionDispatchInfo.Capture(ex)),
        () => sink.SendOnCompleted(id));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "protocol", Justification = "It's setting the field.")]
    public void SetServerProtocol(IQbservableProtocol protocol)
    {
      Contract.Requires(protocol != null);

      this.protocol = protocol;
      this.sink = protocol.FindSink<IServerDuplexQbservableProtocolSink>();

      if (sink == null)
      {
        throw new InvalidOperationException(Errors.ProtocolDuplexSinkUnavailableForClientCallback);
      }
    }

    public TResult ServerInvoke<TResult>(object[] arguments)
    {
      Contract.Requires(CanInvoke);

      try
      {
        var value = (TResult)sink.Invoke(id, arguments);

        var callback = value as DuplexCallback;

        if (callback != null)
        {
          callback.sink = sink;
        }

        return value;
      }
      catch (Exception ex)
      {
        protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));
        throw;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "There is no meaningful way to handle exceptions here other than passing them to a handler, and we cannot let them leave this context because they will be missed.")]
    public void ServerInvoke(object[] arguments)
    {
      Contract.Requires(CanInvoke);

      try
      {
        sink.Invoke(id, arguments);
      }
      catch (Exception ex)
      {
        protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));
      }
    }
  }
}