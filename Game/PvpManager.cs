
public abstract class PvpManager : GameManager
{
    protected BattleCommon.RoundInfo currentRoundInfo;

    protected override bool HasNextMode()
    {
        return true;
    }

    public override void OnEvent(MCEvent evt)
    {
        switch (evt.Type)
        {
            case MCEventType.BATTLE_NETWORK_PULL:
                OnEventStartShoot(null); 
                break;
            case MCEventType.NETWORK_SCORE_UPDATE:
                OnEventScoreUpdate(evt.ObjectValue);
                break;
            case MCEventType.UI_ROUND_ANIMATION_FINISHED:
                OnEventRoundStart(evt.ObjectValue);
                break;
            case MCEventType.MODE_READY:
                OnEventRoundReady(null);
                break;
            case MCEventType.BATTLE_SAUCERMACHINE_EMIT: 
                if (evt.BooleanValue)
                {
                    OnEventDoubleSaucer(null);
                }
                break;
            default:
                base.OnEvent(evt);
                break;
        }
    }

    protected override void OnEventModeFinished(object args)
    {
        PvpProxy.Instance.PvpRequest(PvpProxy.PvpRequestType.STOP_SHOOT, args);
    }

    protected virtual void OnEventStartShoot(object args)
    {
        PvpProxy.Instance.PvpRequest(PvpProxy.PvpRequestType.START_SHOOT, args);
    }

    protected virtual void OnEventScoreUpdate(object args)
    {
        PvpProxy.Instance.PvpRequest(PvpProxy.PvpRequestType.SCORE, args);
    }

    protected virtual void OnEventDoubleSaucer(object args)
    {
        PvpProxy.Instance.PvpRequest(PvpProxy.PvpRequestType.DOUBLE_SAUCER, args);
    }

    protected virtual void OnEventRoundReady(object args)
    {
        var mcEvent = new MCEvent(MCEventType.UI_ROUND_ANIMATION);
        mcEvent.ObjectValue = currentRoundInfo;
        MCEventCenter.instance.dispatchMCEvent(mcEvent);
    }

    protected virtual void OnEventRoundStart(object args)
    {
        RunMode();
    }

    protected virtual void SetRoundMode(object args)
    {
        NextMode();
    }

    protected override void StartGame()
    {
        base.StartGame();

        MCEventCenter.instance.registerMCEventListener(MCEventType.NETWORK_SCORE_UPDATE, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.BATTLE_NETWORK_PULL, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_ROUND_ANIMATION_FINISHED, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.MODE_READY, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.BATTLE_SAUCERMACHINE_EMIT, this);

        RegsiterCallBack();

        PvpProxy.Instance.RegsiterNetworkCallBack();
    }

    protected override void EndGame()
    {
        base.EndGame();

        MCEventCenter.instance.unregisterMCEventListener(MCEventType.NETWORK_SCORE_UPDATE, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.BATTLE_NETWORK_PULL, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_ROUND_ANIMATION_FINISHED, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.MODE_READY, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.BATTLE_SAUCERMACHINE_EMIT, this);

        UnregsiterCallBack();

        PvpProxy.Instance.UnregsiterNetworkCallBack();
        PvpProxy.Instance.ClearMessageCache();
    }

    private void RegsiterCallBack()
    {
        PvpProxy.Instance.PvpGameBegin += OnPvpEventGameBegin;
        PvpProxy.Instance.PvpGameOver += OnPvpEventGameOver;
        PvpProxy.Instance.PvpShowBegin += OnPvpEventShowBegin;
        PvpProxy.Instance.PvpShowOver += OnPvpEventShowOver;
        PvpProxy.Instance.PvpRivalScore += OnPvpEventRivalScore;
        PvpProxy.Instance.PvpDisconect += OnPvpEventDisconect;
    }

    private void UnregsiterCallBack()
    {
        PvpProxy.Instance.PvpGameBegin -= OnPvpEventGameBegin;
        PvpProxy.Instance.PvpGameOver -= OnPvpEventGameOver;
        PvpProxy.Instance.PvpShowBegin -= OnPvpEventShowBegin;
        PvpProxy.Instance.PvpShowOver -= OnPvpEventShowOver;
        PvpProxy.Instance.PvpRivalScore -= OnPvpEventRivalScore;
        PvpProxy.Instance.PvpDisconect -= OnPvpEventDisconect;
    }

    protected virtual void OnPvpEventGameBegin(object args)
    {
        currentRoundInfo = args as BattleCommon.RoundInfo;
        BattleManager.Instance.SetActionType(currentRoundInfo.actionType);
        BattleManager.Instance.SetIsOverTime(currentRoundInfo.isOvertime);

        SetRoundMode(args);
    }

    protected virtual void OnPvpEventGameOver(object args)
    {
        MCEventCenter.instance.dispatchMCEvent(new MCEvent(MCEventType.UI_RESULT_ANIMATION));
    }

    protected virtual void OnPvpEventShowBegin(object args) { }

    protected virtual void OnPvpEventShowOver(object args) { }

    protected virtual void OnPvpEventRivalScore(object args)
    {
        BattleCommon.ScoreInfo scoreInfo = args as BattleCommon.ScoreInfo;

        BattleManager.Instance.RivalScore(scoreInfo);
    }

    protected virtual void OnPvpEventDisconect(object args)
    {
        //BattleManager.Instance.Disconect 
    }
}
