using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum AI2Level
{
    Easy = 1,
    Normal = 2,
    Hard = 3,
    Max = Hard,
}

// Base on CupMatch.EPromotionStage
public enum CupStep
{
    Preliminary = 1,
    Semifinals = 2,
    finals = 4,
}

public enum MatchResult
{
    WinningStreak,
    LosingStreak,
    Other,
}

public class AIRecorder
{
    public List<int> scoreRecord;

    public AIRecorder(int capacity)
    {
        scoreRecord = new List<int>(capacity);
        for(int i = 0; i < scoreRecord.Capacity; i++)
        {
            scoreRecord.Add(0);
        }
    }

    public void RecordScore(int saucerId, int score)
    {
        Common.LogD("Record score saucerId : " + saucerId);
        scoreRecord.Insert(saucerId, score);
    }

    public int GetScoreRecorded(int saucerId)
    {
        if (saucerId >= 0 && saucerId < scoreRecord.Capacity)
        {
            return scoreRecord[saucerId];
        }
        return 0;
    }

    public void SaveRecordScore(BattleCommon.MatchType type)
    {
        List<List<int>> scoreRecordList;

        switch (type)
        {
            case BattleCommon.MatchType.People2Normal:
                scoreRecordList = DataCenter.savedata.normalScoreRecordList;
                break;
            case BattleCommon.MatchType.People2Wild:
                scoreRecordList = DataCenter.savedata.wildScoreRecordList;
                break;
            default:
                return;
        }

        if (scoreRecordList.Count == scoreRecordList.Capacity)
        {
            scoreRecordList.RemoveAt(0);
        }
        scoreRecordList.Add(scoreRecord);
    }
}

public class AIRecordSorter
{
    public List<List<int>> sortedNormalScoreRecordList;
    public List<List<int>> sortedWildScoreRecordList;

    public AIRecordSorter(BattleCommon.MatchType type)
    {
        SortScoreRecordList(type);
    }

    private void SortScoreRecordList(BattleCommon.MatchType type)
    {
        switch (type)
        {
            case BattleCommon.MatchType.People2Normal:
                sortedNormalScoreRecordList = new List<List<int>>(DataCenter.savedata.normalScoreRecordList);
                sortedNormalScoreRecordList.Sort(ComparisonScoreRecord);
                break;
            case BattleCommon.MatchType.People2Wild:
                sortedWildScoreRecordList = new List<List<int>>(DataCenter.savedata.wildScoreRecordList);
                sortedWildScoreRecordList.Sort(ComparisonScoreRecord);
                break;
        }
    }

    private int ComparisonScoreRecord<T>(T x, T y)
        where T : List<int>
    {
        int xSum = x.Sum();
        int ySum = y.Sum();

        if (xSum > ySum)
        {
            return 1;
        }
        else if (xSum < ySum)
        {
            return -1;
        }
        return 0;
    }
}

public interface IAI2Controller
{
    bool NeedSimulatePlayer();

    void RecordScore(int saucerId, int score);

    int GetScoreRecorded(int saucerId);

    void SaveRecordScore(BattleCommon.MatchType matchType);
}

public class AI2Controller : AIController, IAI2Controller
{
    private AIRecorder aiRecorder;

    private AIRecordSorter aiRecordSorter;

    public AI2Controller(int saucerCount)
    {
        aiRecorder = new AIRecorder(saucerCount);
    }

    public AI2Controller(BattleCommon.MatchType type, int saucerCount)
        : this(saucerCount)
    {
        Init(type, DataCenter.savedata.GetMatchResult());
    }

    public AI2Controller(BattleCommon.MatchType type, CupStep cupStep, int saucerCount)
        : this(saucerCount)
    {
        Init(type, cupStep);
    }

    /// <summary>
    /// 初始化AI
    /// </summary>
    /// <param name="type">比赛类型</param>
    /// <param name="matchRecord">比赛记录</param>
    public void Init(BattleCommon.MatchType type, MatchResult matchRecord)
    {
        List<List<int>> recordList = GetRecordList(type);

        shootHitDecision = Get2PeopleMatchAI(recordList, matchRecord);

        shootCounter = 0;
    }

    /// <summary>
    /// 初始化AI
    /// </summary>
    /// <param name="type">比赛类型</param>
    /// <param name="cupStep">杯赛轮次</param>
    public void Init(BattleCommon.MatchType type, CupStep cupStep)
    {
        List<List<int>> recordList = GetRecordList(type);

        shootHitDecision = GetCupMatchAI(recordList, cupStep);

        shootCounter = 0;
    }

    private int GetAI(List<List<int>> list, AI2Level level)
    {
        int segment = list.Count / (int)AI2Level.Max;

        switch (level)
        {
            case AI2Level.Easy:
                return Random.Range(0, segment * (int)AI2Level.Easy);
            case AI2Level.Normal:
                return Random.Range(segment * (int)AI2Level.Easy, segment * (int)AI2Level.Normal);
            case AI2Level.Hard:
                return Random.Range(segment * (int)AI2Level.Normal, list.Count);
        }

        return 0;
    }

    private List<List<int>> GetRecordList(BattleCommon.MatchType type)
    {
        aiRecordSorter = new AIRecordSorter(type);

        switch (type)
        {
            case BattleCommon.MatchType.People2Normal:
                return aiRecordSorter.sortedNormalScoreRecordList;
            case BattleCommon.MatchType.People2Wild:
                return aiRecordSorter.sortedWildScoreRecordList;
            default:
                return null;
        }
    }

    private int[] Get2PeopleMatchAI(List<List<int>> recordList, MatchResult matchRecord)
    {
        int recordId = 0;

        switch(matchRecord)
        {
            case MatchResult.WinningStreak:
                recordId = GetAI(recordList, AI2Level.Hard);
                break;
            case MatchResult.LosingStreak:
                recordId = GetAI(recordList, AI2Level.Easy);
                break;
            case MatchResult.Other:
                recordId = GetAI(recordList, AI2Level.Normal);
                break;
        }

        return recordList.Count == 0 ? null : recordList[recordId].ToArray();
    }

    private int[] GetCupMatchAI(List<List<int>> recordList, CupStep cupStep)
    {
        int recordId = 0;

        switch (cupStep)
        {
            case CupStep.Preliminary:
                recordId = GetAI(recordList, AI2Level.Easy);
                break;
            case CupStep.Semifinals:
                recordId = GetAI(recordList, AI2Level.Normal);
                break;
            case CupStep.finals:
                recordId = GetAI(recordList, AI2Level.Hard);
                break;
        }

        return recordList.Count == 0 ? null : recordList[recordId].ToArray();
    }

    public bool NeedSimulatePlayer()
    {
        return shootHitDecision == null;
    }

    public void RecordScore(int saucerId, int score)
    {
        aiRecorder.RecordScore(saucerId, score);
    }

    public void SaveRecordScore(BattleCommon.MatchType matchType)
    {
        aiRecorder.SaveRecordScore(matchType);
    }

    public int GetScoreRecorded(int saucerId)
    {
        return aiRecorder.GetScoreRecorded(saucerId); 
    }
}
