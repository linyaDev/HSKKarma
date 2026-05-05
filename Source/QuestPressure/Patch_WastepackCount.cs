using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(QuestNode_Root_PollutionDump), "RunInt")]
public static class Patch_WastepackCount
{
    public static void Postfix()
    {
        var settings = QuestPressureMod.Settings;
        if (settings == null || !settings.nerfWastepacks)
            return;

        var slate = QuestGen.slate;
        if (!slate.TryGet<int>("wastepackCount", out int count))
            return;

        // Base reduction
        float multiplier = settings.wastepackBaseMultiplier;

        // Epoch reduction
        var techLevel = Faction.OfPlayer?.def?.techLevel ?? TechLevel.Industrial;
        switch (techLevel)
        {
            case TechLevel.Animal:
            case TechLevel.Neolithic:
                multiplier *= settings.wastepackNeolithicMult;
                break;
            case TechLevel.Medieval:
                multiplier *= settings.wastepackMedievalMult;
                break;
        }

        int newCount = Mathf.Max(Mathf.RoundToInt(count * multiplier), 10);
        slate.Set("wastepackCount", newCount);
        Log.Message($"[KarmaHSK] Wastepack reduced: {count} -> {newCount} (x{multiplier:F2})");

    }
}
