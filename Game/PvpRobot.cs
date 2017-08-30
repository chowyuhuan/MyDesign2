using System.Collections;
using UnityEngine;
using Util;

public enum AIType
{
    AI1,
    AI2,
    AI2_1V1,
    AI2_CUP,
}

public class PvpRobot : SingletonMB<PvpRobot>, IPvpRequest, IMCEventListener
{
    private const int NORMAL_ROUND_COUNT = 4;
    private const int WILD_ROUND_COUNT = 1;
    private const int NORMAL_OVERTIME_ROUND_COUNT = 6;

    private const int PLAYER_FIRST = 0;
    private const int ROBOT_FIRST = 1;

    private const float SCORE_DELAY_MIN = 1.0f;
    private const float SCORE_DELAY_MAX = 2.0f;

    private AIType aiType;

    private AIController aiController;

    private BattleCommon.MatchType matchType;

    private BattleCommon.RoundInfo roundInfo;

    private int roundCount;

    private int roundCounter;

    private int randomFirst;

    private int robotScore;
    private int playerScore;

    private int playerSaucerCounter;
    private int robotSaucerCounter;

    private int doubleSaucerCounter;

    private float timeCounter;

    public PvpRobot()
    {
        roundInfo = new BattleCommon.RoundInfo();
    }

    public void Init(BattleCommon.MatchType type)
    {
        matchType = type;
        roundCount = type == BattleCommon.MatchType.People2Normal ? NORMAL_ROUND_COUNT : WILD_ROUND_COUNT;
        roundCounter = 0;
        randomFirst = PLAYER_FIRST; //UnityEngine.Random.Range(PLAYER_FIRST, ROBOT_FIRST + 1);

        robotScore = 0;
        playerScore = 0;
        playerSaucerCounter = 0;
        robotSaucerCounter = 0;

        doubleSaucerCounter = 0;

        timeCounter = Time.time;
    }

    public void InitAI(AIType type, params object[] args)
    {
        aiType = type;

        switch (aiType)
        {
            case AIType.AI1:
                int scoreMin = (int)args[0];
                int scoreMax = (int)args[1];
                aiController = new AI1Controller(scoreMin, scoreMax, GameManagers.Instance.CurrentManager.GameMode.GetTotalSaucerCount());
                break;
            case AIType.AI2_1V1:
                aiController = new AI2Controller(matchType, GameManagers.Instance.CurrentManager.GameMode.GetTotalSaucerCount());
                break;
            case AIType.AI2_CUP:
                aiController = new AI2Controller(matchType, (CupStep)args[0], GameManagers.Instance.CurrentManager.GameMode.GetTotalSaucerCount());
                break;
        }
    }

    public void StartRun()
    {
        StartCoroutine("AutoRound");
    }

    IEnumerator AutoRound()
    {
        yield return new WaitForSeconds(1);

        do
        {
            if (IsGameOver())
            {
                StartCoroutine("GameOver");
                break;
            }
            else
            {
                StartMatchRound();
            }

            Common.LogD("Robot:NewRound " + matchType.ToString() + " " + roundInfo.round.ToString() + " " + roundInfo.actionType.ToString());

        } while (false);
    }

    private bool IsPlayerTurn()
    {
        return roundCounter % 2 == randomFirst;
    }

    private bool IsRoundFinished()
    {
        return roundCounter >= roundCount;
    }

    private bool IsEvenRound()
    {
        return roundCounter % 2 == 0;
    }

    private bool IsGameOver()
    {
        switch (matchType)
        {
            case BattleCommon.MatchType.People2Normal:
                return IsRoundFinished() && IsEvenRound() && !BattleManager.Instance.IsNormalModeDraw();
            case BattleCommon.MatchType.People2Wild:
                return IsRoundFinished() && robotScore != playerScore;
        }
        return true;
    }

    public static bool IsOverTime(BattleCommon.MatchType type, int round)
    {
        switch (type)
        {
            case BattleCommon.MatchType.People2Normal:
                return round >= NORMAL_OVERTIME_ROUND_COUNT && round % 2 == 0;
            case BattleCommon.MatchType.People2Wild:
                return round >= WILD_ROUND_COUNT;
            default:
                return false;
        }
    }
    private void StartMatchRound()
    {
        switch (matchType)
        {
            case BattleCommon.MatchType.People2Normal:
                roundInfo.actionType = IsPlayerTurn() ? BattleCommon.ActionType.Shooter : BattleCommon.ActionType.Watcher;
                break;
            case BattleCommon.MatchType.People2Wild:
                roundInfo.actionType = BattleCommon.ActionType.Shooter;
                break;
        }

        roundInfo.round = roundCounter;
        roundInfo.isOvertime = IsOverTime(matchType, roundCounter);

        PvpProxy.Instance.PvpGameBegin(roundInfo);

        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_ROUND_ANIMATION_FINISHED, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.BATTLE_SAUCERMACHINE_EMIT, this);
    }

    private void EndMatchRound()
    {
        roundCounter++;

        switch (matchType)
        {
            case BattleCommon.MatchType.People2Normal:
                if (roundInfo.actionType == BattleCommon.ActionType.Watcher)
                {
                    RobotStopShoot();
                }
                break;
            case BattleCommon.MatchType.People2Wild:
                RobotStopShoot();
                break;
        }

        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_ROUND_ANIMATION_FINISHED, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.BATTLE_SAUCERMACHINE_EMIT, this);
    }

    private void StartRobot()
    {
        switch (matchType)
        {
            case BattleCommon.MatchType.People2Normal:
                if (!IsPlayerTurn())
                {
                    StartCoroutine("RobotStartShoot");
                }
                break;
            case BattleCommon.MatchType.People2Wild:
                StartCoroutine("RobotStartShoot");
                break;
        }
    }

    IEnumerator RobotStartShoot()
    {
        yield return new WaitForSeconds(1);

        Common.LogD("Robot:StartShoot");

        MCEventCenter.instance.registerMCEventListener(MCEventType.MODE_STEP_OVER_WATCHER, this);

        PvpProxy.Instance.OnNtfMessage(0, PvpProxy.Instance.Response(PvpProxy.PvpRequestType.START_SHOOT, null), null);
        //PvpProxy.Instance.PvpShowBegin(null);
    }

    private void RobotStopShoot()
    {
        Common.LogD("Robot:StopShoot");

        MCEventCenter.instance.unregisterMCEventListener(MCEventType.MODE_STEP_OVER_WATCHER, this);

        PvpProxy.Instance.OnNtfMessage(0, PvpProxy.Instance.Response(PvpProxy.PvpRequestType.STOP_SHOOT, null), null);
        //PvpProxy.Instance.PvpShowOver(null);
    }

    IEnumerator RobotScore(BattleCommon.ScoreInfo scoreInfo, float hitTime)
    {
        yield return new WaitForSeconds(hitTime);

        Common.LogD("Robot:saucerID " + scoreInfo.saucerId + " score " + scoreInfo.score + " hitRays " + scoreInfo.hitRays + " hitTime " + hitTime);

        PvpProxy.Instance.OnNtfMessage(0, PvpProxy.Instance.Response(PvpProxy.PvpRequestType.SCORE, scoreInfo), null);
        //PvpProxy.Instance.PvpRivalScore(scoreInfo);

        robotScore += scoreInfo.score;
    }

    private void RobotHitDecision(int saucerId, bool isDoubleSaucer)
    {
        if (AI2NeedSimulatePlayer() && matchType == BattleCommon.MatchType.People2Normal)
        {
            AI2SimulatePlayerScore(saucerId);
        }
        else
        {
            BattleCommon.ScoreInfo scoreInfo = new BattleCommon.ScoreInfo();
            scoreInfo.saucerId = saucerId;
            scoreInfo.score = aiController.HitDecision();

            switch (aiType)
            {
                case AIType.AI1:
                    AI1SocreCalculate(scoreInfo, isDoubleSaucer);
                    break;
                case AIType.AI2_1V1:
                case AIType.AI2_CUP:
                    AI2ScoreCalculate(scoreInfo);
                    break;
            }
        }
    }

    private void AI1SocreCalculate(BattleCommon.ScoreInfo scoreInfo, bool isDoubleSaucer)
    {
        float hitTime = UnityEngine.Random.Range(SCORE_DELAY_MIN, SCORE_DELAY_MAX);

        if (scoreInfo.score == 0)
        {
            scoreInfo.hitRays = 0;
        }
        else if (scoreInfo.score == ScoreConfig.Instance.ScoreNormalHit)
        {
            scoreInfo.hitRays = UnityEngine.Random.Range(1, ScoreConfig.Instance.PerfectHitRays - 1);
            scoreInfo.score = ScoreConfig.Instance.CalculateScore(scoreInfo.score, hitTime, isDoubleSaucer);
        }
        else if (scoreInfo.score == ScoreConfig.Instance.ScorePerfectHit)
        {
            scoreInfo.hitRays = ScoreConfig.Instance.PerfectHitRays;
            scoreInfo.score = ScoreConfig.Instance.CalculateScore(scoreInfo.score, hitTime, isDoubleSaucer);
        }

        StartCoroutine(RobotScore(scoreInfo, hitTime));
    }

    private void AI2ScoreCalculate(BattleCommon.ScoreInfo scoreInfo)
    {
        float hitTime = UnityEngine.Random.Range(SCORE_DELAY_MIN, SCORE_DELAY_MAX);

        if (scoreInfo.score == 0)
        {
            scoreInfo.hitRays = 0;
        }
        else if (scoreInfo.score < ScoreConfig.Instance.ScorePerfectHit)
        {
            scoreInfo.hitRays = UnityEngine.Random.Range(1, ScoreConfig.Instance.PerfectHitRays - 1);
        }
        else
        {
            scoreInfo.hitRays = ScoreConfig.Instance.PerfectHitRays;
        }

        StartCoroutine(RobotScore(scoreInfo, hitTime));
    }

    private void AI2SimulatePlayerScore(int saucerId)
    {
        BattleCommon.ScoreInfo scoreInfo = new BattleCommon.ScoreInfo();
        scoreInfo.saucerId = saucerId;
        scoreInfo.score = AI2GetScoreRecorded(CoverntSaucerId(saucerId, playerSaucerCounter)) - (matchType == BattleCommon.MatchType.People2Normal ? Random.Range(1, 6) : Random.Range(1, 4));
        scoreInfo.score = Mathf.Max(0, scoreInfo.score);
        scoreInfo.score = scoreInfo.score == 0 ? 0 : Mathf.Max(60, scoreInfo.score);
        AI2ScoreCalculate(scoreInfo);
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1);

        Common.LogD("Robot:GameOver");

        AI2SaveRecordScore();

        PvpProxy.Instance.OnNtfMessage(0, new gateproto.NotifyGameEnd(), null);
        //PvpProxy.Instance.PvpGameOver(null);
        PvpProxy.Instance.RequestMacthResultWithRobot(BattleTempData.Instance.IsWinner, playerScore, Time.time - timeCounter, doubleSaucerCounter);
    }

    public void QuitGame()
    {
        Common.LogD("Robot:QuitGame");

        EndMatchRound();

        AI2SaveRecordScore();

        PvpProxy.Instance.OnNtfMessage(0, new gateproto.NotifyGameEnd(), null);
        //PvpProxy.Instance.PvpGameOver(null);
        PvpProxy.Instance.RequestMacthResultWithRobot(false, playerScore, Time.time - timeCounter, doubleSaucerCounter);
    }

    void OnDestroy()
    {
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.BATTLE_SAUCERMACHINE_EMIT, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.MODE_STEP_OVER_WATCHER, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_ROUND_ANIMATION_FINISHED, this);
    }

    private void PlayerStartShoot(object request)
    {

    }

    private void PlayerStopShoot(object request)
    {
        EndMatchRound();
        StartCoroutine("AutoRound");
    }

    private void PlayerScore(object request)
    {
        BattleCommon.ScoreInfo scoreInfo = request as BattleCommon.ScoreInfo;
        playerScore += scoreInfo.score;

        if (scoreInfo.score != 0)
        {
            AI2RecordScore(CoverntSaucerId(scoreInfo.saucerId, robotSaucerCounter), scoreInfo.score);
            if (AI2NeedSimulatePlayer() && matchType == BattleCommon.MatchType.People2Wild)
            {
                AI2SimulatePlayerScore(scoreInfo.saucerId);
            }
        }
    }

    private void CountSaucer()
    {
        if (IsPlayerTurn())
        {
            playerSaucerCounter++;
        }
        else
        {
            robotSaucerCounter++;
        }
    }

    private void CountDoubleSaucer(bool isDoubleSaucer)
    {
        if (isDoubleSaucer)
        {
            doubleSaucerCounter++;
        }
    }

    private int CoverntSaucerId(int saucerId, int counter)
    {
        // -1 because saucer id start with 1
        saucerId -= 1;

        switch (matchType)
        {
            case BattleCommon.MatchType.People2Normal:
                return saucerId - counter;
            case BattleCommon.MatchType.People2Wild:
            default:
                return saucerId;
        }
    }

    private bool IsAI2()
    {
        return aiType == AIType.AI2_1V1 || aiType == AIType.AI2_CUP;
    }

    private void AI2RecordScore(int saucerId, int score)
    {
        if (IsAI2())
        {
            ((IAI2Controller)aiController).RecordScore(saucerId, score);
        }
    }

    private int AI2GetScoreRecorded(int saucerId)
    {
        if (IsAI2())
        {
            return ((IAI2Controller)aiController).GetScoreRecorded(saucerId);
        }
        return 0;
    }

    private void AI2SaveRecordScore()
    {
        if (IsAI2())
        {
            ((IAI2Controller)aiController).SaveRecordScore(matchType);
        }
    }

    private bool AI2NeedSimulatePlayer()
    {
        if (IsAI2())
        {
            return ((IAI2Controller)aiController).NeedSimulatePlayer();
        }
        return false;
    }

    public void Request(PvpProxy.PvpRequestType type, object args)
    {
        switch (type)
        {
            case PvpProxy.PvpRequestType.START_SHOOT:
                PlayerStartShoot(args);
                break;
            case PvpProxy.PvpRequestType.STOP_SHOOT:
                PlayerStopShoot(args);
                break;
            case PvpProxy.PvpRequestType.SCORE:
                PlayerScore(args);
                break;
        }
    }

    public void OnEvent(MCEvent evt)
    {
        switch (evt.Type)
        {
            case MCEventType.BATTLE_SAUCERMACHINE_EMIT:
                CountSaucer();
                CountDoubleSaucer(evt.BooleanValue);
                if ((matchType == BattleCommon.MatchType.People2Normal && !IsPlayerTurn()) ||
                     matchType == BattleCommon.MatchType.People2Wild)
                {
                    RobotHitDecision(evt.IntValue, evt.BooleanValue);
                }
                break;
            case MCEventType.MODE_STEP_OVER_WATCHER:
                EndMatchRound();
                StartCoroutine("AutoRound");
                break;
            case MCEventType.UI_ROUND_ANIMATION_FINISHED:
                StartRobot();
                break;
        }
    }
}