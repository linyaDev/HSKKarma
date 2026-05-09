using HarmonyLib;
using RimWorld;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(StorytellerComp), nameof(StorytellerComp.GenerateParms))]
public static class Patch_QuestThreat
{
    public static void Postfix(ref IncidentParms __result, IncidentCategoryDef incCat)
    {
        if (incCat != IncidentCategoryDefOf.GiveQuest)
            return;

        var settings = QuestPressureMod.Settings;
        if (settings == null)
            return;

        float original = __result.points;
        __result.points *= settings.threatMultiplier;
        string questInfo = __result.questScriptDef?.defName ?? __result.quest?.name ?? "?";
        string msg = $"Quest points reduced: {original:F0} -> {__result.points:F0} (x{settings.threatMultiplier}), quest: {questInfo}";
        Log.Message($"[KarmaHSK] {msg}");
        GameComponent_QuestPressure.LogDebug(msg);
    }
}
