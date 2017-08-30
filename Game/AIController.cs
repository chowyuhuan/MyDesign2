
public abstract class AIController
{
    protected int[] shootHitDecision;

    protected int shootCounter;

    public int HitDecision()
    {
        if (shootHitDecision != null)
        {
            return shootHitDecision[shootCounter++ % shootHitDecision.Length];
        }
        return 0;
    }
}
