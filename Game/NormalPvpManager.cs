using System.Collections;
using UnityEngine;

public class NormalPvpManager : PvpManager
{
    protected override void SetRoundMode(object args)
    {
        if (currentRoundInfo.round % 2 == 0)
        {
            NextMode();
        }
        else
        {
            CurrentMode();
        }
    }

    protected override void OnPvpEventShowBegin(object args)
    {
        BattleManager.Instance.ShowBegin();
    }

    protected override void OnPvpEventShowOver(object args)
    {
        BattleManager.Instance.ShowOver();
    }
}
