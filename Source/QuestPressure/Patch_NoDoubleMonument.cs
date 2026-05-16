using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(QuestScriptDef), nameof(QuestScriptDef.CanRun), typeof(Slate))]
public static class Patch_NoDoubleMonument
{
    private static readonly HashSet<string> monumentQuests = new HashSet<string>
    {
        "BuildMonumentWorker",
        "BuildMonument_TimeProtect",
        "BuildMonument_Basic",
        "Decree_BuildMonument",
    };

    public static bool Prefix(QuestScriptDef __instance, ref bool __result)
    {
        if (!monumentQuests.Contains(__instance.defName))
            return true;

        if (!HasActiveMonumentQuest())
            return true;

        string msg = $"Monument quest blocked: {__instance.defName} (already active)";
        Messages.Message("QP_MonumentAlreadyActive".Translate(), MessageTypeDefOf.RejectInput, false);
        Log.Message($"[KarmaHSK] {msg}");
        GameComponent_QuestPressure.LogDebug(msg);
        __result = false;
        return false;
    }

    public static bool HasActiveMonumentQuest()
    {
        foreach (var quest in Find.QuestManager.QuestsListForReading)
        {
            if (quest.State != QuestState.Ongoing)
                continue;
            if (quest.root != null && monumentQuests.Contains(quest.root.defName))
                return true;
        }
        return false;
    }
}
