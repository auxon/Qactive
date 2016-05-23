using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyConfiguration(AssemblyConstants.Configuration)]

[assembly: AssemblyCompany("Dave Sexton")]
[assembly: AssemblyProduct("Qactive")]
[assembly: AssemblyCopyright("Copyright © 2012-2016 Dave Sexton")]
[assembly: AssemblyTrademark("")]

[assembly: AssemblyVersion(AssemblyConstants.Version + ".0")]
[assembly: AssemblyFileVersion(AssemblyConstants.Version + ".0")]

[assembly: SuppressMessage("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly", Justification = "Required for NuGet semantic versioning.")]
[assembly: AssemblyInformationalVersion(AssemblyConstants.Version + AssemblyConstants.PrereleaseVersion)]

[SuppressMessage("Microsoft.Design", "CA1050:DeclareTypesInNamespaces", Justification = "Referenced by assembly-level attributes only.")]
internal static class AssemblyConstants
{
  public const string Version = "1.1.0";

  /// <summary>
  /// Semantic version for the assembly, indicating a prerelease package in NuGet.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The specified name can be arbitrary, but its mere presence indicates a prerelease package.
  /// To indicate a release package instead, use an empty string.
  /// </para>
  /// <para>
  /// If specified, the value must include a preceding hyphen; e.g., "-alpha", "-beta", "-rc".
  /// </para>
  /// </remarks>
  /// <seealso href="http://docs.nuget.org/docs/reference/versioning#Really_brief_introduction_to_SemVer">
  /// NuGet Semantic Versioning
  /// </seealso>
  public const string PrereleaseVersion = "";

#if DEBUG
  public const string Configuration = "Debug";
#else
  public const string Configuration = "Release";
#endif
}