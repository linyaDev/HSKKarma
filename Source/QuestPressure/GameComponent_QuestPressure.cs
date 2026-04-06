using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KarmaHSK;

public enum QuestRecordType : byte
{
    Completed,
    Expired,
    MinorPenalty,  // -1 (e.g. wounding wild man)
    MajorPenalty,  // -4 (e.g. killing wild man)
    MinorBonus     // +2 (e.g. wild man spotted)
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

    public GameComponent_QuestPressure(Game game) : base()
    {
    }

    public const int CompletedWeight = 3;
    public const int ExpiredWeight = -2;
    public const int MinorPenaltyWeight = -1;
    public const int MajorPenaltyWeight = -4;
    public const int MinorBonusWeight = 2;

    public int Score
    {
        get
        {
            int score = 0;
            foreach (var r in records)
            {
                switch (r.type)
                {
                    case QuestRecordType.Completed: score += CompletedWeight; break;
                    case QuestRecordType.Expired: score += ExpiredWeight; break;
                    case QuestRecordType.MinorPenalty: score += MinorPenaltyWeight; break;
                    case QuestRecordType.MajorPenalty: score += MajorPenaltyWeight; break;
                    case QuestRecordType.MinorBonus: score += MinorBonusWeight; break;
                }
            }
            return score;
        }
    }

    public List<QuestRecord> Records => records;

    public void RecordQuest(string questName, int questId, QuestRecordType type)
    {
        records.Add(new QuestRecord
        {
            tick = Find.TickManager.TicksGame,
            questName = questName,
            questId = questId,
            type = type
        });
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
