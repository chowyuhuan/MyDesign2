using UnityEngine;

public class AI1Controller : AIController
{
    private int SCORE_NUMBER_1;
    private int SCORE_NUMBER_2;

    public AI1Controller(int scoreMin, int scoreMax, int saucerCount)
    {
        Init(scoreMin, scoreMax, saucerCount);
    }

    /// <summary>
    /// 初始化AI
    /// </summary>
    /// <param name="scoreMin">最低得分</param>
    /// <param name="scoreMax">最高得分</param>
    /// <param name="saucerCount">飞碟总数</param>
    public void Init(int scoreMin, int scoreMax, int saucerCount)
    {
        SCORE_NUMBER_1 = ScoreConfig.Instance.ScoreNormalHit;
        SCORE_NUMBER_2 = ScoreConfig.Instance.ScorePerfectHit;

        int randomScore = UnityEngine.Random.Range(scoreMin, scoreMax);

        int score2number = UnityEngine.Random.Range(0, randomScore / SCORE_NUMBER_2);

        int score1number = (randomScore - score2number * SCORE_NUMBER_2) / SCORE_NUMBER_1;

        shootHitDecision = new int[saucerCount];

        int i = 0;
        for (; i < score2number && i < shootHitDecision.Length; i++)
        {
            shootHitDecision[i] = SCORE_NUMBER_2; 
        }
        for (; i < score2number + score1number && i < shootHitDecision.Length; i++)
        {
            shootHitDecision[i] = SCORE_NUMBER_1; 
        }

        DisruptOrder();

        shootCounter = 0;
    }

    private void DisruptOrder()
    {
        if (shootHitDecision.Length <= 0)
        {
            return;
        }

        int time = 50;
        for (int i = 0; i < time; i++)
        {
            int rand1 = UnityEngine.Random.Range(0, shootHitDecision.Length - 1);
            int rand2 = UnityEngine.Random.Range(0, shootHitDecision.Length - 1);
            int temp = shootHitDecision[rand1];
            shootHitDecision[rand1] = shootHitDecision[rand2];
            shootHitDecision[rand2] = temp;
        }
    }
}
