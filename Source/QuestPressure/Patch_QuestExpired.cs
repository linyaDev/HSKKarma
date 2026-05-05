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

    // Quests excluded from karma penalty when expired
    private static readonly HashSet<string> excludedQuests = new HashSet<string>
    {
        "OpportunitySite_WorkSite",
        "OpportunitySite_ItemStash"
    };

    public static void Postfix(Quest __instance, bool __state)
    {
        if (!__state)
            return;

        // Skip excluded quest types
        if (__instance.root != null && excludedQuests.Contains(__instance.root.defName))
            return;

        // Skip bugged quests (failed to generate properly)
        if (__instance.PartsListForReading == null || __instance.PartsListForReading.Count == 0
            || (__instance.name != null && __instance.name.StartsWith("ERR:")))
        {
            Messages.Message("QP_BuggedQuestSkipped".Translate(__instance.name ?? "Unknown"), MessageTypeDefOf.NeutralEvent, false);
            return;
        }

        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        if (comp == null) return;

        if (__instance.charity)
            comp.RecordQuest(__instance.name ?? "Unknown", __instance.id, QuestRecordType.CharityExpired);
        else
            comp.RecordQuest(__instance.name ?? "Unknown", __instance.id, QuestRecordType.Expired);
    }
}

[HarmonyPatch(typeof(Quest), nameof(Quest.End))]
public static class Patch_QuestCompleted
{
    public static void Postfix(Quest __instance, QuestEndOutcome outcome)
    {
        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        if (comp == null) return;

        string name = __instance.name ?? "Unknown";

        if (outcome == QuestEndOutcome.Success)
        {
            if (__instance.charity)
                comp.RecordQuest(name, __instance.id, QuestRecordType.CharityCompleted);
            else
                comp.RecordQuest(name, __instance.id, QuestRecordType.Completed);
        }
        else if (outcome == QuestEndOutcome.Fail)
        {
            // Skip excluded quest types
            if (__instance.root != null && excludedQuests.Contains(__instance.root.defName))
                return;

            // Skip bugged quests
            if (__instance.PartsListForReading == null || __instance.PartsListForReading.Count == 0
                || (__instance.name != null && __instance.name.StartsWith("ERR:")))
            {
                Messages.Message("QP_BuggedQuestSkipped".Translate(__instance.name ?? "Unknown"), MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            if (__instance.charity)
                comp.RecordQuest(name, __instance.id, QuestRecordType.CharityExpired);
            else
                comp.RecordQuest(name, __instance.id, QuestRecordType.Failed);
        }
    }

    private static readonly HashSet<string> excludedQuests = new HashSet<string>
    {
        "OpportunitySite_WorkSite",
        "OpportunitySite_ItemStash"
    };
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

        float original = __result.points;
        __result.points *= settings.threatMultiplier;
        Log.Message($"[KarmaHSK] Threat reduced: {original:F0} -> {__result.points:F0} (x{settings.threatMultiplier})");
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

        float original = parms.rewardValue;
        parms.rewardValue *= settings.rewardMultiplier;
        Log.Message($"[KarmaHSK] Reward reduced: {original:F0} -> {parms.rewardValue:F0} (x{settings.rewardMultiplier})");
    }
}
