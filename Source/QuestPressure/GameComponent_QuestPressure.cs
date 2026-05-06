using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace KarmaHSK;

public enum QuestRecordType : byte
{
    Completed,         // +3
    Expired,           // -2
    MinorPenalty,      // -1 (e.g. wounding wild man)
    MajorPenalty,      // -4 (e.g. killing wild man)
    MinorBonus,        // +2 (e.g. wild man spotted)
    CharityCompleted,  // +5 (charity quest)
    CharityExpired,    // -6 (charity quest expired)
    TinyBonus,         // +1 (e.g. allowed travelers)
    TinyPenalty,       // -1 (e.g. refused travelers)
    Failed,            // -3 (quest actively failed)
    ShopPurchase,      // -5 (bought resources)
    ColonistKilled     // -10 (colonist killed by own colonist)
}

public struct QuestRecord : IExposable
{
    public int tick;
    public string questName;
    public int questId;
    public QuestRecordType type;
    public int customWeight;

    public void ExposeData()
    {
        Scribe_Values.Look(ref tick, "tick");
        Scribe_Values.Look(ref questName, "questName");
        Scribe_Values.Look(ref questId, "questId", -1);
        Scribe_Values.Look(ref type, "type");
        Scribe_Values.Look(ref customWeight, "customWeight");
    }
}

public class GameComponent_QuestPressure : GameComponent
{
    private List<QuestRecord> records = new List<QuestRecord>();
    private const int YearTicks = 3600000;
    public const float ScoreMax = 40f;
    private int cleanupCounter;

    public GameComponent_QuestPressure(Game game) : base()
    {
    }

    public const int CompletedWeight = 2;
    public const int ExpiredWeight = -3;
    public const int FailedWeight = -4;
    public const int MinorPenaltyWeight = -1;
    public const int MajorPenaltyWeight = -4;
    public const int MinorBonusWeight = 1;
    public const int CharityCompletedWeight = 4;
    public const int CharityExpiredWeight = -6;
    public const int TinyBonusWeight = 1;
    public const int TinyPenaltyWeight = -1;
    public const int ShopPurchaseWeight = -5;
    public const int ColonistKilledWeight = -5;

    public int Score
    {
        get
        {
            int score = 0;
            foreach (var r in records)
                score += GetPoints(r);
            return score;
        }
    }

    public List<QuestRecord> Records => records;

    // Debug event log (not saved, session-only)
    public static List<string> DebugLog = new List<string>();

    public static void LogDebug(string msg)
    {
        DebugLog.Add($"[{GenTicks.TicksGame}] {msg}");
        if (DebugLog.Count > 50)
            DebugLog.RemoveAt(0);
    }

    public int GetPoints(QuestRecord r)
    {
        if (r.customWeight != 0)
            return r.customWeight;
        switch (r.type)
        {
            case QuestRecordType.Completed: return CompletedWeight;
            case QuestRecordType.Expired: return ExpiredWeight;
            case QuestRecordType.MinorPenalty: return MinorPenaltyWeight;
            case QuestRecordType.MajorPenalty: return MajorPenaltyWeight;
            case QuestRecordType.MinorBonus: return MinorBonusWeight;
            case QuestRecordType.CharityCompleted: return CharityCompletedWeight;
            case QuestRecordType.CharityExpired: return CharityExpiredWeight;
            case QuestRecordType.TinyBonus: return TinyBonusWeight;
            case QuestRecordType.TinyPenalty: return TinyPenaltyWeight;
            case QuestRecordType.Failed: return FailedWeight;
            case QuestRecordType.ShopPurchase: return ShopPurchaseWeight;
            case QuestRecordType.ColonistKilled: return ColonistKilledWeight;
            default: return 0;
        }
    }

    public void RecordQuest(string questName, int questId, QuestRecordType type, bool showMote = true, int customWeight = 0)
    {
        records.Add(new QuestRecord
        {
            tick = Find.TickManager.TicksGame,
            questName = questName,
            questId = questId,
            type = type,
            customWeight = customWeight
        });

        if (showMote)
        {
            var last = records[records.Count - 1];
            ShowMoteOverLeader(GetPoints(last));
        }
    }

    private void ShowMoteOverLeader(int points)
    {
        string text = (points > 0 ? "+" : "") + points + " " + "QP_MoteMercy".Translate();
        Color color = points > 0
            ? new Color(0.4f, 0.95f, 0.4f)
            : new Color(0.95f, 0.4f, 0.4f);

        foreach (var p in PawnsFinder.AllMaps_FreeColonists)
        {
            if (p.Spawned)
                MoteMaker.ThrowText(p.DrawPos, p.Map, text, color);
        }
    }

    public override void GameComponentTick()
    {
        cleanupCounter++;
        if (cleanupCounter < 60000)
            return;
        cleanupCounter = 0;

        int cutoff = Find.TickManager.TicksGame - YearTicks;
        records.RemoveAll(r => r.tick < cutoff);
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref records, "records", LookMode.Deep);
        if (records == null)
            records = new List<QuestRecord>();
    }
}
