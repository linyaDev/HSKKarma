using Verse;

namespace KarmaHSK;

public class QuestPressureSettings : ModSettings
{
    public float threatMultiplier = 0.7f;
    public float rewardMultiplier = 1.0f;
    public bool limitRefugees = true;
    public int maxRefugees = 4;
    public bool nerfWastepacks = true;
    public float wastepackBaseMultiplier = 0.5f;
    public float wastepackNeolithicMult = 0.5f;
    public float wastepackMedievalMult = 0.7f;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref threatMultiplier, "threatMultiplier", 0.7f);
        Scribe_Values.Look(ref rewardMultiplier, "rewardMultiplier", 1.0f);
        Scribe_Values.Look(ref limitRefugees, "limitRefugees", true);
        Scribe_Values.Look(ref maxRefugees, "maxRefugees", 4);
        Scribe_Values.Look(ref nerfWastepacks, "nerfWastepacks", true);
        Scribe_Values.Look(ref wastepackBaseMultiplier, "wastepackBaseMultiplier", 0.5f);
        Scribe_Values.Look(ref wastepackNeolithicMult, "wastepackNeolithicMult", 0.5f);
        Scribe_Values.Look(ref wastepackMedievalMult, "wastepackMedievalMult", 0.7f);
        base.ExposeData();
    }
}
