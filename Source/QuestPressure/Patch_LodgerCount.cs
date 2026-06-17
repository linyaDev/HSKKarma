using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld.QuestGen;
using Verse;

namespace KarmaHSK;

/// <summary>
/// Caps the number of quest lodgers generated through the shared Hospitality count nodes.
/// Covers prisoners and joiners (both resolve lodgersCount via these nodes); animal-lodger
/// quests are skipped. Uses the unified era-based formula in <see cref="LodgerLimit"/>.
/// Refugees/beggars/helpers go through a different node — see <see cref="Patch_RefugeeCount"/>.
/// </summary>
[HarmonyPatch]
public static class Patch_LodgerCount
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(QuestNode_GetPawnCountByPointsWeighted), "RunInt");
        yield return AccessTools.Method(typeof(QuestNode_EvaluateSimpleCurve), "RunInt");
    }

    public static void Postfix(QuestNode __instance)
    {
        var settings = QuestPressureMod.Settings;
        if (settings == null || !settings.limitRefugees)
            return;

        // Both target nodes expose a storeAs SlateRef<string>; only act when they wrote lodgersCount.
        if (GetStoreAs(__instance) != "lodgersCount")
            return;

        // Animal lodger quests use the same node — leave those alone.
        string root = QuestGen.quest?.root?.defName;
        if (root == "Hospitality_Animals")
            return;

        var slate = QuestGen.slate;
        if (!slate.TryGet<int>("lodgersCount", out int original))
            return;

        int limit = LodgerLimit.Compute();
        if (original <= limit)
            return;

        slate.Set("lodgersCount", limit);
        string msg = $"Lodgers limited ({root ?? "?"}): {original} -> {limit}";
        Log.Message($"[KarmaHSK] {msg}");
        GameComponent_QuestPressure.LogDebug(msg);
    }

    private static string GetStoreAs(QuestNode node)
    {
        var slate = QuestGen.slate;
        switch (node)
        {
            case QuestNode_GetPawnCountByPointsWeighted n:
                return n.storeAs.GetValue(slate);
            case QuestNode_EvaluateSimpleCurve n:
                return n.storeAs.GetValue(slate);
            default:
                return null;
        }
    }
}
