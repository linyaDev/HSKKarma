using HarmonyLib;
using RimWorld;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(StorytellerComp), nameof(StorytellerComp.GenerateParms))]
public static class Patch_QuestThreat
{
    public static void Postfix(ref IncidentParms __result, IncidentCategoryDef incCat, StorytellerComp __instance)
    {
        if (incCat != IncidentCategoryDefOf.GiveQuest)
            return;

        var settings = QuestPressureMod.Settings;
        if (settings == null)
            return;

        bool isSite = __instance is StorytellerComp_WorkSite;
        float multiplier = isSite ? settings.siteThreatMultiplier : settings.threatMultiplier;

        float original = __result.points;
        __result.points *= multiplier;
        string questInfo = __result.questScriptDef?.defName ?? __result.quest?.name ?? "?";
        string label = isSite ? "Site" : "Quest";
        string msg = $"{label} points reduced: {original:F0} -> {__result.points:F0} (x{multiplier}), quest: {questInfo}";
        Log.Message($"[KarmaHSK] {msg}");
        GameComponent_QuestPressure.LogDebug(msg);
    }
}
