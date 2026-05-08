using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(QuestNode_Root_PollutionDump), "RunInt")]
public static class Patch_WastepackCount
{
    private static float originalPoints;

    public static void Prefix()
    {
        var settings = QuestPressureMod.Settings;
        if (settings == null || !settings.nerfWastepacks)
            return;

        var slate = QuestGen.slate;
        if (!slate.TryGet<float>("points", out float points))
            return;

        originalPoints = points;

        float multiplier = settings.wastepackBaseMultiplier;

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
            case TechLevel.Industrial:
                multiplier *= settings.wastepackIndustrialMult;
                break;
        }

        float newPoints = points * multiplier;
        slate.Set("points", newPoints);
        GameComponent_QuestPressure.LogDebug(
            $"Wastepack points reduced: {points:F0} -> {newPoints:F0} (x{multiplier:F2})");
    }

    public static void Postfix()
    {
        var settings = QuestPressureMod.Settings;
        if (settings == null || !settings.nerfWastepacks)
            return;

        if (originalPoints > 0f)
        {
            QuestGen.slate.Set("points", originalPoints);
            originalPoints = 0f;
        }
    }
}
