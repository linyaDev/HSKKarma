using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace KarmaHSK;

/// <summary>
/// Filters site factions based on settings — auto mode (same tech level) or manual whitelist.
/// </summary>
[HarmonyPatch(typeof(SiteMakerHelper), "FactionCanOwn",
    typeof(SitePartDef), typeof(Faction), typeof(bool), typeof(System.Predicate<Faction>))]
public static class Patch_SiteFactionFilter
{
    public static void Postfix(ref bool __result, SitePartDef sitePart, Faction faction)
    {
        if (!__result || faction == null) return;
        if (!faction.HostileTo(Faction.OfPlayer)) return;

        var settings = QuestPressureMod.Settings;
        if (settings == null) return;

        string site = sitePart?.defName ?? "?";
        bool allowed = settings.IsFactionAllowed(faction.def.defName, faction.def.techLevel);

        if (!allowed)
        {
            __result = false;
            GameComponent_QuestPressure.LogDebug(
                $"Site faction blocked: {site}, {faction.Name} ({faction.def.defName}, {faction.def.techLevel})");
        }
    }
}
