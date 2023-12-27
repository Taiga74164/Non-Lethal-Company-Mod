using GameNetcodeStuff;
using HarmonyLib;

namespace NonLethalCompany_Mod.Modules;

public class DeadPlayerAlert : Module
{
    public override void Load()
    {
        Harmony.CreateAndPatchAll(typeof(DeadPlayerAlertPatch));
    }
}

[HarmonyPatch(typeof(PlayerControllerB))]
public class DeadPlayerAlertPatch
{
    [HarmonyPatch("KillPlayerClientRpc")]
    [HarmonyPrefix]
    private static void KillPlayerClientRpcPrefix(PlayerControllerB instance, ref int playerId, ref int causeOfDeath)
    {
        if (ModConfig.DeadPlayerAlert == null || !ModConfig.DeadPlayerAlert.Value) return;

        var player = instance.playersManager.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        var hud = HUDManager.Instance;
        var txt = "<color=#FFFFFF>" + player.playerUsername + " dead by " + (CauseOfDeath)causeOfDeath + "</color>";

        hud.DisplayTip("Player Dead", txt);
        hud.ChatMessageHistory.Add(txt);
        hud.chatText.text += "\n" + txt;

        while (hud.ChatMessageHistory.Count >= 4)
        {
            // hud.chatText.text.Remove(0, 1);
            hud.ChatMessageHistory.Remove(hud.ChatMessageHistory[0]);
        }

        hud.PingHUDElement(hud.Chat, 4f);
    }
}
