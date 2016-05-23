using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Security;

namespace Qactive
{
#if SERIALIZATION
  [Serializable]
#endif
  internal sealed class ExpressionSecurityException : SecurityException
  {
    public ExpressionSecurityException()
    {
    }

    public ExpressionSecurityException(string message)
      : base(message)
    {
    }

#if SERIALIZATION
    private ExpressionSecurityException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Contract.Requires(info != null);
    }
#endif
  }
}