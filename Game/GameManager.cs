using UnityEngine;
using Util;

public abstract class GameManager : MonoBehaviour, IMCEventListener
{
    private GameMode gameMode;

    public GameMode GameMode
    {
        get { return gameMode; }
        set { gameMode = value; }
    }

    void OnEnable()
    {
        Common.LogD("GameManager OnEnable");
        GameManagers.Instance.CurrentManager = this;

        StartGame();
    }

    void OnDisable()
    {
        Common.LogD("GameManager OnDisable");
        GameManagers.Instance.CurrentManager = null;

        EndGame();
    }

    void Update()
    {
        PvpProxy.Instance.Update();
    }

    protected void NextMode()
    {
        if (gameMode != null && HasNextMode())
        {
            gameMode.NextStep();
        }
        else
        {
            AllModeFinished();
        }
    }

    protected void CurrentMode()
    {
        gameMode.CurrentStep();
    }

    protected virtual bool HasNextMode()
    {
        return gameMode.HasNextStep();
    }

    protected void RunMode()
    {
        gameMode.RunStep();
    }

    public virtual void OnEvent(MCEvent evt)
    {
        switch (evt.Type)
        {
            case MCEventType.MODE_STEP_OVER:
                OnEventModeFinished(null);
                break;
        }
    }

    protected virtual void StartGame()
    {
        MCEventCenter.instance.registerMCEventListener(MCEventType.MODE_STEP_OVER, this);
    }

    protected virtual void EndGame()
    {
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.MODE_STEP_OVER, this);
    }

    protected virtual void AllModeFinished() { }

    protected abstract void OnEventModeFinished(object args);
}
