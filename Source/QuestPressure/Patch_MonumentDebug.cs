using HarmonyLib;
using RimWorld.QuestGen;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(QuestNode_GetMonumentSketch), "RunInt")]
public static class Patch_MonumentDebug
{
    public static void Prefix(QuestNode_GetMonumentSketch __instance)
    {
        Log.Message("[KarmaHSK] Patch_MonumentDebug fired");
        var slate = QuestGen.slate;
        float points = slate.Get("points", 0f);
        float pointsPerArea = AccessTools.FieldRefAccess<QuestNode_GetMonumentSketch, SlateRef<float>>(__instance, "pointsPerArea").GetValue(slate);
        float area = points / pointsPerArea;

        string msg = $"Monument: points={points:F0}, pointsPerArea={pointsPerArea:F1}, area={area:F0}";
        Log.Message($"[KarmaHSK] {msg}");
        GameComponent_QuestPressure.LogDebug(msg);
    }
}
