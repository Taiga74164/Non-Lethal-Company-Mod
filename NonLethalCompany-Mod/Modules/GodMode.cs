using GameNetcodeStuff;
using HarmonyLib;

namespace NonLethalCompany_Mod.Modules;

public class GodMode : Module
{
    public override void Load()
    {
        Harmony.CreateAndPatchAll(typeof(PlayerDamagePatch));
    }
}

[HarmonyPatch(typeof(PlayerControllerB))]
public class PlayerDamagePatch
{
    [HarmonyPatch("AllowPlayerDeath")]
    [HarmonyPrefix]
    private static bool AllowPlayerDeathPrefix(PlayerControllerB instance)
    {
        return ModConfig.GodMode != null && !ModConfig.GodMode.Value;
    }

    [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
    [HarmonyPrefix]
    private static void KillPlayerPrefix(ref CauseOfDeath causeOfDeath)
    {
        if (ModConfig.GodMode != null &&
            !ModConfig.GodMode.Value &&
            causeOfDeath is CauseOfDeath.Suffocation or CauseOfDeath.Drowning)
            return;
    }
}
