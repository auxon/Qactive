using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Qactive.Expressions
{
  public sealed class SerializableExpressionConverter
  {
    private const List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>> noInitializers = null;
    private const SerializableExpression noExpression = null;
    private const IList<object> noRecursion = null;

    private readonly Dictionary<Expression, SerializableExpression> serialized = new Dictionary<Expression, SerializableExpression>();

    private BinaryExpression binary;
    private BlockExpression block;
    private ConditionalExpression conditional;
    private ConstantExpression constant;
    private DefaultExpression @default;
    private GotoExpression @goto;
    private IndexExpression index;
    private InvocationExpression invocation;
    private LabelExpression label;
    private LambdaExpression lambda;
    private ListInitExpression listInit;
    private LoopExpression loop;
    private MemberExpression member;
    private MemberInitExpression memberInit;
    private MethodCallExpression methodCall;
    private NewArrayExpression newArray;
    private NewExpression @new;
    private ParameterExpression parameter;
    private RuntimeVariablesExpression runtimeVariables;
    private SwitchExpression @switch;
    private TryExpression @try;
    private TypeBinaryExpression typeBinary;
    private UnaryExpression unary;

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(serialized != null);
    }

    public IList<TSerializableExpression> TryConvert<TSerializableExpression>(IEnumerable<Expression> expressions)
      where TSerializableExpression : SerializableExpression
    {
      Contract.Ensures(Contract.Result<IList<TSerializableExpression>>() != null);

      return expressions?.Select(TryConvert).Cast<TSerializableExpression>().ToList() ?? new List<TSerializableExpression>(0);
    }

    public IList<SerializableExpression> TryConvert(IEnumerable<Expression> expressions)
    {
      Contract.Ensures(Contract.Result<IList<SerializableExpression>>() != null);

      return expressions?.Select(TryConvert).ToList() ?? new List<SerializableExpression>(0);
    }

    public TSerializableExpression TryConvert<TSerializableExpression>(Expression expression)
      where TSerializableExpression : SerializableExpression
      => (TSerializableExpression)TryConvert(expression);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It's not too complex; it's a simple factory method for a fixed number of concrete types.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "It's a factory method, so it must know about all concrete implementations.")]
    public SerializableExpression TryConvert(Expression expression)
    {
      if (expression == null)
      {
        return null;
      }
      else if (serialized.ContainsKey(expression))
      {
        /* Caching is required to maintain object references during serialization.
         * See the comments on SerializableExpression.ConvertWithCache for more info.
         */
        return serialized[expression];
      }
      else if ((methodCall = expression as MethodCallExpression) != null)
      {
        return serialized[expression] = new SerializableMethodCallExpression(methodCall, this);
      }
      else if ((lambda = expression as LambdaExpression) != null)
      {
        return serialized[expression] = new SerializableLambdaExpression(lambda, this);
      }
      else if ((constant = expression as ConstantExpression) != null)
      {
        return serialized[expression] = new SerializableConstantExpression(constant);
      }
      else if ((member = expression as MemberExpression) != null)
      {
        return serialized[expression] = new SerializableMemberExpression(member, this);
      }
      else if ((binary = expression as BinaryExpression) != null)
      {
        return serialized[expression] = new SerializableBinaryExpression(binary, this);
      }
      else if ((block = expression as BlockExpression) != null)
      {
        return serialized[expression] = new SerializableBlockExpression(block, this);
      }
      else if ((conditional = expression as ConditionalExpression) != null)
      {
        return serialized[expression] = new SerializableConditionalExpression(conditional, this);
      }
      else if ((@default = expression as DefaultExpression) != null)
      {
        return serialized[expression] = new SerializableDefaultExpression(@default);
      }
      else if ((@goto = expression as GotoExpression) != null)
      {
        return serialized[expression] = new SerializableGotoExpression(@goto, this);
      }
      else if ((index = expression as IndexExpression) != null)
      {
        return serialized[expression] = new SerializableIndexExpression(index, this);
      }
      else if ((invocation = expression as InvocationExpression) != null)
      {
        return serialized[expression] = new SerializableInvocationExpression(invocation, this);
      }
      else if ((label = expression as LabelExpression) != null)
      {
        return serialized[expression] = new SerializableLabelExpression(label, this);
      }
      else if ((listInit = expression as ListInitExpression) != null)
      {
        return serialized[expression] = new SerializableListInitExpression(listInit, this);
      }
      else if ((loop = expression as LoopExpression) != null)
      {
        return serialized[expression] = new SerializableLoopExpression(loop, this);
      }
      else if ((memberInit = expression as MemberInitExpression) != null)
      {
        return serialized[expression] = new SerializableMemberInitExpression(memberInit, this);
      }
      else if ((newArray = expression as NewArrayExpression) != null)
      {
        return serialized[expression] = new SerializableNewArrayExpression(newArray, this);
      }
      else if ((@new = expression as NewExpression) != null)
      {
        return serialized[expression] = new SerializableNewExpression(@new, this);
      }
      else if ((parameter = expression as ParameterExpression) != null)
      {
        return serialized[expression] = new SerializableParameterExpression(parameter);
      }
      else if ((runtimeVariables = expression as RuntimeVariablesExpression) != null)
      {
        return serialized[expression] = new SerializableRuntimeVariablesExpression(runtimeVariables, this);
      }
      else if ((@switch = expression as SwitchExpression) != null)
      {
        return serialized[expression] = new SerializableSwitchExpression(@switch, this);
      }
      else if ((@try = expression as TryExpression) != null)
      {
        return serialized[expression] = new SerializableTryExpression(@try, this);
      }
      else if ((typeBinary = expression as TypeBinaryExpression) != null)
      {
        return serialized[expression] = new SerializableTypeBinaryExpression(typeBinary, this);
      }
      else if ((unary = expression as UnaryExpression) != null)
      {
        return serialized[expression] = new SerializableUnaryExpression(unary, this);
      }
      else
      {
        throw new ArgumentOutOfRangeException("expression");
      }
    }

    public static Expression TryConvert(SerializableExpression expression)
      => expression.TryConvertBack();

    // Workaround for a bug deserializing closed generic methods.
    // https://connect.microsoft.com/VisualStudio/feedback/details/736993/bound-generic-methodinfo-throws-argumentnullexception-on-deserialization
    public static Tuple<MethodInfo, Type[]> Convert(MethodInfo method)
      => method != null && method.IsGenericMethod && !method.IsGenericMethodDefinition
       ? Tuple.Create(method.GetGenericMethodDefinition(), method.GetGenericArguments())
       : Tuple.Create(method, (Type[])null);

    // Workaround for a bug deserializing closed generic methods.
    // https://connect.microsoft.com/VisualStudio/feedback/details/736993/bound-generic-methodinfo-throws-argumentnullexception-on-deserialization
    public static MethodInfo Convert(Tuple<MethodInfo, Type[]> method)
      => method.Item2 == null ? method.Item1 : method.Item1.MakeGenericMethod(method.Item2);

    public Tuple<MemberInfo, Type[]> Convert(MemberInfo source)
    {
      Contract.Ensures(Contract.Result<Tuple<MemberInfo, Type[]>>() != null);

      var method = source as MethodInfo;

      if (method != null)
      {
        var converted = Convert(method);

        return Tuple.Create((MemberInfo)converted.Item1, converted.Item2);
      }
      else
      {
        return Tuple.Create(source, (Type[])null);
      }
    }

    public static MemberInfo Convert(Tuple<MemberInfo, Type[]> member)
    {
      Contract.Requires(member != null);
      Contract.Ensures(Contract.Result<MemberInfo>() != null);

      var method = member.Item1 as MethodInfo;

      if (method != null)
      {
        return Convert(Tuple.Create(method, member.Item2));
      }
      else
      {
        return member.Item1;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Creating a custom type here would probably either require generics again or casting. It's fine as is.")]
    public IList<Tuple<MemberInfo, Type[]>> TryConvert(IEnumerable<MemberInfo> members)
    {
      Contract.Ensures(Contract.Result<IList<Tuple<MemberInfo, Type[]>>>() != null);

      return members?.Select(Convert).ToList() ?? new List<Tuple<MemberInfo, Type[]>>(0);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Creating a custom type here would probably either require generics again or casting. It's fine as is.")]
    public Tuple<Tuple<MemberInfo, Type[]>, MemberBindingType, SerializableExpression, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>, IList<object>> Convert(MemberBinding binding)
    {
      Contract.Requires(binding != null);
      Contract.Ensures(Contract.Result<Tuple<Tuple<MemberInfo, Type[]>, MemberBindingType, SerializableExpression, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>, IList<object>>>() != null);

      switch (binding.BindingType)
      {
        case MemberBindingType.Assignment:
          var assign = (MemberAssignment)binding;

          return Tuple.Create(
            Convert(binding.Member),
            binding.BindingType,
            TryConvert(assign.Expression),
            noInitializers,
            noRecursion);
        case MemberBindingType.ListBinding:
          var list = (MemberListBinding)binding;

          return Tuple.Create(
            Convert(binding.Member),
            binding.BindingType,
            noExpression,
            list.Initializers.Select(i => Tuple.Create(Convert(i.AddMethod), TryConvert(i.Arguments))).ToList(),
            noRecursion);
        case MemberBindingType.MemberBinding:
          var m = (MemberMemberBinding)binding;

          return Tuple.Create(
            Convert(binding.Member),
            binding.BindingType,
            noExpression,
            noInitializers,
            (IList<object>)m.Bindings.Select(Convert).ToList());
        default:
          throw new InvalidOperationException("Unknown member binding type.");
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Creating a custom type here would probably either require generics again or casting. It's fine as is.")]
    public static MemberBinding Convert(Tuple<Tuple<MemberInfo, Type[]>, MemberBindingType, SerializableExpression, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>, IList<object>> data)
    {
      Contract.Requires(data != null);
      Contract.Ensures(Contract.Result<MemberBinding>() != null);

      switch (data.Item2)
      {
        case MemberBindingType.Assignment:
          return Expression.Bind(Convert(data.Item1), data.Item3.TryConvertBack());
        case MemberBindingType.ListBinding:
          return Expression.ListBind(Convert(data.Item1), data.Item4.Select(i => Expression.ElementInit(Convert(i.Item1), i.Item2.TryConvert())));
        case MemberBindingType.MemberBinding:
          return Expression.MemberBind(Convert(data.Item1), data.Item5
            .Cast<Tuple<Tuple<MemberInfo, Type[]>, MemberBindingType, SerializableExpression, List<Tuple<Tuple<MethodInfo, Type[]>, IList<SerializableExpression>>>, IList<object>>>()
            .Select(Convert));
        default:
          throw new InvalidOperationException("Unknown member binding type.");
      }
    }
  }
}