
namespace BALL
{
    public enum BallType
    {
        Green,
        Red,

        Max,
    }

    public enum BallNumberRange
    {
        Min = 0,
        Max = 9,
    }

    public class BallCommon
    {
        public static int RoundNumber(int number)
        {
            if (number > (int)BallNumberRange.Max)
            {
                return (int)BallNumberRange.Min;
            }
            if (number < (int)BallNumberRange.Min)
            {
                return (int)BallNumberRange.Max;
            }
            return number;
        }
    }
}
