namespace System.Reflection
{
  [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Duplicating the real BindingFlags enum defined in FCL.")]
  [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Duplicating the real BindingFlags enum defined in FCL.")]
  [Flags]
  public enum BindingFlags
  {
    Default = 0,
    Instance = 4,
    Static = 8,
    Public = 16,
    [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "NonPublic", Justification = "Duplicating the real BindingFlags enum defined in FCL.")]
    NonPublic = 32,
    //FlattenHierarchy = 64,
    //ExactBinding = 65536
  }
}
