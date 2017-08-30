
public class PveManager : GameManager
{
    protected override void StartGame()
    {
        base.StartGame();

        NextMode();
    }

    protected override void AllModeFinished()
    {
    }

    protected override void OnEventModeFinished(object args)
    {
        NextMode();
    }
}
