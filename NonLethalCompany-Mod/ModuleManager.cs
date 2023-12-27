using System.Reflection;

namespace NonLethalCompany_Mod.Modules;

public static class ModuleManager
{
    /// <summary>
    /// Loads all modules in the 'NonLethalCompany_Mod' namespace.
    /// </summary>
    public static void LoadAllModules()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            if (type.Namespace != "NonLethalCompany_Mod.Modules") continue;
            if (type.BaseType != typeof(Module)) continue;

            var method = type.GetMethod("Load");
            method?.Invoke(null, null);
        }
    }
}
