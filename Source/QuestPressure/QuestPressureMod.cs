using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace KarmaHSK;

public class QuestPressureMod : Mod
{
    public static QuestPressureSettings Settings;
    private Vector2 scrollPos;

    public QuestPressureMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<QuestPressureSettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Rect scrollArea = new Rect(0f, 0f, inRect.width, inRect.height);
        Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, 800f);
        Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);

        var list = new Listing_Standard();
        list.ColumnWidth = viewRect.width;
        list.Begin(viewRect);

        // === Threat & Reward ===
        SectionHeader(list, "QP_ThreatRewardSection".Translate());

        InputLabeled(list, "QP_ThreatMultiplier".Translate(), ref Settings.threatMultiplier, 0.1f, 2.0f, pct: true);
        InputLabeled(list, "QP_RewardMultiplier".Translate(), ref Settings.rewardMultiplier, 0.1f, 3.0f, pct: true);

        // === Quests ===
        SectionHeader(list, "QP_QuestSection".Translate());

        list.CheckboxLabeled("QP_LimitRefugees".Translate(), ref Settings.limitRefugees, "QP_LimitRefugeesTooltip".Translate());
        if (Settings.limitRefugees)
        {
            InputLabeled(list, "QP_MaxRefugees".Translate(), ref Settings.maxRefugees, 1, 10);
            InputLabeled(list, "QP_MaxHelpers".Translate(), ref Settings.maxHelpers, 1, 6);
        }

        // === Wastepacks ===
        list.Gap(4f);
        list.CheckboxLabeled("QP_NerfWastepacks".Translate(), ref Settings.nerfWastepacks, "QP_NerfWastepacksTooltip".Translate());
        if (Settings.nerfWastepacks)
        {
            InputLabeled(list, "QP_WastepackBase".Translate(), ref Settings.wastepackBaseMultiplier, 0.1f, 1.0f, pct: true);
            InputLabeled(list, "QP_WastepackNeolithic".Translate(), ref Settings.wastepackNeolithicMult, 0.1f, 1.0f, pct: true);
            InputLabeled(list, "QP_WastepackMedieval".Translate(), ref Settings.wastepackMedievalMult, 0.1f, 1.0f, pct: true);
        }

        // === Site Factions ===
        SectionHeader(list, "QP_SiteFactions".Translate());

        list.CheckboxLabeled("QP_SiteFactionAuto".Translate(), ref Settings.siteFactionAutoMode, "QP_SiteFactionAutoTooltip".Translate());

        if (!Settings.siteFactionAutoMode && Current.Game != null)
        {
            list.Gap(4f);
            var factions = Find.FactionManager.AllFactions
                .Where(f => f.def.humanlikeFaction && !f.IsPlayer && !f.temporary && !f.Hidden)
                .OrderBy(f => (int)f.def.techLevel)
                .ThenBy(f => f.Name)
                .ToList();

            TechLevel lastTech = TechLevel.Undefined;
            foreach (var faction in factions)
            {
                if (faction.def.techLevel != lastTech)
                {
                    lastTech = faction.def.techLevel;
                    list.Gap(4f);
                    GUI.color = new Color(1f, 1f, 0.7f);
#if V15
                    list.Label("— " + lastTech.ToStringHuman().CapitalizeFirst() + " —", -1f, (string)null);
#else
                    list.Label("— " + lastTech.ToStringHuman().CapitalizeFirst() + " —", -1f, (TipSignal?)null);
#endif
                    GUI.color = Color.white;
                }

                bool allowed = Settings.allowedSiteFactions.Contains(faction.def.defName);
                string label = "  " + faction.Name
                    + (faction.HostileTo(Faction.OfPlayer) ? "" : " (" + "QP_Ally".Translate() + ")");
                list.CheckboxLabeled(label, ref allowed);
                if (allowed && !Settings.allowedSiteFactions.Contains(faction.def.defName))
                    Settings.allowedSiteFactions.Add(faction.def.defName);
                else if (!allowed)
                    Settings.allowedSiteFactions.Remove(faction.def.defName);
            }
        }
        else if (!Settings.siteFactionAutoMode)
        {
#if V15
            list.Label("QP_SiteFactionNeedGame".Translate(), -1f, (string)null);
#else
            list.Label("QP_SiteFactionNeedGame".Translate(), -1f, (TipSignal?)null);
#endif
        }

        list.End();
        Widgets.EndScrollView();
    }

    private static void SectionHeader(Listing_Standard list, string label)
    {
        list.GapLine();
        GUI.color = new Color(1f, 1f, 0.7f);
#if V15
        list.Label(label, -1f, (string)null);
#else
        list.Label(label, -1f, (TipSignal?)null);
#endif
        GUI.color = Color.white;
        list.Gap(4f);
    }

    private static void InputLabeled(Listing_Standard list, string label, ref float value, float min, float max, bool pct = false)
    {
        Rect row = list.GetRect(24f);
        float labelW = row.width - 60f;
        string suffix = pct ? "%" : "";
        Widgets.Label(new Rect(row.x, row.y, labelW, row.height), label + suffix);

        float displayVal = pct ? value * 100f : value;
        string buf = displayVal.ToString(pct ? "F0" : "F2");
        buf = Widgets.TextField(new Rect(row.xMax - 55f, row.y, 55f, row.height), buf);
        if (float.TryParse(buf, out float parsed))
        {
            float newVal = pct ? parsed / 100f : parsed;
            value = Mathf.Clamp(newVal, min, max);
        }
        list.Gap(2f);
    }

    private static void InputLabeled(Listing_Standard list, string label, ref int value, int min, int max)
    {
        Rect row = list.GetRect(24f);
        float labelW = row.width - 60f;
        Widgets.Label(new Rect(row.x, row.y, labelW, row.height), label);

        string buf = value.ToString();
        buf = Widgets.TextField(new Rect(row.xMax - 55f, row.y, 55f, row.height), buf);
        if (int.TryParse(buf, out int parsed))
            value = Mathf.Clamp(parsed, min, max);
        list.Gap(2f);
    }

    public override string SettingsCategory()
    {
        return "QP_SettingsCategory".Translate();
    }
}
