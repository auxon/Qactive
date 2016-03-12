using System.Linq.Expressions;
using System.Reactive.Linq;

namespace ReactiveQ
{
	public interface IParameterizedQbservableProvider : IQbservableProvider
	{
		IQbservable<TResult> CreateQuery<TResult>(Expression expression, object argument);
	}
}