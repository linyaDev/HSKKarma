using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace KarmaHSK;

/// <summary>
/// Prevents site quests from spawning factions above the player's tech level.
/// </summary>
[HarmonyPatch(typeof(SiteMakerHelper), "FactionCanOwn",
    typeof(SitePartDef), typeof(Faction), typeof(bool), typeof(System.Predicate<Faction>))]
public static class Patch_SiteFactionTechLevel
{
    public static void Postfix(ref bool __result, Faction faction)
    {
        if (!__result || faction == null) return;

        if ((int)faction.def.techLevel > (int)Faction.OfPlayer.def.techLevel)
        {
            __result = false;
            GameComponent_QuestPressure.LogDebug(
                $"Site faction skipped: {faction.Name} ({faction.def.techLevel}) > player ({Faction.OfPlayer.def.techLevel})");
        }
    }
}
