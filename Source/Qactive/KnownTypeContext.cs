using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Qactive
{
#if SERIALIZATION
  [Serializable]
#endif
  public class KnownTypeContext
  {
    private readonly HashSet<Assembly> knownAssemblies;
    private readonly HashSet<Type> knownTypes;

    public KnownTypeContext(params Type[] knownTypes)
      : this((IEnumerable<Type>)knownTypes)
    {
      Contract.Requires(knownTypes == null || Contract.ForAll(knownTypes, type => !type.GetIsGenericType() || type.GetIsGenericTypeDefinition()));
    }

    public KnownTypeContext(IEnumerable<Type> knownTypes)
      : this(null, knownTypes)
    {
      Contract.Requires(knownTypes == null || Contract.ForAll(knownTypes, type => !type.GetIsGenericType() || type.GetIsGenericTypeDefinition()));
    }

    public KnownTypeContext(IEnumerable<Assembly> knownAssemblies, params Type[] additionalKnownTypes)
      : this(knownAssemblies, (IEnumerable<Type>)additionalKnownTypes)
    {
      Contract.Requires(additionalKnownTypes == null || Contract.ForAll(additionalKnownTypes, type => !type.GetIsGenericType() || type.GetIsGenericTypeDefinition()));
    }

    public KnownTypeContext(IEnumerable<Assembly> knownAssemblies, IEnumerable<Type> additionalKnownTypes)
    {
      Contract.Requires(additionalKnownTypes == null || Contract.ForAll(additionalKnownTypes, type => !type.GetIsGenericType() || type.GetIsGenericTypeDefinition()));

      this.knownAssemblies = new HashSet<Assembly>(knownAssemblies ?? Enumerable.Empty<Assembly>());
      this.knownTypes = new HashSet<Type>(additionalKnownTypes ?? Enumerable.Empty<Type>());
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(knownAssemblies != null);
      Contract.Invariant(knownTypes != null);
    }

    public void AddKnownType(Type type)
    {
      Contract.Requires(type != null);

      knownTypes.Add(type);
    }

    public bool IsTypeInKnownAssembly(Type type) => type != null && knownAssemblies.Contains(type.GetAssembly());

    public bool IsTypeKnown(object value) => value == null || IsKnownType(value.GetType());

    public virtual bool IsKnownType(Type type) => type == null
                                               || type.GetIsPrimitive()
                                               || type.IsArray && IsKnownType(type.GetElementType())
                                               || IsTypeInKnownAssembly(type)
                                               || knownTypes.Contains(type.GetIsGenericType() ? type.GetGenericTypeDefinition() : type);
  }
}