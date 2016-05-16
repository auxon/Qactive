using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qactive
{
  [ContractClass(typeof(IParameterizedQbservableProviderContract))]
  public interface IParameterizedQbservableProvider : IQbservableProvider
  {
    IQbservable<TResult> CreateQuery<TResult>(Expression expression, object argument);
  }

  [ContractClassFor(typeof(IParameterizedQbservableProvider))]
  internal abstract class IParameterizedQbservableProviderContract : IParameterizedQbservableProvider
  {
    public IQbservable<TResult> CreateQuery<TResult>(Expression expression, object argument)
    {
      Contract.Requires(expression != null);
      Contract.Ensures(Contract.Result<IQbservable<TResult>>() != null);
      return null;
    }

    public IQbservable<TResult> CreateQuery<TResult>(Expression expression) => null;
  }
}