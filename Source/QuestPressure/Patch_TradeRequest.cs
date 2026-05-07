using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace KarmaHSK;

[HarmonyPatch(typeof(QuestNode_TradeRequest_GetRequestedThing), "RunInt")]
public static class Patch_TradeRequest
{
    public static void Postfix()
    {
        var settings = QuestPressureMod.Settings;
        if (settings == null)
            return;

        var slate = QuestGen.slate;
        int original = slate.Get<int>("requestedThingCount");
        int reduced = System.Math.Max(1, (int)(original * settings.rewardMultiplier));
        slate.Set("requestedThingCount", reduced);

        // Recalculate market value
        var thingDef = slate.Get<ThingDef>("requestedThing");
        if (thingDef != null)
            slate.Set("requestedThingMarketValue", thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * reduced);

        string msg = $"Trade request reduced: {original} -> {reduced} (x{settings.rewardMultiplier})";
        Log.Message($"[KarmaHSK] {msg}");
        GameComponent_QuestPressure.LogDebug(msg);
    }
}
