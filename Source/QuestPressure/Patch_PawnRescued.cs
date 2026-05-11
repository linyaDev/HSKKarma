using HarmonyLib;
using RimWorld;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.Notify_PawnUndowned))]
public static class Patch_PawnRescued
{
    public static void Postfix(Pawn_GuestTracker __instance)
    {
        Log.Message("[KarmaHSK] Patch_PawnRescued fired");
        var pawn = __instance.pawn;
        if (pawn == null || !pawn.RaceProps.Humanlike)
            return;

        if (!__instance.getRescuedThoughtOnUndownedBecauseOfPlayer)
            return;

        GameComponent_QuestPressure.LogDebug($"Pawn rescued: {pawn.LabelShort}");
    }
}
