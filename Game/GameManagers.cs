using UnityEngine;
using Util;

public class GameManagers : SingletonMB<GameManagers>
{
    public PveManager pveManager;

    public NormalPvpManager normalPvpManager;

    public WildPvpManager wildPvpManager;

    public GuideManager guideManager;

    private GameManager currentManager;

    public GameManager CurrentManager
    {
        get { return currentManager; }
        set { currentManager = value; }
    }
}