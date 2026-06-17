using RimWorld;
using UnityEngine;
using Verse;

namespace KarmaHSK;

/// <summary>
/// Unified lodger-count limit for all quest guests (refugees, helpers, prisoners, joiners).
/// Base count scales with the player faction's tech era, then ±1 random, minimum 1.
/// Era table is intentionally hardcoded (no settings).
/// </summary>
public static class LodgerLimit
{
    // Base lodger count per era (before the ±1 jitter).
    public static int BaseForEra(TechLevel tech)
    {
        switch (tech)
        {
            case TechLevel.Animal:
            case TechLevel.Neolithic:
                return 1;
            case TechLevel.Medieval:
                return 2;
            case TechLevel.Industrial:
                return 3;
            case TechLevel.Spacer:
                return 4;
            default: // Ultra, Archotech
                return 5;
        }
    }

    /// <summary>
    /// Computes the lodger cap for the current colony: era base ±1, clamped to a minimum of 1.
    /// </summary>
    public static int Compute()
    {
        var tech = Faction.OfPlayer?.def?.techLevel ?? TechLevel.Industrial;
        int limit = BaseForEra(tech) + Rand.RangeInclusive(-1, 1);
        return Mathf.Max(1, limit);
    }
}
