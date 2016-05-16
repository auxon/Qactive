using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive.Expressions
{
  [Serializable]
  [ContractClass(typeof(SerializableExpressionContract))]
  public abstract class SerializableExpression
  {
    public ExpressionType NodeType { get; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Mirrors the Expression.Type property.")]
    public Type Type { get; }

    [NonSerialized]
    private Expression converted;

    protected SerializableExpression(Expression expression)
    {
      Contract.Requires(expression != null);

      NodeType = expression.NodeType;
      Type = expression.Type;
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(Type != null);
    }

    /* Caching is required to ensure that expressions referring to the same objects actually refer to the same
     * instances in memory.  For example, when a lambda expression's Body uses parameters, they must reference 
     * the actual expression objects that are defined in the lambda expression's Parameters collection; otherwise, 
     * compiling the lambda throws an exception with a message similar to the following: 
     * 
     * Expression variable 'p' of type 'System.Int32' referenced from scope '', but it is not defined
     */
    internal Expression ConvertWithCache()
    {
      Contract.Ensures(Contract.Result<Expression>() != null);

      return converted ?? (converted = Convert());
    }

    internal abstract Expression Convert();
  }

  [ContractClassFor(typeof(SerializableExpression))]
  internal abstract class SerializableExpressionContract : SerializableExpression
  {
    public SerializableExpressionContract(Expression expression)
      : base(expression)
    {
    }

    internal override Expression Convert()
    {
      Contract.Ensures(Contract.Result<Expression>() != null);
      return null;
    }
  }
}