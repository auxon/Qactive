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
#if SERIALIZATION
  [Serializable]
#endif
  internal class DuplexCallback
  {
    public bool CanInvoke => sink != null && protocol != null;

    public IQbservableProtocol Protocol => protocol;

    protected IServerDuplexQbservableProtocolSink Sink => sink;

    protected int Id => id;

    private static readonly MethodInfo serverInvokeMethod = typeof(DuplexCallback)
      .GetMethods()
      .Where(m => m.IsGenericMethod && m.Name == "ServerInvoke")
      .First();

    private static readonly MethodInfo serverInvokeVoidMethod = typeof(DuplexCallback)
      .GetMethods()
      .Where(m => !m.IsGenericMethod && m.Name == "ServerInvoke")
      .First();

#if SERIALIZATION
    [NonSerialized]
#endif
    private IServerDuplexQbservableProtocolSink sink;

#if SERIALIZATION
    [NonSerialized]
#endif
    private IQbservableProtocol protocol;

    private readonly int id;

    protected DuplexCallback(int id)
    {
      this.id = id;
    }

    private DuplexCallback(IQbservableProtocol protocol, Func<int, object[], object> callback)
    {
      Contract.Requires(protocol != null);
      Contract.Requires(callback != null);

      id = protocol.GetOrAddSink(protocol.CreateClientDuplexSink)
                   .RegisterInvokeCallback(arguments => callback(this.id, arguments));
    }

    public static Expression Create(IQbservableProtocol protocol, object instance, PropertyInfo property)
    {
      Contract.Requires(protocol != null);
      Contract.Requires(property != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return CreateInvoke(
        new DuplexCallback(protocol, (_, __) => ConvertIfSequence(protocol, property.GetValue(instance))),
        property.PropertyType);
    }

    public static Expression Create(IQbservableProtocol protocol, object instance, FieldInfo field)
    {
      Contract.Requires(protocol != null);
      Contract.Requires(field != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return CreateInvoke(
        new DuplexCallback(protocol, (_, __) => ConvertIfSequence(protocol, field.GetValue(instance))),
        field.FieldType);
    }

    public static Expression Create(IQbservableProtocol protocol, object instance, MethodInfo method, IEnumerable<Expression> argExpressions)
    {
      Contract.Requires(protocol != null);
      Contract.Requires(method != null);
      Contract.Requires(argExpressions != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return CreateInvoke(
        new DuplexCallback(protocol, (_, arguments) => ConvertIfSequence(protocol, method.Invoke(instance, arguments))),
        method.ReturnType,
        argExpressions);
    }

    public static Expression CreateEnumerable(IQbservableProtocol protocol, object instance, Type dataType, Type type)
    {
      Contract.Requires(protocol != null);
      Contract.Requires(dataType != null);
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return Expression.Constant(
        CreateRemoteEnumerable(protocol, (IEnumerable)instance, dataType),
        type);
    }

    public static Expression CreateObservable(IQbservableProtocol protocol, object instance, Type dataType, Type type)
    {
      Contract.Requires(protocol != null);
      Contract.Requires(dataType != null);
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return Expression.Constant(
        CreateRemoteObservable(protocol, instance, dataType),
        type);
    }

    private static Expression CreateInvoke(DuplexCallback callback, Type returnType, IEnumerable<Expression> arguments = null)
    {
      Contract.Requires(callback != null);
      Contract.Requires(returnType != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return Expression.Call(
        Expression.Constant(callback),
        returnType == typeof(void) ? DuplexCallback.serverInvokeVoidMethod : DuplexCallback.serverInvokeMethod.MakeGenericMethod(returnType),
        Expression.NewArrayInit(
          typeof(object),
          (arguments == null ? new Expression[0] : arguments.Select(a => (Expression)Expression.Convert(a, typeof(object))))));
    }

    private static object ConvertIfSequence(IQbservableProtocol protocol, object instance)
    {
      Contract.Requires(protocol != null);

      if (instance != null)
      {
        var type = instance.GetType();

#if SERIALIZATION
        if (!type.IsSerializable)
#endif
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
      Contract.Requires(protocol != null);
      Contract.Requires(instance != null);
      Contract.Requires(dataType != null);
      Contract.Ensures(Contract.Result<object>() != null);

      var sink = protocol.GetOrAddSink(protocol.CreateClientDuplexSink);

      int id = 0;
      id = sink.RegisterEnumerableCallback(instance.GetEnumerator);

      return Activator.CreateInstance(typeof(DuplexCallbackEnumerable<>).MakeGenericType(dataType), id);
    }

    private static object CreateRemoteObservable(IQbservableProtocol protocol, object instance, Type dataType)
    {
      Contract.Requires(protocol != null);
      Contract.Requires(instance != null);
      Contract.Requires(dataType != null);
      Contract.Ensures(Contract.Result<object>() != null);

      var sink = protocol.GetOrAddSink(protocol.CreateClientDuplexSink);

      int id = 0;
      id = sink.RegisterObservableCallback(serverId => Subscribe(sink, new DuplexCallbackId(id, serverId), instance, dataType));

      return Activator.CreateInstance(typeof(DuplexCallbackObservable<>).MakeGenericType(dataType), id);
    }

    private static IDisposable Subscribe(IClientDuplexQbservableProtocolSink sink, DuplexCallbackId id, object instance, Type dataType)
    {
      Contract.Requires(sink != null);
      Contract.Requires(instance != null);
      Contract.Requires(dataType != null);
      Contract.Ensures(Contract.Result<IDisposable>() != null);

      return dataType.UpCast(instance).Subscribe(
        value => sink.SendOnNext(id, value),
        ex => sink.SendOnError(id, ExceptionDispatchInfo.Capture(ex)),
        () => sink.SendOnCompleted(id));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "protocol", Justification = "It's setting the field.")]
    public void SetClientProtocol(IQbservableProtocol protocol)
    {
      Contract.Requires(Protocol == null);
      Contract.Requires(protocol != null);

      this.protocol = protocol;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "protocol", Justification = "It's setting the field.")]
    public void SetServerProtocol(IQbservableProtocol protocol)
    {
      Contract.Requires(Protocol == null);
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