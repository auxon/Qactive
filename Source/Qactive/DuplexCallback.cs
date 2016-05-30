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
  internal class DuplexCallback : IInvokeDuplexCallback
  {
    public bool IsInitialized => sink != null && protocol != null;

    /// <summary>
    /// Get the name of the member or variable that this callback represents, for diagnostic purposes only.
    /// </summary>
    public string Name { get; }

    public IQbservableProtocol Protocol => protocol;

    protected IServerDuplexQbservableProtocolSink Sink => sink;

    protected int Id { get; }

    protected object ClientId { get; }

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

#if SERIALIZATION
    [NonSerialized]
#endif
    private readonly Func<int, object[], object> invoke;

    protected DuplexCallback(string name, int id, object clientId)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(clientId != null);

      Name = name;
      Id = id;
      ClientId = clientId;
    }

    private DuplexCallback(string name, IQbservableProtocol protocol, Func<int, object[], object> invoke)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(protocol != null);
      Contract.Requires(invoke != null);

      Name = name;
      this.invoke = invoke;
      ClientId = protocol.ClientId;
      Id = protocol.GetOrAddSink(protocol.CreateClientDuplexSink)
                   .RegisterInvokeCallback(this);
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(!string.IsNullOrEmpty(Name));
      Contract.Invariant(ClientId != null);
    }

    public static Expression Create(string name, IQbservableProtocol protocol, object instance, PropertyInfo property)
    {
      Contract.Requires(protocol != null);
      Contract.Requires(property != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return CreateInvoke(
        new DuplexCallback(name, protocol, (_, __) => ConvertIfSequence(name, protocol, property.GetValue(instance))),
        property.PropertyType);
    }

    public static Expression Create(string name, IQbservableProtocol protocol, object instance, FieldInfo field)
    {
      Contract.Requires(protocol != null);
      Contract.Requires(field != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return CreateInvoke(
        new DuplexCallback(name, protocol, (_, __) => ConvertIfSequence(name, protocol, field.GetValue(instance))),
        field.FieldType);
    }

    public static Expression Create(string name, IQbservableProtocol protocol, object instance, MethodInfo method, IEnumerable<Expression> argExpressions)
    {
      Contract.Requires(protocol != null);
      Contract.Requires(method != null);
      Contract.Requires(argExpressions != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return CreateInvoke(
        new DuplexCallback(name, protocol, (_, arguments) => ConvertIfSequence(name, protocol, method.Invoke(instance, arguments))),
        method.ReturnType,
        argExpressions);
    }

    public static Expression CreateEnumerable(string name, IQbservableProtocol protocol, object instance, Type dataType, Type type)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(protocol != null);
      Contract.Requires(dataType != null);
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return Expression.Constant(
        CreateRemoteEnumerable(name, protocol, (IEnumerable)instance, dataType),
        type);
    }

    public static Expression CreateObservable(string name, IQbservableProtocol protocol, object instance, Type dataType, Type type)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(protocol != null);
      Contract.Requires(dataType != null);
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<Expression>() != null);

      return Expression.Constant(
        CreateRemoteObservable(name, protocol, instance, dataType),
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

    private static object ConvertIfSequence(string name, IQbservableProtocol protocol, object instance)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
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
            return CreateRemoteObservable(name, protocol, instance, observableType.GetGenericArguments()[0]);
          }

          var enumerableType = type.GetGenericInterfaceFromDefinition(typeof(IEnumerable<>));
          var enumerable = instance as IEnumerable;

          if (enumerableType != null)
          {
            return CreateRemoteEnumerable(name, protocol, enumerable, enumerableType.GetGenericArguments()[0]);
          }
          else if (enumerable != null)
          {
            return CreateRemoteEnumerable(name, protocol, enumerable.Cast<object>(), typeof(object));
          }
        }
      }

      return instance;
    }

    private static object CreateRemoteEnumerable(string name, IQbservableProtocol protocol, IEnumerable instance, Type dataType)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(protocol != null);
      Contract.Requires(instance != null);
      Contract.Requires(dataType != null);
      Contract.Ensures(Contract.Result<object>() != null);

      var sink = protocol.GetOrAddSink(protocol.CreateClientDuplexSink);

      return sink.RegisterEnumerableCallback(clientId => (IEnumerableDuplexCallback)Activator.CreateInstance(typeof(DuplexCallbackEnumerable<>).MakeGenericType(dataType), name, clientId, protocol.ClientId, instance));
    }

    private static object CreateRemoteObservable(string name, IQbservableProtocol protocol, object instance, Type dataType)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(protocol != null);
      Contract.Requires(instance != null);
      Contract.Requires(dataType != null);
      Contract.Ensures(Contract.Result<object>() != null);

      var sink = protocol.GetOrAddSink(protocol.CreateClientDuplexSink);

      return sink.RegisterObservableCallback(clientId => (IObservableDuplexCallback)Activator.CreateInstance(typeof(DuplexCallbackObservable<>).MakeGenericType(dataType), name, clientId, protocol.ClientId, instance));
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

    object IInvokeDuplexCallback.Invoke(object[] arguments)
      => invoke(Id, arguments);

    public TResult ServerInvoke<TResult>(object[] arguments)
    {
      Contract.Requires(IsInitialized);

      try
      {
        var value = (TResult)sink.Invoke(Name, Id, arguments);

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
      Contract.Requires(IsInitialized);

      try
      {
        sink.Invoke(Name, Id, arguments);
      }
      catch (Exception ex)
      {
        protocol.CancelAllCommunication(ExceptionDispatchInfo.Capture(ex));
      }
    }

    public override string ToString()
      => Name + " <- " + ClientId + "," + Id;
  }
}
