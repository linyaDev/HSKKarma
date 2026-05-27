using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(QuestNode_GetPawnCountByPointsWeighted), "RunInt")]
public static class Patch_PrisonerCount
{
    public static void Postfix()
    {
        var settings = QuestPressureMod.Settings;
        if (settings == null || !settings.limitRefugees)
            return;

        var slate = QuestGen.slate;

        // Only apply to prisoner quests
        if (!slate.TryGet<bool>("lodgersArePrisoners", out bool isPrisoners) || !isPrisoners)
            return;

        if (!slate.TryGet<int>("lodgersCount", out int original))
            return;

        int maxPrisoners = settings.maxRefugees;
        if (original <= maxPrisoners)
            return;

        slate.Set("lodgersCount", maxPrisoners);
        Log.Message($"[KarmaHSK] Prisoners limited: {original} -> {maxPrisoners} (max={maxPrisoners})");
        GameComponent_QuestPressure.LogDebug($"Prisoners limited: {original} -> {maxPrisoners}");
    }
}
