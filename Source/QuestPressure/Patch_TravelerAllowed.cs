using HarmonyLib;
using RimWorld;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(IncidentWorker_TravelerGroup), "TryExecuteWorker")]
public static class Patch_TravelerAllowed
{
    public static void Postfix(bool __result, IncidentParms parms)
    {
        // Only count when player explicitly allowed (forced = true from ABE dialog)
        if (!__result || !parms.forced)
            return;

        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        comp?.RecordQuest("QP_GuestsAllowed".Translate(), 0, QuestRecordType.TinyBonus);
    }
}
