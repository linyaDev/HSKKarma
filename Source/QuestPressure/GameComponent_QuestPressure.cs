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
    TinyPenalty        // -1 (e.g. refused travelers) — same weight as MinorPenalty but separate type
}

public struct QuestRecord : IExposable
{
    public int tick;
    public string questName;
    public int questId;
    public QuestRecordType type;

    public void ExposeData()
    {
        Scribe_Values.Look(ref tick, "tick");
        Scribe_Values.Look(ref questName, "questName");
        Scribe_Values.Look(ref questId, "questId", -1);
        Scribe_Values.Look(ref type, "type");
    }
}

public class GameComponent_QuestPressure : GameComponent
{
    private List<QuestRecord> records = new List<QuestRecord>();
    private const int YearTicks = 3600000;
    private int cleanupCounter;
    private int lastGuestsRefused;

    public GameComponent_QuestPressure(Game game) : base()
    {
    }

    public const int CompletedWeight = 3;
    public const int ExpiredWeight = -2;
    public const int MinorPenaltyWeight = -1;
    public const int MajorPenaltyWeight = -4;
    public const int MinorBonusWeight = 2;
    public const int CharityCompletedWeight = 5;
    public const int CharityExpiredWeight = -6;
    public const int TinyBonusWeight = 1;
    public const int TinyPenaltyWeight = -1;

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

    public int GetPoints(QuestRecord r)
    {
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
            default: return 0;
        }
    }

    public void RecordQuest(string questName, int questId, QuestRecordType type, bool showMote = true)
    {
        records.Add(new QuestRecord
        {
            tick = Find.TickManager.TicksGame,
            questName = questName,
            questId = questId,
            type = type
        });

        if (showMote)
        {
            var last = records[records.Count - 1];
            ShowMoteOverLeader(GetPoints(last));
        }
    }

    private void ShowMoteOverLeader(int points)
    {
        var leader = FindLeader();
        if (leader == null || !leader.Spawned)
            return;

        string text = (points > 0 ? "+" : "") + points + " " + "QP_MoteMercy".Translate();
        Color color = points > 0
            ? new Color(0.4f, 0.95f, 0.4f)
            : new Color(0.95f, 0.4f, 0.4f);
        MoteMaker.ThrowText(leader.DrawPos, leader.Map, text, color);
    }

    private Pawn FindLeader()
    {
        foreach (var p in PawnsFinder.AllMaps_FreeColonists)
        {
            if (p.Spawned)
                return p;
        }
        return null;
    }

    public override void GameComponentTick()
    {
        // Check ABE guest refusals every game hour
        if (cleanupCounter % 2500 == 0)
            CheckGuestRefusals();

        cleanupCounter++;
        if (cleanupCounter < 60000)
            return;
        cleanupCounter = 0;

        int cutoff = Find.TickManager.TicksGame - YearTicks;
        records.RemoveAll(r => r.tick < cutoff);
    }

    private System.Reflection.FieldInfo cachedRefusalField;
    private GameComponent cachedRefusalTracker;
    private bool refusalLookupDone;

    private void CheckGuestRefusals()
    {
        if (!refusalLookupDone)
        {
            refusalLookupDone = true;
            foreach (var comp in Current.Game.components)
            {
                if (comp.GetType().FullName == "AskBeforeEnter.GameComponent_RefusalTracker")
                {
                    cachedRefusalTracker = comp;
                    cachedRefusalField = comp.GetType().GetField("guestsRefused");
                    break;
                }
            }
        }

        if (cachedRefusalTracker == null || cachedRefusalField == null)
            return;

        int current = (int)cachedRefusalField.GetValue(cachedRefusalTracker);

        try { System.IO.File.AppendAllText(@"D:\Mods\karma_debug.txt",
            $"REFUSAL CHECK: current={current}, last={lastGuestsRefused}\n"); } catch { }

        if (lastGuestsRefused == 0)
        {
            lastGuestsRefused = current;
            return;
        }

        if (current > lastGuestsRefused)
        {
            int diff = current - lastGuestsRefused;
            for (int i = 0; i < diff; i++)
                RecordQuest("QP_GuestsRefused".Translate(), 0, QuestRecordType.TinyPenalty);
        }
        lastGuestsRefused = current;
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref records, "records", LookMode.Deep);
        Scribe_Values.Look(ref lastGuestsRefused, "lastGuestsRefused");
        if (records == null)
            records = new List<QuestRecord>();
    }
}
