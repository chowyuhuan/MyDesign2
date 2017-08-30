using System.Collections.Generic;
using System;
using UnityEngine;
using Network;

public class PvpProxy : IPvpRequest
{
    #region Receive events

    public delegate void PvpResponse(object args);

    // 比赛开始，常规比赛判定击靶还是观战
    public PvpResponse PvpGameBegin { get; set; }
    // 观战方开始展示
    public PvpResponse PvpShowBegin { get; set; }
    // 观战方结束展示
    public PvpResponse PvpShowOver { get; set; }
    // 同步对手分数
    public PvpResponse PvpRivalScore { get; set; }
    // 比赛结束，获得比赛结果
    public PvpResponse PvpGameOver { get; set; }
    // 比赛中断，断线
    public PvpResponse PvpDisconect { get; set; }

    #endregion

    #region Send events.

    public enum PvpRequestType
    {
        // 击靶开始
        START_SHOOT,
        // 击靶结束
        STOP_SHOOT,
        // 同步分数
        SCORE,
        // 同步翻倍碟
        DOUBLE_SAUCER,
    }

    #endregion

    private static PvpProxy instance = null;

    public static PvpProxy Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PvpProxy();
                instance.Init();
            }
            return instance;
        }
    }

    private PbCommon.EGameType gameType;

    private BattleCommon.MatchType matchType;

    private BattleCommon.RoundInfo roundInfo;

    private IPvpRequest ipvpRequest;

    private float messageTimer;

    private const float targetActionDelay = 0.8f;

    private float targetActionDelayTimer;

    private Queue<object> messageCache;

    public delegate bool MessageCallback(object args);
    private Dictionary<Type, MessageCallback> executeImmediate;
    private Dictionary<Type, MessageCallback> executeFromCache;

    protected PvpProxy() 
    {
        roundInfo = new BattleCommon.RoundInfo();
        messageCache = new Queue<object>();
    }

    protected void Init()
    {
        executeImmediate = new Dictionary<Type, MessageCallback>();
        executeImmediate.Add(typeof(gateproto.NotifyGameEnd), GameEnd);

        executeFromCache = new Dictionary<Type, MessageCallback>();
        executeFromCache.Add(typeof(gateproto.NtfReadyGame), ReadyGame);
        executeFromCache.Add(typeof(gateproto.NtfClientReady), GameBegin);
        executeFromCache.Add(typeof(gateproto.MatchTimeout), GameBeginWithRobot);
        executeFromCache.Add(typeof(gateproto.TargetActionNotify), TargetActionTimeDelay);
    }

    public void RegsiterNetworkCallBack()
    {
        NetworkManager.RegisterHandler((uint)gateproto.command.NTF_READY_GAME, OnNtfMessage);
        NetworkManager.RegisterHandler((uint)gateproto.command.NTF_CLIENT_READY, OnNtfMessage);
        NetworkManager.RegisterHandler((uint)gateproto.command.NTF_TARGET_ACTION, OnNtfMessage);
        NetworkManager.RegisterHandler((uint)gateproto.command.NOTIFY_GAME_END, OnNtfMessage);
    }

    public void UnregsiterNetworkCallBack()
    {
        NetworkManager.UnregisterHandler((uint)gateproto.command.NTF_READY_GAME, OnNtfMessage);
        NetworkManager.UnregisterHandler((uint)gateproto.command.NTF_CLIENT_READY, OnNtfMessage);
        NetworkManager.UnregisterHandler((uint)gateproto.command.NTF_TARGET_ACTION, OnNtfMessage);
        NetworkManager.UnregisterHandler((uint)gateproto.command.NOTIFY_GAME_END, OnNtfMessage);
    }

    public void Update()
    {
        messageTimer += Time.deltaTime;

        RunMessageFromCache();
    }

    private void ResetMessageTimer()
    {
        messageTimer = 0;
    }

    public void PvpRequest(PvpRequestType type, object args)
    {
        Log("Request:" + type.ToString());

        ipvpRequest.Request(type, args);
    }

    public void Request(PvpRequestType type, object args)
    {
        gateproto.TargetActionReq request = new gateproto.TargetActionReq();
        request.action_param = new PbCommon.TargetActionParam();

        switch (type)
        {
            case PvpRequestType.START_SHOOT:
                request.action_param.action_type = (uint)PbCommon.ETargetActionType.E_Target_Begin;
                ResetMessageTimer();
                break;
            case PvpRequestType.STOP_SHOOT:
                request.action_param.action_type = (uint)PbCommon.ETargetActionType.E_Target_Finish;
                break;
            case PvpRequestType.SCORE:
                BattleCommon.ScoreInfo scoreInfo = args as BattleCommon.ScoreInfo;
                request.action_param.action_type = (uint)PbCommon.ETargetActionType.E_Target_Score;
                request.action_param.saucer_type = (uint)scoreInfo.saucerType;
                request.action_param.score = (uint)scoreInfo.score;
                request.action_param.saucer_id = (uint)scoreInfo.saucerId;
                request.action_param.ray_count = (uint)scoreInfo.hitRays;
                break;
            case PvpRequestType.DOUBLE_SAUCER:
                request.action_param.action_type = (uint)PbCommon.ETargetActionType.E_Target_DoubleSaucer;
                break;
        }

        request.action_param.action_param = messageTimer;

        request.session_id = DataCenter.sessionId;
        NetworkManager.SendRequest(ProtocolDataType.TcpPersistent, request);

        Log("Request:" + messageTimer.ToString() + " " + type.ToString());
    }

    public object Response(PvpRequestType type, object args)
    {
        gateproto.TargetActionNotify response = new gateproto.TargetActionNotify();
        response.action_param = new PbCommon.TargetActionParam();

        switch (type)
        {
            case PvpRequestType.START_SHOOT:
                response.action_param.action_type = (uint)PbCommon.ETargetActionType.E_Target_Begin;
                ResetMessageTimer();
                break;
            case PvpRequestType.STOP_SHOOT:
                response.action_param.action_type = (uint)PbCommon.ETargetActionType.E_Target_Finish;
                break;
            case PvpRequestType.SCORE:
                BattleCommon.ScoreInfo scoreInfo = args as BattleCommon.ScoreInfo;
                response.action_param.action_type = (uint)PbCommon.ETargetActionType.E_Target_Score;
                response.action_param.saucer_type = (uint)scoreInfo.saucerType;
                response.action_param.score = (uint)scoreInfo.score;
                response.action_param.saucer_id = (uint)scoreInfo.saucerId;
                response.action_param.ray_count = (uint)scoreInfo.hitRays;
                break;
        }

        response.action_param.action_param = messageTimer;

        return response;
    }

    public void OnNtfMessage(ushort result, object response, object request)
    {
        if (result == 0)
        {
            MessageCallback callback;

            if (executeImmediate.TryGetValue(response.GetType(), out callback))
            {
                callback(response);
            }

            if (executeFromCache.TryGetValue(response.GetType(), out callback))
            {
                Log("Response:" + response.GetType().ToString() + " time:" + Time.time);
                messageCache.Enqueue(response); 
            }
        }
    }

    private void RunMessageFromCache()
    {
        targetActionDelayTimer += Time.deltaTime;

        if (messageCache.Count > 0)
        {
            object response = messageCache.Peek();

            MessageCallback callback;

            if (executeFromCache.TryGetValue(response.GetType(), out callback))
            {
                if (callback(response))
                {
                    messageCache.Dequeue();
                }
            }
        }
    }

    public void ClearMessageCache()
    {
        Log("Clear message cache");
        messageCache.Clear();
    }

    private bool GameBegin(object response)
    {
        Log("Excute:GameBegin");
        PvpGameBegin(roundInfo);
        ipvpRequest = this;
        return true;
    }

    private bool GameBeginWithRobot(object response)
    {
        Log("Excute:GameBeginWithRobot");

        gateproto.MatchTimeout msg = response as gateproto.MatchTimeout;

        switch ((PbCommon.EGameType)msg.game_type)
        {
            case PbCommon.EGameType.E_Game_Regular_2P:
                PvpRobot.Instance.Init(BattleCommon.MatchType.People2Normal);
                break;
            case PbCommon.EGameType.E_Game_Wild_2P:
                PvpRobot.Instance.Init(BattleCommon.MatchType.People2Wild);
                break;
            case PbCommon.EGameType.E_Game_Regular_8P:
                PvpRobot.Instance.Init(BattleCommon.MatchType.People2Normal);
                break;
            case PbCommon.EGameType.E_Game_Wild_8P:
                PvpRobot.Instance.Init(BattleCommon.MatchType.People2Wild);
                break;
            default:
                return false;
        }

        InitRobotAI(AIType.AI2, msg);
        PvpRobot.Instance.StartRun();

        ipvpRequest = PvpRobot.Instance;
        return true;
    }

    private void InitRobotAI(AIType aiType, gateproto.MatchTimeout msg)
    {
        switch (aiType)
        {
            case AIType.AI1:
                PvpRobot.Instance.InitAI(AIType.AI1, (int)msg.score_low, (int)msg.score_high);
                break;
            case AIType.AI2:
                switch ((PbCommon.EGameType)msg.game_type)
                {
                    case PbCommon.EGameType.E_Game_Regular_2P:
                    case PbCommon.EGameType.E_Game_Wild_2P:
                        PvpRobot.Instance.InitAI(AIType.AI2_1V1);
                        break;
                    case PbCommon.EGameType.E_Game_Regular_8P:
                    case PbCommon.EGameType.E_Game_Wild_8P:
                        PvpRobot.Instance.InitAI(AIType.AI2_CUP, DataCenter.CupMatch.GetPromotionStage());
                        break;
                }
                break;
        }
    }

    private bool TargetActionTimeDelay(object response)
    {
        if (targetActionDelayTimer >= targetActionDelay)
        {
            gateproto.TargetActionNotify msg = response as gateproto.TargetActionNotify;
            PbCommon.TargetActionParam param = msg.action_param;

            if (param.action_param <= 0.01)
            {
                param.action_param = 0.1f;
                targetActionDelayTimer = 0;
            }

            if (param.action_param <= targetActionDelayTimer - targetActionDelay)
            {
                Log("Excute:TargetAction " + param.action_param + " / " + Time.time + " " + ((PbCommon.ETargetActionType)param.action_type).ToString());
                TargetAction(response);
                return true;
            }
        }
        return false;
    }

    private void TargetAction(object response)
    {
        gateproto.TargetActionNotify msg = response as gateproto.TargetActionNotify;
        PbCommon.TargetActionParam param = msg.action_param;

        switch ((PbCommon.ETargetActionType)param.action_type)
        {
            case PbCommon.ETargetActionType.E_Target_Begin:
                PvpShowBegin(null);
                break;
            case PbCommon.ETargetActionType.E_Target_Finish:
                PvpShowOver(null);
                break;
            case PbCommon.ETargetActionType.E_Target_Score:
                BattleCommon.ScoreInfo scoreInfo = new BattleCommon.ScoreInfo();
                scoreInfo.saucerType = (BattleCommon.SaucerType)param.saucer_type;
                scoreInfo.saucerId = (int)param.saucer_id;
                scoreInfo.score = (int)param.score;
                scoreInfo.hitRays = (int)param.ray_count;
                PvpRivalScore(scoreInfo);
                break;
        }
    }

    private bool ReadyGame(object response)
    {
        Log("Excute:GameReady");
        SetMatchRoundInfo(response);
        RequestClientReady();
        return true;
    }

    private bool GameEnd(object response)
    {
        Log("Excute:GameEnd");
        PvpGameOver(null);
        return true;
    }

    public void SetMatchRoundInfo(object response)
    {
        gateproto.NtfReadyGame msg = response as gateproto.NtfReadyGame;
        roundInfo.actionType = msg.is_shooter ? BattleCommon.ActionType.Shooter : BattleCommon.ActionType.Watcher;
        roundInfo.round = (int)msg.round_count;
        roundInfo.isOvertime = PvpRobot.IsOverTime(matchType, roundInfo.round);
    }

    public void RequestClientReady()
    {
        gateproto.ClientReadyReq request = new gateproto.ClientReadyReq();
        request.session_id = DataCenter.sessionId;
        NetworkManager.SendRequest(ProtocolDataType.TcpPersistent, request);
    }

    public void RequestMacthResultWithRobot(bool playerWin, int playerScore, float time, int doubleSaucer)
    {
        gateproto.ResultReq request = new gateproto.ResultReq();
        request.session_id = DataCenter.sessionId;
        request.is_win = playerWin;
        request.score = (uint)playerScore;
        request.room_type = (uint)BattleTempData.Instance.RoomType;
        request.double_saucer_count = (uint)doubleSaucer;
        request.is_finish = (uint)DealWithSendMatchTime(playerWin, time, out time);
        request.room_time = (uint)time;
        BattleItemHelper.PushItems(request.consume_item_list);

        NetworkManager.SendRequest(ProtocolDataType.TcpPersistent, request);
    }

    private int DealWithSendMatchTime(bool playerWin, float time, out float resultTime)
    {
        switch (gameType)
        {
            case PbCommon.EGameType.E_Game_Regular_8P:
            case PbCommon.EGameType.E_Game_Wild_8P:
                DataCenter.CupMatch.MatchTimeTotal += time;
                resultTime = DataCenter.CupMatch.MatchTimeTotal;
                return (!playerWin || DataCenter.CupMatch.GetPromotionStage() == CupMatch.EPromotionStage.Promotion2) ? 1 : 0;
            default:
                resultTime = time;
                return 1;
        }
    }

    public BattleCommon.MatchType SetMatchType(PbCommon.EGameType gameType)
    {
        this.gameType = gameType;

        switch (gameType)
        {
            case PbCommon.EGameType.E_Game_Regular_2P:
            case PbCommon.EGameType.E_Game_Regular_8P:
            case PbCommon.EGameType.E_Game_Friend_Normal:
                matchType = BattleCommon.MatchType.People2Normal;
                break;
            case PbCommon.EGameType.E_Game_Wild_2P:
            case PbCommon.EGameType.E_Game_Wild_8P:
            case PbCommon.EGameType.E_Game_Friend_Wild:
                matchType = BattleCommon.MatchType.People2Wild;
                break;
            default:
                Common.LogE("Invalid Match Type: " + gameType);
                matchType = BattleCommon.MatchType.People2Normal;
                break;
        }

        return matchType;
    }

    private void Log(string message, string color = "cyan")
    {
        string format = "<color=" + color + ">" + "PvpBehavior: " + " " + message + "</color>";
        Common.LogD(format);
    }
}
