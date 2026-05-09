using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace KarmaHSK;

public class Dialog_QuestDebug : Window
{
    private Vector2 scrollPosition;
    private Rect? parentRect;

    public override Vector2 InitialSize => new Vector2(550f, 600f);

    public Dialog_QuestDebug(Rect? parentRect = null)
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
        Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "Active Quests");
        Text.Font = GameFont.Small;

        Rect scrollArea = new Rect(0f, 38f, inRect.width, inRect.height - 78f);
        float contentH = 2000f;
        Rect viewRect = new Rect(0f, 0f, scrollArea.width - 16f, contentH);
        Widgets.BeginScrollView(scrollArea, ref scrollPosition, viewRect);

        float y = 0f;
        float w = viewRect.width;

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

        Widgets.EndScrollView();
    }
}
