using System.Diagnostics;
using System.Linq.Expressions;

namespace Qactive
{
  partial class Log
  {
    [Conditional("_")]
    internal static void ClientSendingExpression(object clientId, Expression expression)
    {
    }

    [Conditional("_")]
    internal static void ClientRewrittenExpression(object clientId, Expression expression)
    {
    }

    [Conditional("_")]
    internal static void ServerReceivingExpression(object clientId, Expression expression)
    {
    }

    [Conditional("_")]
    internal static void ServerRewrittenExpression(object clientId, Expression expression)
    {
    }
  }
}
