using System;
using System.Reflection;

namespace Qactive.Tests
{
  internal static partial class Any
  {
    public const string Message = "Any.Message";
    public const string Name = "Any.Name";
    public static readonly Type Type = typeof(Any);
    public static readonly MemberInfo Member = typeof(Any).GetField("Member");
  }
}
