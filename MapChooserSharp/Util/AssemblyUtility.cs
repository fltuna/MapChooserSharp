namespace MapChooserSharp.Util;

public class AssemblyUtility
{
    public static bool IsAssemblyLoaded(string assemblyName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.GetName().Name?.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) == true);
    }
}