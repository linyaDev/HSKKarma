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

        list.GapLine();

        // === Quests section ===
#if V15
        list.Label("QP_QuestSection".Translate(), -1f, (string)null);
#else
        list.Label("QP_QuestSection".Translate(), -1f, (TipSignal?)null);
#endif
        list.Gap(4f);

        list.CheckboxLabeled("QP_LimitRefugees".Translate(), ref Settings.limitRefugees, "QP_LimitRefugeesTooltip".Translate());
        if (Settings.limitRefugees)
        {
            string maxLabel = "QP_MaxRefugees".Translate() + ": " + Settings.maxRefugees;
#if V15
            list.Label(maxLabel, -1f, (string)null);
#else
            list.Label(maxLabel, -1f, (TipSignal?)null);
#endif
            Settings.maxRefugees = (int)list.Slider(Settings.maxRefugees, 1f, 10f);
        }

        list.Gap(6f);

        list.CheckboxLabeled("QP_NerfWastepacks".Translate(), ref Settings.nerfWastepacks, "QP_NerfWastepacksTooltip".Translate());
        if (Settings.nerfWastepacks)
        {
            string baseLabel = "QP_WastepackBase".Translate() + ": " + (Settings.wastepackBaseMultiplier * 100f).ToString("F0") + "%";
#if V15
            list.Label(baseLabel, -1f, (string)null);
#else
            list.Label(baseLabel, -1f, (TipSignal?)null);
#endif
            Settings.wastepackBaseMultiplier = Mathf.Round(list.Slider(Settings.wastepackBaseMultiplier, 0.1f, 1.0f) * 10f) / 10f;

            string neoLabel = "QP_WastepackNeolithic".Translate() + ": " + (Settings.wastepackNeolithicMult * 100f).ToString("F0") + "%";
#if V15
            list.Label(neoLabel, -1f, (string)null);
#else
            list.Label(neoLabel, -1f, (TipSignal?)null);
#endif
            Settings.wastepackNeolithicMult = Mathf.Round(list.Slider(Settings.wastepackNeolithicMult, 0.1f, 1.0f) * 10f) / 10f;

            string medLabel = "QP_WastepackMedieval".Translate() + ": " + (Settings.wastepackMedievalMult * 100f).ToString("F0") + "%";
#if V15
            list.Label(medLabel, -1f, (string)null);
#else
            list.Label(medLabel, -1f, (TipSignal?)null);
#endif
            Settings.wastepackMedievalMult = Mathf.Round(list.Slider(Settings.wastepackMedievalMult, 0.1f, 1.0f) * 10f) / 10f;
        }

        list.End();
    }

    public override string SettingsCategory()
    {
        return "QP_SettingsCategory".Translate();
    }
}
