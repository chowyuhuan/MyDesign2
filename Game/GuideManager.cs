using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GuideManager : GameManager
{
    private delegate void GuideAction();

    private List<GuideAction> guideStepList;

    private int step;

    public override void OnEvent(MCEvent evt)
    {
        switch (evt.Type)
        {
            case MCEventType.UI_GAME_GUIDE_START:
                FirstMode();
                break;
            case MCEventType.GUIDE_NEXT_STEP:
                NextGuideStep();
                break;
            default:
                base.OnEvent(evt);
                break;
        }
    }

    protected override void StartGame()
    {
        base.StartGame();
        
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_GAME_GUIDE_START, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.GUIDE_NEXT_STEP, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.GUIDE_FINISHED, this);

        InitGuideStep();
    }

    protected override void EndGame()
    {
        base.EndGame();

        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_GAME_GUIDE_START, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.GUIDE_NEXT_STEP, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.GUIDE_FINISHED, this);

        guideStepList.Clear();
    }

    protected override void OnEventModeFinished(object args)
    {
        NextMode();
    }

    private void InitGuideStep()
    {
        guideStepList = new List<GuideAction>();
        guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.Hello));
        guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.Gestures));
        guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.GuesturesEnd));
        guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.Aim));
        guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.Shoot));
        guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.HurtWordyRussell_3));
        guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.FreeShoot));
        guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.WordyRussell_4));
        guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.WordyRussell_5));
        guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.WordyRussell_6));
        //guideStepList.Add(() => Guide(BattleCommon.GameGuideStep.WordyRussell_7));
        guideStepList.Add(GuideFinish);
        step = DataCenter.savedata.GuideStepIndex;
    }

    private void NextGuideStep()
    {
        if (++step < guideStepList.Count)
        {
            if (step >= 7)
            {
                DataCenter.savedata.GuideStepIndex = step;
                DataCenter.Save();
            }
            guideStepList[step]();
        }
        else
        {
            NextMode();
        }
    }

    void Guide(BattleCommon.GameGuideStep step)
    {
        BattleManager.Instance.SetGuideStep(step);
    }

    /*
    private void GuideGuestures()
    {
        BattleManager.Instance.SetGuideStep(BattleCommon.GameGuideStep.Gestures);
    }

    private void GuideSummery()
    {
        BattleManager.Instance.SetGuideStep(BattleCommon.GameGuideStep.Summery);
    }

    private void GuideAim()
    {
        BattleManager.Instance.SetGuideStep(BattleCommon.GameGuideStep.Aim);
    }

    private void GuideFrontSight()
    {
        BattleManager.Instance.SetGuideStep(BattleCommon.GameGuideStep.FrontSight);
    }

    private void GuideShoot()
    {
        BattleManager.Instance.SetGuideStep(BattleCommon.GameGuideStep.Shoot);
    }

    private void GuideScore()
    {
        BattleManager.Instance.SetGuideStep(BattleCommon.GameGuideStep.Score);
    }

    private void GuideFreeShoot()
    {
        BattleManager.Instance.SetGuideStep(BattleCommon.GameGuideStep.FreeShoot);
    }
     * */

    private void GuideFinish()
    {
        MCEventCenter.instance.dispatchMCEvent(new MCEvent(MCEventType.GUIDE_PRACTICE_MODE_FINISHED));
    }

    void FirstMode()
    {
        this.NextMode();
        guideStepList[step]();
    }
}
