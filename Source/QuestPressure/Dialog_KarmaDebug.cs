using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace KarmaHSK;

public class Dialog_KarmaDebug : Window
{
    public override Vector2 InitialSize => new Vector2(400f, 300f);

    public Dialog_KarmaDebug()
    {
        doCloseButton = true;
        doCloseX = true;
        draggable = true;
        absorbInputAroundWindow = false;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "KarmaHSK Debug");
        Text.Font = GameFont.Small;

        float y = 40f;

        // Lodgers check
        bool hasLodgers = Patch_NoDoubleGuests.HasLodgersOnAnyMap();
        GUI.color = hasLodgers ? new Color(0.95f, 0.4f, 0.4f) : new Color(0.4f, 0.95f, 0.4f);
        Widgets.Label(new Rect(0f, y, inRect.width, 24f),
            "Guest quests blocked: " + (hasLodgers ? "YES" : "NO"));
        GUI.color = Color.white;
        y += 26f;

        // List lodgers
        if (hasLodgers)
        {
            foreach (var map in Find.Maps)
            {
                if (!map.IsPlayerHome) continue;
                foreach (var p in map.mapPawns.FreeColonistsSpawned.Where(p => p.IsQuestLodger()))
                {
                    Widgets.Label(new Rect(10f, y, inRect.width - 10f, 22f),
                        "  " + p.LabelShortCap + " (" + (p.guest?.HostFaction?.Name ?? "?") + ")");
                    y += 22f;
                }
            }
        }

        y += 10f;

        // Score
        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        if (comp != null)
        {
            Widgets.Label(new Rect(0f, y, inRect.width, 24f), "Karma score: " + comp.Score);
            y += 26f;
            Widgets.Label(new Rect(0f, y, inRect.width, 24f), "Records: " + comp.Records.Count);
        }
    }
}
