using System;
using System.Linq.Expressions;

namespace ReactiveQ.Expressions
{
	[Serializable]
	internal sealed class SerializableDefaultExpression : SerializableExpression
	{
		public SerializableDefaultExpression(DefaultExpression expression, SerializableExpressionConverter converter)
			: base(expression)
		{
		}

		internal override Expression Convert()
		{
			return Expression.Default(Type);
		}
	}
}