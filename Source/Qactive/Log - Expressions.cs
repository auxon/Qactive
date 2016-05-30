using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
#if TRACING
using System.Security;
using System.Security.Permissions;
using Qactive.Properties;
#endif

namespace Qactive
{
  static partial class Log
  {
    [Conditional("TRACE")]
    public static void ClientSendingExpression(object sourceId, Expression expression)
    {
      Contract.Requires(expression != null);

#if TRACING
      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
        if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Information))
        {
          QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ClientSendingExpression, TraceEventType.Information, sourceId, LogMessages.ClientSendingExpression, GetDebugView(expression));
        }

#if DEBUG
        DebugPrint(GetDebugView(expression), "Client sending expression");
#endif
      }
      finally
      {
        PermissionSet.RevertAssert();
      }
#endif
    }

    [Conditional("TRACE")]
    public static void ClientRewrittenExpression(object sourceId, Expression expression)
    {
      Contract.Requires(expression != null);

#if TRACING
      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
        if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Verbose))
        {
          QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ClientRewrittenExpression, TraceEventType.Verbose, sourceId, LogMessages.ClientRewrittenExpression, GetDebugView(expression));
        }

#if DEBUG
        DebugPrint(GetDebugView(expression), "Client rewritten expression");
#endif
      }
      finally
      {
        PermissionSet.RevertAssert();
      }
#endif
    }

    [Conditional("TRACE")]
    public static void ServerReceivingExpression(object sourceId, Expression expression)
    {
      Contract.Requires(expression != null);

#if TRACING
      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
        if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Information))
        {
          QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ServerReceivingExpression, TraceEventType.Information, sourceId, LogMessages.ServerReceivingExpression, GetDebugView(expression));
        }

#if DEBUG
        DebugPrint(GetDebugView(expression), "Server receiving expression");
#endif
      }
      finally
      {
        PermissionSet.RevertAssert();
      }
#endif
    }

    [Conditional("TRACE")]
    public static void ServerRewrittenExpression(object sourceId, Expression expression)
    {
      Contract.Requires(expression != null);

#if TRACING
      new PermissionSet(PermissionState.Unrestricted).Assert();

      try
      {
        if (QactiveTraceSources.QactiveExpressions.Switch.ShouldTrace(TraceEventType.Verbose))
        {
          QactiveTraceSources.QactiveExpressions.SemanticObject(SemanticTrace.ServerRewrittenExpression, TraceEventType.Verbose, sourceId, LogMessages.ServerRewrittenExpression, GetDebugView(expression));
        }

#if DEBUG
        DebugPrint(GetDebugView(expression), "Server rewritten expression");
#endif
      }
      finally
      {
        PermissionSet.RevertAssert();
      }
#endif
    }
  }
}
