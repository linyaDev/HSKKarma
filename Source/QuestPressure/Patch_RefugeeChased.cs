using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KarmaHSK;

[StaticConstructorOnStartup]
public static class Patch_RefugeeChased
{
    static Patch_RefugeeChased()
    {
        var type = AccessTools.TypeByName("SK.IncidentWorker_RefugeeChased");
        if (type == null)
        {
            Log.Message("[KarmaHSK] SK.IncidentWorker_RefugeeChased not found, skipping patch.");
            return;
        }

        var method = AccessTools.Method(type, "TryExecuteWorker");
        if (method == null)
        {
            Log.Message("[KarmaHSK] TryExecuteWorker not found on RefugeeChased, skipping patch.");
            return;
        }

        var harmony = new Harmony("linya.karmahsk.refugeechased");
        harmony.Patch(method, postfix: new HarmonyMethod(typeof(Patch_RefugeeChased), nameof(Postfix)));
        Log.Message("[KarmaHSK] RefugeeChased patch applied.");
    }

    private static FieldInfo diaNodeField;
    private static FieldInfo incidentParmsField;

    public static void Postfix(object __instance, bool __result)
    {
        if (!__result) return;

        // Cache reflection
        if (diaNodeField == null)
        {
            var type = __instance.GetType();
            diaNodeField = AccessTools.Field(type, "diaNode");
            incidentParmsField = AccessTools.Field(type, "incidentParms");
        }

        var diaNode = diaNodeField?.GetValue(__instance) as DiaNode;
        var parms = incidentParmsField?.GetValue(__instance) as IncidentParms;

        if (diaNode == null || diaNode.options == null || diaNode.options.Count < 2)
            return;

        // Apply threat multiplier to raid incidentParms
        var settings = QuestPressureMod.Settings;
        if (settings != null && parms != null)
        {
            float original = parms.points;
            parms.points *= settings.threatMultiplier;
            Log.Message($"[KarmaHSK] Refugee raid threat reduced: {original:F0} -> {parms.points:F0} (x{settings.threatMultiplier})");
        }

        // Wrap Accept action (first option) with karma bonus
        var acceptOption = diaNode.options[0];
        var originalAcceptAction = acceptOption.action;
        acceptOption.action = delegate
        {
            originalAcceptAction?.Invoke();
            var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
            comp?.RecordQuest("QP_RefugeeAccepted".Translate(), 0, QuestRecordType.CharityCompleted);
        };

        // Wrap Reject action (second option) with karma penalty
        var rejectOption = diaNode.options[1];
        var originalRejectAction = rejectOption.action;
        rejectOption.action = delegate
        {
            originalRejectAction?.Invoke();
            var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
            comp?.RecordQuest("QP_RefugeeRejected".Translate(), 0, QuestRecordType.CharityExpired);
        };
    }
}
