using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace KarmaHSK;

public class Dialog_MercyInfo : Window
{
    private readonly Pawn pawn;
    private Vector2 scrollPosition;
    private HashSet<int> selectedIndices = new HashSet<int>();
    private bool editMode;

    private static readonly Color GreenBg = new Color(0.2f, 0.5f, 0.2f, 0.3f);
    private static readonly Color RedBg = new Color(0.5f, 0.2f, 0.2f, 0.3f);
    private static readonly Color SelectedBg = new Color(0.3f, 0.3f, 0.8f, 0.4f);
    private static readonly Color GreenText = new Color(0.4f, 0.95f, 0.4f);
    private static readonly Color RedText = new Color(0.95f, 0.4f, 0.4f);
    private static readonly Color DimText = new Color(1f, 1f, 1f, 0.5f);

    public override Vector2 InitialSize => new Vector2(460f, 520f);

    public Dialog_MercyInfo(Pawn pawn)
    {
        this.pawn = pawn;
        doCloseButton = true;
        doCloseX = true;
        draggable = true;
        absorbInputAroundWindow = false;
    }

    public override void DoWindowContents(Rect inRect)
    {
        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        if (comp == null)
            return;

        // Title
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "QP_MercyHistory".Translate());
        Text.Font = GameFont.Small;

        float y = 40f;

        // Stats summary
        int completed = 0, expired = 0;
        foreach (var r in comp.Records)
        {
            if (r.type == QuestRecordType.Completed) completed++;
            else expired++;
        }

        int gained = completed * GameComponent_QuestPressure.CompletedWeight;
        int lost = expired * GameComponent_QuestPressure.ExpiredWeight;

        // Score bar area
        Rect statsRect = new Rect(0f, y, inRect.width, 50f);
        Widgets.DrawBoxSolid(statsRect, new Color(0.15f, 0.15f, 0.15f, 0.8f));

        float thirdW = inRect.width / 3f;
        Text.Anchor = TextAnchor.MiddleCenter;

        // Gained
        GUI.color = GreenText;
        Widgets.Label(new Rect(0f, y + 2f, thirdW, 22f), "QP_Gained".Translate());
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0f, y + 22f, thirdW, 26f), "+" + gained);
        Text.Font = GameFont.Small;

        // Score
        GUI.color = comp.Score >= 0 ? GreenText : RedText;
        Widgets.Label(new Rect(thirdW, y + 2f, thirdW, 22f), "QP_Score".Translate());
        Text.Font = GameFont.Medium;
        string scoreStr = comp.Score >= 0 ? "+" + comp.Score : comp.Score.ToString();
        Widgets.Label(new Rect(thirdW, y + 22f, thirdW, 26f), scoreStr);
        Text.Font = GameFont.Small;

        // Lost
        GUI.color = RedText;
        Widgets.Label(new Rect(thirdW * 2f, y + 2f, thirdW, 22f), "QP_Lost".Translate());
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(thirdW * 2f, y + 22f, thirdW, 26f), lost.ToString());
        Text.Font = GameFont.Small;

        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
        y += 56f;

        // === Progress bar with colored stages ===
        y = DrawProgressBar(inRect, y, comp.Score);

        // Separator
        GUI.color = new Color(1f, 1f, 1f, 0.2f);
        Widgets.DrawLineHorizontal(0f, y, inRect.width);
        GUI.color = Color.white;
        y += 4f;

        // Edit buttons
        Rect editBtnRect = new Rect(inRect.width - 80f, y, 75f, 24f);
        if (editMode)
        {
            // Delete button
            if (selectedIndices.Count > 0)
            {
                Rect delBtnRect = new Rect(inRect.width - 165f, y, 80f, 24f);
                GUI.color = RedText;
                if (Widgets.ButtonText(delBtnRect, "QP_Delete".Translate()))
                {
                    var sorted = new List<int>(selectedIndices);
                    sorted.Sort((a, b) => b.CompareTo(a));
                    foreach (int idx in sorted)
                    {
                        if (idx >= 0 && idx < comp.Records.Count)
                            comp.Records.RemoveAt(idx);
                    }
                    selectedIndices.Clear();
                }
                GUI.color = Color.white;
            }

            if (Widgets.ButtonText(editBtnRect, "QP_Done".Translate()))
            {
                editMode = false;
                selectedIndices.Clear();
            }
        }
        else
        {
            if (Widgets.ButtonText(editBtnRect, "QP_Edit".Translate()))
                editMode = true;
        }
        y += 28f;

        // Quest list
        var records = comp.Records;
        float listHeight = records.Count * 30f;
        Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, listHeight);
        Rect outRect = new Rect(0f, y, inRect.width, inRect.height - y - 50f);

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

        float rowY = 0f;
        for (int i = records.Count - 1; i >= 0; i--)
        {
            var r = records[i];
            Rect rowRect = new Rect(0f, rowY, viewRect.width, 28f);

            bool isPositive = r.type == QuestRecordType.Completed || r.type == QuestRecordType.MinorBonus || r.type == QuestRecordType.CharityCompleted;
            int points;
            switch (r.type)
            {
                case QuestRecordType.Completed: points = GameComponent_QuestPressure.CompletedWeight; break;
                case QuestRecordType.MinorBonus: points = GameComponent_QuestPressure.MinorBonusWeight; break;
                case QuestRecordType.MinorPenalty: points = GameComponent_QuestPressure.MinorPenaltyWeight; break;
                case QuestRecordType.MajorPenalty: points = GameComponent_QuestPressure.MajorPenaltyWeight; break;
                case QuestRecordType.CharityCompleted: points = GameComponent_QuestPressure.CharityCompletedWeight; break;
                case QuestRecordType.CharityExpired: points = GameComponent_QuestPressure.CharityExpiredWeight; break;
                default: points = GameComponent_QuestPressure.ExpiredWeight; break;
            }
            string pointsStr = isPositive ? "+" + points : points.ToString();

            // Row background
            bool isSelected = selectedIndices.Contains(i);
            if (isSelected)
                Widgets.DrawBoxSolid(rowRect, SelectedBg);
            else
                Widgets.DrawBoxSolid(rowRect, isPositive ? GreenBg : RedBg);

            // Edit mode: click to select/deselect
            if (editMode && Widgets.ButtonInvisible(rowRect))
            {
                if (isSelected)
                    selectedIndices.Remove(i);
                else
                    selectedIndices.Add(i);
            }
            if (i % 2 == 0)
                Widgets.DrawLightHighlight(rowRect);

            // Hover highlight + click to open quest
            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
                if (r.questId >= 0 && Widgets.ButtonInvisible(rowRect))
                {
                    Quest quest = Find.QuestManager.QuestsListForReading.Find(q => q.id == r.questId);
                    if (quest != null)
                    {
                        Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
                        ((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
                        Close();
                    }
                }
            }

            // Icon
            GUI.color = isPositive ? GreenText : RedText;
            string icon = isPositive ? "▲" : "▼";
            Widgets.Label(new Rect(5f, rowY, 20f, 28f), icon);

            // Quest name (truncate to 1 line)
            GUI.color = Color.white;
            float nameWidth = viewRect.width - 170f;
            string questName = r.questName;
            if (questName.Length > 25)
                questName = questName.Substring(0, 22) + "...";
            Widgets.Label(new Rect(25f, rowY, nameWidth, 28f), questName);
            if (r.questName.Length > 25 && Mouse.IsOver(new Rect(25f, rowY, nameWidth, 28f)))
                TooltipHandler.TipRegion(new Rect(25f, rowY, nameWidth, 28f), r.questName);

            // Points
            GUI.color = isPositive ? GreenText : RedText;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(viewRect.width - 140f, rowY, 50f, 28f), pointsStr);

            // Time ago
            GUI.color = DimText;
            Text.Anchor = TextAnchor.MiddleRight;
            int daysAgo = (Find.TickManager.TicksGame - r.tick) / 60000;
            string timeStr = daysAgo <= 0 ? "QP_Today".Translate().ToString() : "QP_DaysAgo".Translate(daysAgo);
            Widgets.Label(new Rect(viewRect.width - 85f, rowY, 80f, 28f), timeStr);

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            rowY += 30f;
        }

        Widgets.EndScrollView();
    }

    private static readonly int[] StageMoods = { -20, -15, -10, -5, -2, 0, 2, 4, 6, 8 };
    private static readonly float[] StageThresholds = { 0.10f, 0.20f, 0.30f, 0.40f, 0.50f, 0.60f, 0.70f, 0.80f, 0.90f, 1.0f };
    private static readonly Color[] StageColors =
    {
        new Color(0.7f, 0.1f, 0.1f),   // -20 dark red
        new Color(0.85f, 0.2f, 0.2f),  // -15 red
        new Color(0.95f, 0.3f, 0.2f),  // -10 orange-red
        new Color(0.95f, 0.5f, 0.2f),  // -5  orange
        new Color(0.95f, 0.75f, 0.2f), // -2  yellow
        new Color(0.7f, 0.7f, 0.7f),   // 0   gray
        new Color(0.4f, 0.75f, 0.3f),  // +2  light green
        new Color(0.3f, 0.85f, 0.3f),  // +4  green
        new Color(0.2f, 0.9f, 0.4f),   // +6  bright green
        new Color(0.1f, 0.95f, 0.5f),  // +8  vivid green
    };
    private static readonly string[] StageLabels =
    {
        "QP_Stage0", "QP_Stage1", "QP_Stage2", "QP_Stage3", "QP_Stage4",
        "QP_Stage5", "QP_Stage6", "QP_Stage7", "QP_Stage8", "QP_Stage9"
    };

    private float DrawProgressBar(Rect inRect, float y, int score)
    {
        const float ScoreMin = -30f;
        const float ScoreMax = 30f;
        float barHeight = 18f;
        float barX = 20f;
        float barWidth = inRect.width - 40f;

        // Background
        Rect barBg = new Rect(barX, y, barWidth, barHeight);
        Widgets.DrawBoxSolid(barBg, new Color(0.1f, 0.1f, 0.1f, 0.8f));

        // Draw colored segments
        float prevX = 0f;
        for (int i = 0; i < StageThresholds.Length; i++)
        {
            float segEnd = StageThresholds[i] * barWidth;
            Rect segRect = new Rect(barX + prevX, y, segEnd - prevX, barHeight);
            Widgets.DrawBoxSolid(segRect, StageColors[i]);

            // Segment border
            GUI.color = new Color(0f, 0f, 0f, 0.3f);
            Widgets.DrawBox(segRect, 1);
            GUI.color = Color.white;

            prevX = segEnd;
        }

        // Current position marker
        float normalizedScore = Mathf.InverseLerp(ScoreMin, ScoreMax, score);
        float markerX = barX + normalizedScore * barWidth;
        Rect markerRect = new Rect(markerX - 2f, y - 2f, 4f, barHeight + 4f);
        Widgets.DrawBoxSolid(markerRect, Color.white);

        // Tooltips on segments
        prevX = 0f;
        for (int i = 0; i < StageThresholds.Length; i++)
        {
            float segEnd = StageThresholds[i] * barWidth;
            Rect segRect = new Rect(barX + prevX, y, segEnd - prevX, barHeight);
            if (Mouse.IsOver(segRect))
            {
                string moodVal = StageMoods[i] >= 0 ? "+" + StageMoods[i] : StageMoods[i].ToString();
                string stageLabel = StageLabels[i].Translate();
                TooltipHandler.TipRegion(segRect, stageLabel + " (" + moodVal + ")");
            }
            prevX = segEnd;
        }

        y += barHeight + 4f;

        // Current stage info
        int currentStage = GetCurrentStage(normalizedScore);
        string stageName = StageLabels[currentStage].Translate();
        int nextScore = GetScoreForNextStage(currentStage, score);

        Text.Anchor = TextAnchor.MiddleCenter;
        GUI.color = StageColors[currentStage];
        string infoStr = stageName + " (" + StageMoods[currentStage] + ")";
        if (nextScore > 0 && currentStage < StageMoods.Length - 1)
            infoStr += "  →  " + "QP_NextLevel".Translate(nextScore);
        Widgets.Label(new Rect(0f, y, inRect.width, 20f), infoStr);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
        y += 24f;

        return y;
    }

    private int GetCurrentStage(float normalizedScore)
    {
        for (int i = 0; i < StageThresholds.Length; i++)
        {
            if (i == 4 && normalizedScore >= 0.50f) continue; // 0.5 exact = neutral
            if (normalizedScore <= StageThresholds[i]) return i;
        }
        return StageThresholds.Length - 1;
    }

    private int GetScoreForNextStage(int currentStage, int score)
    {
        if (currentStage >= StageMoods.Length - 1) return 0;
        float nextThreshold = StageThresholds[currentStage];
        int nextScore = (int)(nextThreshold * 60f - 30f) - score + 1; // 60 = range, 30 = offset
        return Mathf.Max(0, nextScore);
    }
}
