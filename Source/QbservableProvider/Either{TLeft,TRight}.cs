namespace QbservableProvider
{
  public sealed class Either<TLeft, TRight>
  {
    internal Either(TLeft left, TRight right, bool isLeft)
    {
      Left = left;
      Right = right;
      IsLeft = isLeft;
    }

    public TLeft Left { get; }

    public TRight Right { get; }

    public bool IsLeft { get; }
  }
}
