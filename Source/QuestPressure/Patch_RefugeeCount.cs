using HarmonyLib;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace KarmaHSK;

// Caps beggars and refugees/helpers, which resolve their count through this node.
// Joiners and prisoners use a different node — see Patch_LodgerCount.
[HarmonyPatch(typeof(QuestNode_Root_Beggars), nameof(QuestNode_Root_Beggars.LodgerCountFromPopulation))]
public static class Patch_RefugeeCount
{
    public static void Postfix(ref int __result)
    {
        var settings = QuestPressureMod.Settings;
        if (settings == null || !settings.limitRefugees)
            return;

        int limit = LodgerLimit.Compute();
        if (__result <= limit)
            return;

        int original = __result;
        __result = Mathf.Min(__result, limit);
        GameComponent_QuestPressure.LogDebug($"Refugees/helpers limited: {original} -> {__result}");
    }
}
