using Verse;

namespace KarmaHSK;

public class QuestPressureSettings : ModSettings
{
    public float threatMultiplier = 0.7f;
    public float rewardMultiplier = 1.0f;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref threatMultiplier, "threatMultiplier", 0.7f);
        Scribe_Values.Look(ref rewardMultiplier, "rewardMultiplier", 1.0f);
        base.ExposeData();
    }
}
