using RimWorld;
using Verse;

namespace KarmaHSK;

public class ThoughtWorker_Mercy : ThoughtWorker
{
    public override ThoughtState CurrentStateInternal(Pawn p)
    {
        var need = p.needs?.TryGetNeed<Need_Mercy>();
        if (need == null || !need.ShowOnNeedList)
            return ThoughtState.Inactive;

        float level = need.CurLevel;

        if (level <= 0.10f) return ThoughtState.ActiveAtStage(0);  // -20
        if (level <= 0.20f) return ThoughtState.ActiveAtStage(1);  // -15
        if (level <= 0.30f) return ThoughtState.ActiveAtStage(2);  // -10
        if (level <= 0.40f) return ThoughtState.ActiveAtStage(3);  // -5
        if (level < 0.50f) return ThoughtState.ActiveAtStage(4);   // -2
        if (level <= 0.60f) return ThoughtState.ActiveAtStage(5);  // 0
        if (level <= 0.70f) return ThoughtState.ActiveAtStage(6);  // +2
        if (level <= 0.80f) return ThoughtState.ActiveAtStage(7);  // +4
        if (level <= 0.90f) return ThoughtState.ActiveAtStage(8);  // +6
        return ThoughtState.ActiveAtStage(9);                      // +8
    }
}
