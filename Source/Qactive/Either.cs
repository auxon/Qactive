using System.Diagnostics.Contracts;
namespace Qactive
{
  public static class Either
  {
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft leftValue)
    {
      Contract.Ensures(Contract.Result<Either<TLeft, TRight>>() != null);
      Contract.Ensures(Contract.Result<Either<TLeft, TRight>>().IsLeft);

      return new Either<TLeft, TRight>(leftValue, default(TRight), isLeft: true);
    }

    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight rightValue)
    {
      Contract.Ensures(Contract.Result<Either<TLeft, TRight>>() != null);
      Contract.Ensures(Contract.Result<Either<TLeft, TRight>>().IsLeft == false);

      return new Either<TLeft, TRight>(default(TLeft), rightValue, isLeft: false);
    }
  }
}
