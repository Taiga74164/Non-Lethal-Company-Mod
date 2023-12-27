using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using NonLethalCompany_Mod.Modules;

namespace NonLethalCompany_Mod;

[BepInPlugin(
    PluginInfo.PLUGIN_GUID,
    PluginInfo.PLUGIN_NAME,
    PluginInfo.PLUGIN_VERSION
)]
public class NonLethalCompany : BaseUnityPlugin
{
    public static NonLethalCompany Instance { get; private set; } = null!;

    public ILogSource ModLogger => Logger;

    private void Awake()
    {
        Instance = this;

        ModuleManager.LoadAllModules();

        Logger.LogMessage("\u001b[31mMOD LOADED WOW!!!!!!!!!!!!!!\u001b[0m");
    }

    private void Start()
    {
        // Set all config values.

        #region QoL

        ModConfig.SkipOnlineMode = Config.Bind("Quality of Life", "Skip Online Mode", false, "Skips the selection screen between Online and LAN.");

        #endregion

        #region Player

        ModConfig.GodMode = Config.Bind("Player", "God Mode", false, "Makes your player character invincible.");
        ModConfig.MovementSpeed = Config.Bind("Player", "Movement Speed", 4.6f, "Affects how fast your player character moves.");
        ModConfig.GrabDistance = Config.Bind("Player", "Grab Distance", 5f, "Affects how far you can grab objects.");
        ModConfig.NoFall = Config.Bind("Player", "No Fall Damage", false, "Disables fall damage.");

        #endregion

        #region Game

        ModConfig.DeadPlayerAlert = Config.Bind("Game", "Dead Player Notifier", false, "Alerts you if someone dies.");
        ModConfig.ShowScrapList = Config.Bind("Game", "Show Scrap List", false, "Shows a list of all scrap.");
        ModConfig.ShowEnemyList = Config.Bind("Game", "Show Enemy List", false, "Shows a list with all enemies.");
        ModConfig.ShowPlayerList = Config.Bind("Game", "Show Player List", false, "Shows a list with all players.");

        #endregion

        #region Visual

        ModConfig.ShowAllEnemies = Config.Bind("Visual", "Reveal All Enemies", false, "Disables invisibility on enemies.");
        ModConfig.NoFog = Config.Bind("Visual", "No Fog", false, "Disables fog.");

        #endregion

        #region ESP

        ModConfig.UseEsp = Config.Bind("ESP", "Enable", false, "Enables ESP.");
        ModConfig.EspShowPlayers = Config.Bind("ESP", "Show Players", false, "Shows players.");
        ModConfig.EspShowEnemies = Config.Bind("ESP", "Show Enemies", false, "Shows enemies.");
        ModConfig.EspShowItems = Config.Bind("ESP", "Show Scrap", false, "Shows scrap.");
        ModConfig.EspDrawLines = Config.Bind("ESP", "Draw Lines", false, "Draws lines to players.");
        ModConfig.EspDrawNames = Config.Bind("ESP", "Draw Names", false, "Shows the names of the selected objects.");
        ModConfig.EspDrawDistance = Config.Bind("ESP", "Draw Distance", false, "Shows the distance between you and your selected objects.");

        #endregion
    }
}

public static class ModConfig
{
    #region QoL

    public static ConfigEntry<bool>? SkipOnlineMode;

    #endregion

    #region Player

    public static ConfigEntry<bool>? GodMode;
    public static ConfigEntry<float>? MovementSpeed;
    public static ConfigEntry<float>? GrabDistance;
    public static ConfigEntry<bool>? NoFall;

    #endregion

    #region Game

    public static ConfigEntry<bool>? DeadPlayerAlert;
    public static ConfigEntry<bool>? ShowScrapList;
    public static ConfigEntry<bool>? ShowEnemyList;
    public static ConfigEntry<bool>? ShowPlayerList;

    #endregion

    #region Visual

    public static ConfigEntry<bool>? ShowAllEnemies;
    public static ConfigEntry<bool>? NoFog;

    #endregion

    #region ESP

    public static ConfigEntry<bool>? UseEsp;
    public static ConfigEntry<bool>? EspShowPlayers;
    public static ConfigEntry<bool>? EspShowEnemies;
    public static ConfigEntry<bool>? EspShowItems;
    public static ConfigEntry<bool>? EspDrawLines;
    public static ConfigEntry<bool>? EspDrawNames;
    public static ConfigEntry<bool>? EspDrawDistance;

    #endregion
}
