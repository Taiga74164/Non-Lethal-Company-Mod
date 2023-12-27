using BepInEx.Logging;

namespace NonLethalCompany_Mod.Modules;

public abstract class Module
{
    public ILogSource Logger => NonLethalCompany.Instance.ModLogger;

    /// <summary>
    /// Called when the module is loaded.
    /// </summary>
    public abstract void Load();
}
