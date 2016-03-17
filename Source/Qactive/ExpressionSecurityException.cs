using System;
using System.Runtime.Serialization;
using System.Security;

namespace Qactive
{
  [Serializable]
  internal sealed class ExpressionSecurityException : SecurityException
  {
    public ExpressionSecurityException()
    {
    }

    public ExpressionSecurityException(string message)
      : base(message)
    {
    }

    private ExpressionSecurityException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}