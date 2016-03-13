namespace Qactive
{
  public static class Either
  {
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft left)
    {
      return new Either<TLeft, TRight>(left, default(TRight), isLeft: true);
    }

    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight right)
    {
      return new Either<TLeft, TRight>(default(TLeft), right, isLeft: false);
    }
  }
}
