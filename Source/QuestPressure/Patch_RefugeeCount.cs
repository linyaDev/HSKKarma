using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(QuestNode_Root_Beggars), nameof(QuestNode_Root_Beggars.LodgerCountFromPopulation))]
public static class Patch_RefugeeCount
{
    public static void Postfix(ref int __result, Map map)
    {
        var settings = QuestPressureMod.Settings;
        if (settings == null || !settings.limitRefugees)
            return;

        int colonists = map.mapPawns.FreeColonistsSpawnedCount;
        int randomOffset = Rand.RangeInclusive(-1, 1);
        int limit = Mathf.Min(colonists / 2 + randomOffset, settings.maxRefugees);
        limit = Mathf.Max(limit, 1);
        int original = __result;

        __result = Mathf.Min(__result, limit);

        Log.Message($"[KarmaHSK] RefugeeCount: colonists={colonists}, original={original}, random={randomOffset}, limit={limit}, maxSetting={settings.maxRefugees}, result={__result}");
    }
}
