using GameNetcodeStuff;
using HarmonyLib;

namespace NonLethalCompany_Mod.Modules;

public class NoFall : Module
{
    public override void Load()
    {
        Harmony.CreateAndPatchAll(typeof(NoFallPatch));
    }
}

[HarmonyPatch(typeof(PlayerControllerB))]
public class NoFallPatch
{
    [HarmonyPatch(nameof(PlayerControllerB.DamagePlayer))]
    [HarmonyPrefix]
    private static void DamagePlayerPrefix(ref int damageNumber, ref CauseOfDeath causeOfDeath, ref bool fallDamage)
    {
        if (ModConfig.NoFall != null && !ModConfig.NoFall.Value)
            return;

        if (!fallDamage && causeOfDeath != CauseOfDeath.Gravity)
            return;

        damageNumber = 0;
    }
}
