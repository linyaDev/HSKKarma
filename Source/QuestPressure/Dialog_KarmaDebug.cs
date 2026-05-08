using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace KarmaHSK;

public class Dialog_KarmaDebug : Window
{
    private Vector2 scrollPosition;
    private Rect? parentRect;

    public override Vector2 InitialSize => new Vector2(500f, 500f);

    public Dialog_KarmaDebug(Rect? parentRect = null)
    {
        this.parentRect = parentRect;
        doCloseButton = true;
        doCloseX = true;
        draggable = true;
        absorbInputAroundWindow = false;
    }

    public override void SetInitialSizeAndPosition()
    {
        base.SetInitialSizeAndPosition();
        if (parentRect.HasValue)
        {
            float x = parentRect.Value.xMax + 10f;
            float y = parentRect.Value.y;
            if (x + windowRect.width > UI.screenWidth)
                x = parentRect.Value.x - windowRect.width - 10f;
            if (y + windowRect.height > UI.screenHeight)
                y = UI.screenHeight - windowRect.height;
            windowRect.x = x;
            windowRect.y = y;
        }
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "KarmaHSK Debug");
        Text.Font = GameFont.Small;

        Rect scrollArea = new Rect(0f, 38f, inRect.width, inRect.height - 78f);
        float contentH = 800f;
        Rect viewRect = new Rect(0f, 0f, scrollArea.width - 16f, contentH);
        Widgets.BeginScrollView(scrollArea, ref scrollPosition, viewRect);

        float y = 0f;
        float w = viewRect.width;

        // Lodgers check
        bool hasLodgers = Patch_NoDoubleGuests.HasLodgersOnAnyMap();
        GUI.color = hasLodgers ? new Color(0.95f, 0.4f, 0.4f) : new Color(0.4f, 0.95f, 0.4f);
        Widgets.Label(new Rect(0f, y, w, 22f),
            "Guest quests blocked: " + (hasLodgers ? "YES" : "NO"));
        GUI.color = Color.white;
        y += 24f;

        if (hasLodgers)
        {
            foreach (var map in Find.Maps)
            {
                if (!map.IsPlayerHome) continue;
                foreach (var p in map.mapPawns.FreeColonistsSpawned.Where(p => p.IsQuestLodger()))
                {
                    Widgets.Label(new Rect(10f, y, w - 10f, 20f),
                        "  " + p.LabelShortCap + " (" + (p.guest?.HostFaction?.Name ?? "?") + ")");
                    y += 20f;
                }
            }
        }

        y += 6f;

        // Score
        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        if (comp != null)
        {
            Widgets.Label(new Rect(0f, y, w, 22f), "Karma score: " + comp.Score);
            y += 22f;
            Widgets.Label(new Rect(0f, y, w, 22f), "Records: " + comp.Records.Count);
            y += 26f;
        }

        // Settings impact
        GUI.color = new Color(1f, 1f, 0.7f);
        Widgets.Label(new Rect(0f, y, w, 22f), "--- Quest Settings ---");
        GUI.color = Color.white;
        y += 24f;

        var settings = QuestPressureMod.Settings;
        if (settings != null)
        {
            Widgets.Label(new Rect(0f, y, w, 20f), "Threat multiplier: " + (settings.threatMultiplier * 100f).ToString("F0") + "%");
            y += 20f;
            Widgets.Label(new Rect(0f, y, w, 20f), "Reward multiplier: " + (settings.rewardMultiplier * 100f).ToString("F0") + "%");
            y += 20f;

            // Current storyteller points
            var map = Find.CurrentMap;
            if (map != null)
            {
                float rawPoints = StorytellerUtility.DefaultThreatPointsNow(map);
                float adjustedPoints = rawPoints * settings.threatMultiplier;
                Widgets.Label(new Rect(0f, y, w, 20f), "Raw threat points: " + rawPoints.ToString("F0"));
                y += 20f;
                Widgets.Label(new Rect(0f, y, w, 20f), "After multiplier: " + adjustedPoints.ToString("F0"));
                y += 20f;
                Widgets.Label(new Rect(0f, y, w, 20f), "Colonists: " + map.mapPawns.FreeColonistsSpawnedCount);
                y += 20f;
                Widgets.Label(new Rect(0f, y, w, 20f), "Player tech level: " + Faction.OfPlayer.def.techLevel);
                y += 20f;
            }
        }

        y += 6f;

        // Active quests
        GUI.color = new Color(1f, 1f, 0.7f);
        Widgets.Label(new Rect(0f, y, w, 22f), "--- Active Quests ---");
        GUI.color = Color.white;
        y += 24f;

        foreach (var quest in Find.QuestManager.QuestsListForReading)
        {
            if (quest.State != QuestState.NotYetAccepted && quest.State != QuestState.Ongoing)
                continue;

            int parts = quest.PartsListForReading?.Count ?? 0;
            string root = quest.root?.defName ?? "null";
            bool isBugged = parts == 0 || (quest.name != null && quest.name.StartsWith("ERR:"));

            GUI.color = isBugged ? new Color(0.95f, 0.4f, 0.4f) : Color.white;
            Widgets.Label(new Rect(0f, y, w, 20f),
                quest.name + " [" + quest.State + "]");
            y += 20f;

            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.7f, 0.7f, 0.7f);
            Widgets.Label(new Rect(10f, y, w - 10f, 18f),
                "root=" + root + "  parts=" + parts +
                "  charity=" + quest.charity +
                (isBugged ? "  BUGGED" : ""));
            y += 18f;

            // List involved factions
            var factions = quest.InvolvedFactions?.ToList();
            if (factions != null && factions.Count > 0)
            {
                Widgets.Label(new Rect(10f, y, w - 10f, 18f),
                    "factions: " + string.Join(", ", factions.Select(f => f.Name)));
                y += 18f;
            }

            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            y += 4f;
        }

        // Debug event log
        if (GameComponent_QuestPressure.DebugLog.Count > 0)
        {
            y += 6f;
            GUI.color = new Color(1f, 1f, 0.7f);
            Widgets.Label(new Rect(0f, y, w, 22f), "--- Event Log ---");
            GUI.color = Color.white;
            y += 24f;

            Text.Font = GameFont.Tiny;
            for (int i = GameComponent_QuestPressure.DebugLog.Count - 1; i >= 0; i--)
            {
                Widgets.Label(new Rect(0f, y, w, 18f), GameComponent_QuestPressure.DebugLog[i]);
                y += 18f;
            }
            Text.Font = GameFont.Small;
        }

        Widgets.EndScrollView();
    }
}
