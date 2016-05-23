using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Security.Permissions;
using Qactive.Properties;

namespace Qactive
{
  static partial class Log
  {
    [Conditional("TRACE"), SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
    public static void ClientSendingExpression(object clientId, Expression expression)
    {
      Contract.Requires(expression != null);

      if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Information))
      {
        QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ClientSendingExpression, TraceEventType.Information, clientId, LogMessages.ClientSendingExpression, GetDebugView(expression));
      }

#if DEBUG
      DebugPrint(GetDebugView(expression), "Client sending expression");
#endif
    }

    [Conditional("TRACE"), SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
    public static void ClientRewrittenExpression(object clientId, Expression expression)
    {
      Contract.Requires(expression != null);

      if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Verbose))
      {
        QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ClientRewrittenExpression, TraceEventType.Verbose, clientId, LogMessages.ClientRewrittenExpression, GetDebugView(expression));
      }

#if DEBUG
      DebugPrint(GetDebugView(expression), "Client rewritten expression");
#endif
    }

    [Conditional("TRACE"), SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
    public static void ServerReceivingExpression(object clientId, Expression expression)
    {
      Contract.Requires(expression != null);

      if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Information))
      {
        QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ServerReceivingExpression, TraceEventType.Information, clientId, LogMessages.ServerReceivingExpression, GetDebugView(expression));
      }

#if DEBUG
      DebugPrint(GetDebugView(expression), "Server receiving expression");
#endif
    }

    [Conditional("TRACE"), SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
    public static void ServerRewrittenExpression(object clientId, Expression expression)
    {
      Contract.Requires(expression != null);

      if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Verbose))
      {
        QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ServerRewrittenExpression, TraceEventType.Verbose, clientId, LogMessages.ServerRewrittenExpression, GetDebugView(expression));
      }

#if DEBUG
      DebugPrint(GetDebugView(expression), "Server rewritten expression");
#endif
    }
  }
}
