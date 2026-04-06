using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KarmaHSK;

/// <summary>
/// Track wild man events:
/// - Killed by colonist: -2
/// - Wounded by colonist: -1
/// - Spawned on map: +1
/// </summary>

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
public static class Patch_WildManKilled
{
    public static void Prefix(Pawn __instance, DamageInfo? dinfo)
    {
        if (!__instance.IsWildMan())
            return;

        // Check if killer is a colonist
        Pawn killer = dinfo?.Instigator as Pawn;
        if (killer == null || !killer.IsColonist)
            return;

        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        if (comp == null)
            return;

        // -4 via MajorPenalty weight
        comp.RecordQuest("QP_WildManKilled".Translate(), -1, QuestRecordType.MajorPenalty);
    }
}

[HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
public static class Patch_WildManWounded
{
    private static readonly HashSet<int> woundedThisDay = new HashSet<int>();
    private static int lastCleanupDay;

    public static void Postfix(Pawn __instance, DamageInfo dinfo)
    {
        if (__instance.Dead)
            return;

        if (!__instance.IsWildMan())
            return;

        Pawn attacker = dinfo.Instigator as Pawn;
        if (attacker == null || !attacker.IsColonist)
            return;

        // Only once per wild man per day
        int today = GenDate.DaysPassed;
        if (today != lastCleanupDay)
        {
            woundedThisDay.Clear();
            lastCleanupDay = today;
        }

        if (woundedThisDay.Contains(__instance.thingIDNumber))
            return;
        woundedThisDay.Add(__instance.thingIDNumber);

        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        // -1 via MinorPenalty weight
        comp?.RecordQuest("QP_WildManWounded".Translate(), -1, QuestRecordType.MinorPenalty);
    }
}

[HarmonyPatch(typeof(IncidentWorker), nameof(IncidentWorker.TryExecute))]
public static class Patch_WildManIncident
{
    public static void Postfix(IncidentWorker __instance, IncidentParms parms, bool __result)
    {
        if (!__result)
            return;

        string defName = __instance.def?.defName;
        if (defName == "WildManWandersIn")
        {
            var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
            comp?.RecordQuest("QP_WildManArrived".Translate(), -1, QuestRecordType.MinorBonus);
        }
        else if (defName == "WildMenGroupWanderIn")
        {
            int count = parms.pawnCount > 0 ? parms.pawnCount : 1;
            var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
            if (comp == null) return;

            for (int i = 0; i < count; i++)
                comp.RecordQuest("QP_WildManArrived".Translate(), -1, QuestRecordType.MinorBonus);
        }
    }
}
