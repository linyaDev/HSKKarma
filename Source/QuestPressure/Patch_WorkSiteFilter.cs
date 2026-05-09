using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace KarmaHSK;

/// <summary>
/// Filters WorkSite candidates — only Mining allowed.
/// </summary>
[HarmonyPatch(typeof(QuestNode_Root_WorkSite), "GetCandidates")]
public static class Patch_WorkSiteFilter
{
    private static readonly HashSet<string> allowedWorkSites = new HashSet<string>
    {
        "WorkSite_Mining"
    };

    public static IEnumerable<object> Postfix(IEnumerable<object> __result)
    {
        foreach (var candidate in __result)
        {
            // SiteSpawnCandidate is a private struct, access sitePart via reflection
            var sitePartField = AccessTools.Field(candidate.GetType(), "sitePart");
            var sitePart = sitePartField?.GetValue(candidate) as SitePartDef;

            if (sitePart != null && !allowedWorkSites.Contains(sitePart.defName))
            {
                GameComponent_QuestPressure.LogDebug($"WorkSite filtered: {sitePart.defName}");
                continue;
            }

            yield return candidate;
        }
    }
}
