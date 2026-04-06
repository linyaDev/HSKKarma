using UnityEngine;
using Verse;

namespace KarmaHSK;

public class QuestPressureMod : Mod
{
    public static QuestPressureSettings Settings;

    public QuestPressureMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<QuestPressureSettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var list = new Listing_Standard();
        list.ColumnWidth = inRect.width;
        list.Begin(inRect);

        string threatLabel = "QP_ThreatMultiplier".Translate() + ": " + Settings.threatMultiplier.ToString("F2");
        string rewardLabel = "QP_RewardMultiplier".Translate() + ": " + Settings.rewardMultiplier.ToString("F2");

#if V15
        list.Label(threatLabel, -1f, (string)null);
#else
        list.Label(threatLabel, -1f, (TipSignal?)null);
#endif
        Settings.threatMultiplier = list.Slider(Settings.threatMultiplier, 0.1f, 2.0f);

        list.Gap(6f);

#if V15
        list.Label(rewardLabel, -1f, (string)null);
#else
        list.Label(rewardLabel, -1f, (TipSignal?)null);
#endif
        Settings.rewardMultiplier = list.Slider(Settings.rewardMultiplier, 0.1f, 3.0f);

        list.End();
    }

    public override string SettingsCategory()
    {
        return "QP_SettingsCategory".Translate();
    }
}
