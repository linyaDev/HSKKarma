using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace KarmaHSK;

public class Need_Mercy : Need
{
    private const float ScoreMin = -30f;
    private const float ScoreMax = 30f;

    private static Texture2D cachedInfoIcon;
    private static Texture2D InfoIcon => cachedInfoIcon ??= ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton", true);

    public Need_Mercy(Pawn pawn) : base(pawn)
    {
    }

    public override bool ShowOnNeedList => !IsExcluded;

    private bool IsExcluded
    {
        get
        {
            if (pawn.story?.traits?.HasTrait(TraitDefOf.Bloodlust) == true)
                return true;
            if (ModsConfig.AnomalyActive && pawn.health?.hediffSet?.HasHediff(HediffDefOf.Inhumanized) == true)
                return true;
            return false;
        }
    }

    public override int GUIChangeArrow => 0;

    public override void SetInitialLevel()
    {
        CurLevel = 0.5f;
    }

    public override void NeedInterval()
    {
        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        if (comp == null)
            return;

        float score = Mathf.Clamp(comp.Score, ScoreMin, ScoreMax);
        CurLevel = Mathf.InverseLerp(ScoreMin, ScoreMax, score);
    }

    public override string GetTipString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(def.LabelCap);
        sb.AppendLine();
        sb.AppendLine(def.description);

        var comp = Current.Game?.GetComponent<GameComponent_QuestPressure>();
        if (comp != null && comp.Records.Count > 0)
        {
            int completed = 0, expired = 0;
            foreach (var r in comp.Records)
            {
                if (r.type == QuestRecordType.Completed) completed++;
                else expired++;
            }
            sb.AppendLine();
            sb.AppendLine("QP_Stats".Translate(completed, expired, comp.Score));
        }

        return sb.ToString();
    }

    public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue,
        float customMargin = -1f, bool drawArrows = true, bool doTooltip = true,
        Rect? rectForTooltip = null, bool drawLabel = true)
    {
        float margin = customMargin >= 0f ? customMargin : 29f;

        // Draw info button under the bar, same position as Vanilla Food Variety Expanded
        Rect btnRect = new Rect(rect.x + margin, rect.y + rect.height - 10f, 16f, 16f);
        GUI.DrawTexture(btnRect, InfoIcon, ScaleMode.ScaleToFit);
        if (Widgets.ButtonInvisible(btnRect))
        {
            Find.WindowStack.Add(new Dialog_MercyInfo(pawn));
        }

        base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip, drawLabel);
    }

    public override void ExposeData()
    {
        base.ExposeData();
    }
}
