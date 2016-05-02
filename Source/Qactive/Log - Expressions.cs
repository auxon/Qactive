using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive
{
  static partial class Log
  {
    [Conditional("TRACE")]
    public static void ClientSendingExpression(object clientId, Expression expression)
    {
      Contract.Requires(expression != null);

      if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Information))
      {
        QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ClientSendingExpression, TraceEventType.Information, clientId, "Client sending expression", GetDebugView(expression));
      }

#if DEBUG
      DebugPrint(GetDebugView(expression), "Client sending expression");
#endif
    }

    [Conditional("TRACE")]
    public static void ClientRewrittenExpression(object clientId, Expression expression)
    {
      Contract.Requires(expression != null);

      if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Verbose))
      {
        QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ClientRewrittenExpression, TraceEventType.Verbose, clientId, "Client rewritten expression", GetDebugView(expression));
      }

#if DEBUG
      DebugPrint(GetDebugView(expression), "Client rewritten expression");
#endif
    }

    [Conditional("TRACE")]
    public static void ServerReceivingExpression(object clientId, Expression expression)
    {
      Contract.Requires(expression != null);

      if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Information))
      {
        QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ServerReceivingExpression, TraceEventType.Information, clientId, "Server receiving expression", GetDebugView(expression));
      }

#if DEBUG
      DebugPrint(GetDebugView(expression), "Server receiving expression");
#endif
    }

    [Conditional("TRACE")]
    public static void ServerRewrittenExpression(object clientId, Expression expression)
    {
      Contract.Requires(expression != null);

      if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Verbose))
      {
        QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ServerRewrittenExpression, TraceEventType.Verbose, clientId, "Server rewritten expression", GetDebugView(expression));
      }

#if DEBUG
      DebugPrint(GetDebugView(expression), "Server rewritten expression");
#endif
    }
  }
}
