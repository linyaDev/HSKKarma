using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace KarmaHSK;

/// <summary>
/// Limits enemy count on site maps based on player tech level.
/// Neolithic: max 1, Medieval: max 2, Industrial+: unchanged.
/// Removes excess pawns after map generation.
/// </summary>
[HarmonyPatch(typeof(GenStep_Outpost), nameof(GenStep_Outpost.Generate))]
public static class Patch_SiteThreatByTech
{
    public static void Postfix(Map map)
    {
        var faction = map.ParentFaction;
        var parent = map.Parent;
        string mapInfo = $"map={parent?.def?.defName ?? "?"}, faction={faction?.Name ?? "none"}, tile={parent?.Tile ?? -1}";
        Log.Message($"[KarmaHSK] Patch_SiteThreatByTech fired: {mapInfo}");
        GameComponent_QuestPressure.LogDebug($"SiteThreatByTech fired: {mapInfo}");

        var techLevel = Faction.OfPlayer?.def?.techLevel ?? TechLevel.Industrial;

        int maxEnemies;
        switch (techLevel)
        {
            case TechLevel.Animal:
            case TechLevel.Neolithic:
                maxEnemies = 1;
                break;
            case TechLevel.Medieval:
                maxEnemies = 2;
                break;
            default:
                return;
        }

        if (faction == null || !faction.HostileTo(Faction.OfPlayer))
            return;

        var enemies = map.mapPawns.AllPawnsSpawned
            .Where(p => p.Faction == faction && !p.Dead && p.RaceProps.Humanlike)
            .ToList();

        if (enemies.Count <= maxEnemies)
            return;

        int removed = 0;
        // Remove weakest first (lowest combatPower)
        var toRemove = enemies
            .OrderBy(p => p.kindDef.combatPower)
            .Skip(maxEnemies)
            .ToList();

        foreach (var pawn in toRemove)
        {
            pawn.GetLord()?.RemovePawn(pawn);
            pawn.Destroy();
            removed++;
        }

        string msg = $"Site enemies capped: {enemies.Count} -> {maxEnemies} ({techLevel}, removed {removed})";
        Log.Message($"[KarmaHSK] {msg}");
        GameComponent_QuestPressure.LogDebug(msg);
    }
}
