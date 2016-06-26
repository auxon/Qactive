using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Qactive
{
#if SERIALIZATION
  [Serializable]
#endif
  public sealed class QbservableServiceOptions
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It's immutable due to the frozen flag.")]
    public static readonly QbservableServiceOptions Default = new QbservableServiceOptions()
    {
      IsFrozen = true,
      evaluationContext = new ServiceEvaluationContext()
    };

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It's immutable due to the frozen flag.")]
    public static readonly QbservableServiceOptions Unrestricted = new QbservableServiceOptions()
    {
      IsFrozen = true,
      allowExpressionsUnrestricted = true,
      enableDuplex = true,
      evaluationContext = new ServiceEvaluationContext()
    };

    private readonly List<ExpressionVisitor> visitors = new List<ExpressionVisitor>();
    private bool sendServerErrorsToClients;
    private bool enableDuplex;
    private bool allowExpressionsUnrestricted;
    private ExpressionOptions expressionOptions;
    private ServiceEvaluationContext evaluationContext;

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(visitors != null);
    }

    public bool IsFrozen { get; private set; }

    public bool SendServerErrorsToClients
    {
      get
      {
        return sendServerErrorsToClients;
      }
      set
      {
        Contract.Requires(!IsFrozen);

        sendServerErrorsToClients = value;
      }
    }

    public bool EnableDuplex
    {
      get
      {
        return enableDuplex;
      }
      set
      {
        Contract.Requires(!IsFrozen);

        enableDuplex = value;
      }
    }

    public bool AllowExpressionsUnrestricted
    {
      get
      {
        return allowExpressionsUnrestricted;
      }
      set
      {
        Contract.Requires(!IsFrozen);

        allowExpressionsUnrestricted = value;
      }
    }

#if READONLYCOLLECTIONS
    public IReadOnlyList<ExpressionVisitor> Visitors => visitors.AsReadOnly();
#else
    public ReadOnlyCollection<ExpressionVisitor> Visitors => visitors.AsReadOnly();
#endif

    public ExpressionOptions ExpressionOptions
    {
      get
      {
        return expressionOptions;
      }
      set
      {
        Contract.Requires(!IsFrozen);

        expressionOptions = value;
      }
    }

    public ServiceEvaluationContext EvaluationContext
    {
      get
      {
        Contract.Ensures(Contract.Result<ServiceEvaluationContext>() != null);

        if (evaluationContext == null)
        {
          evaluationContext = new ServiceEvaluationContext();
        }

        return evaluationContext;
      }
      set
      {
        Contract.Requires(!IsFrozen);

        evaluationContext = value;
      }
    }

    public QbservableServiceOptions()
    {
      Contract.Ensures(!IsFrozen);
    }

    public QbservableServiceOptions(QbservableServiceOptions clone)
    {
      Contract.Requires(clone != null);
      Contract.Ensures(!IsFrozen);

      sendServerErrorsToClients = clone.sendServerErrorsToClients;
      enableDuplex = clone.enableDuplex;
      allowExpressionsUnrestricted = clone.allowExpressionsUnrestricted;
      expressionOptions = clone.expressionOptions;
      evaluationContext = clone.evaluationContext;
    }

    public QbservableServiceOptions Add(ExpressionVisitor visitor)
    {
      Contract.Requires(visitor != null);

      var options = IsFrozen ? Clone() : this;

      options.visitors.Add(visitor);

      return options;
    }

    public QbservableServiceOptions Freeze()
    {
      Contract.Ensures(Contract.Result<QbservableServiceOptions>() == this);
      Contract.Ensures(IsFrozen);

      IsFrozen = true;
      return this;
    }

    public QbservableServiceOptions Clone()
    {
      Contract.Ensures(Contract.Result<QbservableServiceOptions>() != null);
      Contract.Ensures(!Contract.Result<QbservableServiceOptions>().IsFrozen);

      return new QbservableServiceOptions(this);
    }
  }
}