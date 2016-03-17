namespace Qactive
{
  public static class Either
  {
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft leftValue)
    {
      return new Either<TLeft, TRight>(leftValue, default(TRight), isLeft: true);
    }

    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight rightValue)
    {
      return new Either<TLeft, TRight>(default(TLeft), rightValue, isLeft: false);
    }
  }
}
