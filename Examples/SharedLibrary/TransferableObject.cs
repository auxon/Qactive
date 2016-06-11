using System;

namespace SharedLibrary
{
  [Serializable]
  public sealed class TransferableObject
  {
    public int Value
    {
      get
      {
        Console.Write("TransferableObject.");
        ConsoleTrace.PrintCurrentMethod();

        return value;
      }
    }

    private readonly int value;

    public TransferableObject(int value)
    {
      Console.Write("TransferableObject");
      ConsoleTrace.PrintCurrentMethod();

      this.value = value;
    }
  }
}