using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BallInputPanel : ShootInputPanel {

    protected override void Start()
    {
        base.Start();

        PhoneSensitivity = SceneConfig.Instance.PhoneSensitivity;
        AdditionSpeedLimit = SceneConfig.Instance.AdditionSpeedLimit;
        AdditionCoefficient = SceneConfig.Instance.AdditionCoefficient;

        AimCallback = AimRotate;
    }

    public void AimRotate(float axisX, float axisY)
    {
        MCEvent aimEvent = new MCEvent(MCEventType.REQUEST_AIM_ROTATE);
        aimEvent.ObjectValue = new Vector2(axisX, axisY);
        MCEventCenter.instance.dispatchMCEvent(aimEvent);
    }
}
