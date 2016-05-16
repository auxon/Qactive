using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Qactive.Properties;

namespace Qactive
{
  public class LocalEvaluationContext : KnownTypeContext
  {
    private static readonly IEnumerable<Assembly> defaultKnownAssemblies = new List<Assembly>()
      {
        typeof(LocalEvaluationContext).Assembly,
        typeof(int).Assembly,
        typeof(System.Uri).Assembly,
        typeof(System.Data.DataSet).Assembly,
        typeof(System.Xml.XmlReader).Assembly,
        typeof(System.Xml.Linq.XElement).Assembly,
        typeof(System.Linq.Enumerable).Assembly,
        typeof(System.Reactive.Linq.Observable).Assembly,
        typeof(System.Reactive.Linq.Qbservable).Assembly,
        typeof(System.Reactive.Notification).Assembly,
        typeof(System.Reactive.IEventPattern<,>).Assembly,
        typeof(System.Reactive.Concurrency.TaskPoolScheduler).Assembly,
      }
      .AsReadOnly();

    internal IDictionary<string, ParameterExpression> ReplacedParameters { get; } = new Dictionary<string, ParameterExpression>();

    public LocalEvaluationContext(params Type[] knownTypes)
      : base(defaultKnownAssemblies, knownTypes)
    {
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(ReplacedParameters != null);
    }

    internal bool EnsureKnownTypes(IEnumerable<MemberInfo> members)
    {
      var processedAny = false;

      if (members != null)
      {
        foreach (var type in members.Select(member => member.DeclaringType).Distinct())
        {
          processedAny |= EnsureKnownType(type);
        }
      }

      return processedAny;
    }

    // return value indicates whether the type has been processed, not whether it's serializable.
    internal bool EnsureKnownType(MemberInfo member, Action<Type> replaceCompilerGeneratedType = null, Action<Type, Type> unknownType = null, Action<Type> genericArgumentsUpdated = null)
      => member != null && EnsureKnownType(member.DeclaringType, replaceCompilerGeneratedType, unknownType, genericArgumentsUpdated);

    // return value indicates whether the type has been processed, not whether it's serializable.
    internal bool EnsureKnownType(MethodInfo method, Action<Type> replaceCompilerGeneratedType = null, Action<Type, Type> unknownType = null, Action<MethodInfo> genericMethodArgumentsUpdated = null)
      => method != null
      && (EnsureKnownType(method.DeclaringType, replaceCompilerGeneratedType, unknownType)
        || EnsureGenericTypeArgumentsSerializable(method, genericMethodArgumentsUpdated));

    // return value indicates whether the type has been processed, not whether it's serializable.
    internal bool EnsureKnownType(LabelTarget target, Action<Type> replaceCompilerGeneratedType = null, Action<Type, Type> unknownType = null, Action<Type> genericArgumentsUpdated = null)
      => target != null && EnsureKnownType(target.Type, replaceCompilerGeneratedType, unknownType, genericArgumentsUpdated);

    // return value indicates whether the type has been processed, not whether it's serializable.
    internal bool EnsureKnownType(Type type, Action<Type> replaceCompilerGeneratedType = null, Action<Type, Type> unknownType = null, Action<Type> genericArgumentsUpdated = null)
      => type != null
      && (EnsureCompilerGeneratedTypeIsReplaced(type, replaceCompilerGeneratedType)
        || EnsureKnownTypeHierarchy(type, unknownType)
        || EnsureGenericTypeArgumentsSerializable(type, genericArgumentsUpdated));

    private static bool EnsureCompilerGeneratedTypeIsReplaced(Type type, Action<Type> replaceCompilerGeneratedType = null)
    {
      Contract.Requires(type != null);

      if (type == typeof(CompilerGenerated))
      {
        throw new InvalidOperationException(Errors.ExpressionVisitedCompilerTypeTwice);
      }

      if (type.GetCustomAttribute<CompilerGeneratedAttribute>(inherit: true) != null)
      {
        if (replaceCompilerGeneratedType != null)
        {
          replaceCompilerGeneratedType(type);
        }
        else
        {
          throw new InvalidOperationException(Errors.ExpressionUnsupportedCompilerType);
        }

        return true;
      }

      return false;
    }

    private bool EnsureKnownTypeHierarchy(Type type, Action<Type, Type> unknownType = null)
    {
      Contract.Requires(type != null);

      var current = type;

      do
      {
        if (!IsKnownType(current))
        {
          if (unknownType != null)
          {
            unknownType(current, type);

            return true;
          }
          else if (current == type)
          {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ExpressionUnknownType, type.FullName));
          }
          else
          {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Errors.ExpressionUnknownBaseType, type.FullName, current.FullName));
          }
        }
      }
      while ((type = type.DeclaringType) != null);

      return false;
    }

    private bool EnsureGenericTypeArgumentsSerializable(Type type, Action<Type> genericArgumentsUpdated = null)
    {
      Contract.Requires(type != null);

      if (type.IsGenericType)
      {
        if (EnsureGenericTypeArgumentsSerializable(ref type))
        {
          if (genericArgumentsUpdated != null)
          {
            genericArgumentsUpdated(type);
          }
          else
          {
            throw new InvalidOperationException(Errors.ExpressionUnsupportedCompilerTypeAsTypeArg);
          }

          return true;
        }
      }

      return false;
    }

    private bool EnsureGenericTypeArgumentsSerializable(MethodInfo method, Action<MethodInfo> genericArgumentsUpdated = null)
    {
      Contract.Requires(method != null);

      if (method.IsGenericMethod)
      {
        if (EnsureGenericTypeArgumentsSerializable(ref method))
        {
          if (genericArgumentsUpdated != null)
          {
            genericArgumentsUpdated(method);
          }
          else
          {
            throw new InvalidOperationException(Errors.ExpressionUnsupportedCompilerTypeAsMethodTypeArg);
          }

          return true;
        }
      }

      return false;
    }

    private bool EnsureGenericTypeArgumentsSerializable(ref Type type)
    {
      Contract.Requires(type != null);

      var genericTypeArguments = type.GetGenericArguments();

      Type oldType = type;
      Type newType = null;

      EnsureGenericTypeArgumentsSerializable(
        genericTypeArguments,
        () => newType = oldType.GetGenericTypeDefinition().MakeGenericType(genericTypeArguments));

      if (newType != null)
      {
        type = newType;

        return true;
      }

      return false;
    }

    private bool EnsureGenericTypeArgumentsSerializable(ref MethodInfo method)
    {
      Contract.Requires(method != null);

      var genericTypeArguments = method.GetGenericArguments();

      MethodInfo oldMethodInfo = method;
      MethodInfo newMethodInfo = null;

      EnsureGenericTypeArgumentsSerializable(
        genericTypeArguments,
        () => newMethodInfo = oldMethodInfo.GetGenericMethodDefinition().MakeGenericMethod(genericTypeArguments));

      if (newMethodInfo != null)
      {
        method = newMethodInfo;

        return true;
      }

      return false;
    }

    private void EnsureGenericTypeArgumentsSerializable(IList<Type> genericTypeArguments, Action updateType)
    {
      Contract.Requires(genericTypeArguments != null);
      Contract.Requires(updateType != null);

      var replacedAny = false;

      for (int i = 0; i < genericTypeArguments.Count; i++)
      {
        var argument = genericTypeArguments[i];

        var replaced = EnsureKnownType(
          argument,
          replaceCompilerGeneratedType: _ =>
          {
            genericTypeArguments[i] = typeof(CompilerGenerated);
            replacedAny = true;
          });

        if (!replaced && argument.IsGenericType && EnsureGenericTypeArgumentsSerializable(ref argument))
        {
          genericTypeArguments[i] = argument;
          replacedAny = true;
        }
      }

      if (replacedAny)
      {
        updateType();
      }
    }
  }
}