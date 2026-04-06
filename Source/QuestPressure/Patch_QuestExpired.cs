using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KarmaHSK;

[StaticConstructorOnStartup]
public static class QuestPressureInit
{
    static QuestPressureInit()
    {
        var harmony = new Harmony("linya.karmahsk");
        harmony.PatchAll();
        Log.Message("[QuestPressure] Patches applied.");
    }
}

[HarmonyPatch(typeof(Quest), nameof(Quest.QuestTick))]
public static class Patch_QuestExpired
{
    public static void Prefix(Quest __instance, out bool __state)
    {
#if V15
        __state = __instance.ticksUntilAcceptanceExpiry == 1
                  && __instance.State == QuestState.NotYetAccepted
                  && !__instance.cleanedUp;
#else
        __state = __instance.TicksUntilExpiry == 0
                  && __instance.State == QuestState.NotYetAccepted
                  && !__instance.cleanedUp;
#endif
    }

    public static void Postfix(Quest __instance, bool __state)
    {
        if (!__state)
            return;

        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        comp?.RecordQuest(__instance.name ?? "Unknown", __instance.id, QuestRecordType.Expired);
    }
}

[HarmonyPatch(typeof(Quest), nameof(Quest.End))]
public static class Patch_QuestCompleted
{
    public static void Postfix(Quest __instance, QuestEndOutcome outcome)
    {
        if (outcome != QuestEndOutcome.Success)
            return;

        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        comp?.RecordQuest(__instance.name ?? "Unknown", __instance.id, QuestRecordType.Completed);
    }
}

[HarmonyPatch(typeof(StorytellerComp), nameof(StorytellerComp.GenerateParms))]
public static class Patch_QuestThreatMultiplier
{
    public static void Postfix(ref IncidentParms __result, IncidentCategoryDef incCat)
    {
        if (incCat != IncidentCategoryDefOf.GiveQuest)
            return;

        var settings = QuestPressureMod.Settings;
        if (settings == null)
            return;

        __result.points *= settings.threatMultiplier;
    }
}

[HarmonyPatch(typeof(RewardsGenerator), nameof(RewardsGenerator.Generate),
    new[] { typeof(RewardsGeneratorParams), typeof(float) },
    new[] { ArgumentType.Normal, ArgumentType.Out })]
public static class Patch_QuestRewardMultiplier
{
    public static void Prefix(ref RewardsGeneratorParams parms)
    {
        var settings = QuestPressureMod.Settings;
        if (settings == null)
            return;

        parms.rewardValue *= settings.rewardMultiplier;
    }
}
