using UnityEngine;
using System.Collections.Generic;

namespace BALL
{
    public class BallManager : MonoBehaviour
    {
        public int boxNum;
        public Transform boxPos;
        public Transform boxSpace;
        public GameObject box;
        public Transform boxRoot;

        public GameObject greenBall;
        public GameObject redBall;
        public Transform ballRoot;

        private List<GameObject> boxes;
        // Ball type map
        private Dictionary<BallType, GameObject> ballTypes;
        // Ball pool for each type
        private const int BallPoolSize = 8;
        private List<GameObject>[] ballPool;
        // Record ball active
        private List<GameObject> ballActive;

        public List<GameObject> BallActive
        {
            get { return ballActive; }
        }

        // Generate ball random function recursion depth
        private const int RecursionDepth = 50;
        private int recursionDepthCounter;

        private int ballMaxCount;

        public int BallMaxCount
        {
            get { return ballMaxCount; }
            set { ballMaxCount = value; }
        }

        public int HalfBallMaxCount
        {
            get { return ballMaxCount / 2; }
        }

        private float ballSpeedMin;

        public float BallSpeedMin
        {
            get { return ballSpeedMin; }
            set { ballSpeedMin = value; }
        }

        private float ballSpeedMax;

        public float BallSpeedMax
        {
            get { return ballSpeedMax; }
            set { ballSpeedMax = value; }
        }

        private List<int> ballNumberSample;

        public List<int> BallNumberSample
        {
            get { return ballNumberSample; }
            set { ballNumberSample = value; }
        }

        void Start()
        {
            InitBox();
            InitBallType();
            InitBallPool();
            InitData();
        }

        void Update()
        {
        }

        private void InitBox()
        {
            boxes = new List<GameObject>();

            for (int i = 0; i < boxNum; i++)
            {
                GameObject newBox = Instantiate(box);
                newBox.SetActive(true);
                newBox.transform.localPosition = boxPos.transform.localPosition + i * boxSpace.transform.localPosition;
                newBox.transform.SetParent(boxRoot);
                boxes.Add(newBox);
            }
        }

        private void InitBallType()
        {
            ballTypes = new Dictionary<BallType, GameObject>();
            ballTypes.Add(BallType.Green, greenBall);
            ballTypes.Add(BallType.Red, redBall);
        }

        private void InitBallPool()
        {
            ballPool = new List<GameObject>[(int)BallType.Max];

            for (int i = 0; i < ballPool.Length; i++)
            {
                ballPool[i] = new List<GameObject>(BallPoolSize);

                for (int j = 0; j < ballPool[i].Capacity; j++)
                {
                    GameObject ball = Instantiate(ballTypes[(BallType)i]);
                    ball.SetActive(false);
                    ballPool[i].Add(ball);
                    ball.transform.SetParent(ballRoot);
                }
            }
        }

        private GameObject FindAnUnusedBall(BallType ballType)
        {
            List<GameObject> ballTypePool = ballPool[(int)ballType];

            foreach (GameObject ball in ballTypePool)
            {
                if (!ball.activeSelf)
                {
                    return ball;
                }
            }
            return null;
        }

        private void InitData()
        {
            ballActive = new List<GameObject>();

            ballNumberSample = new List<int>();
        }

        public void GenerateBall()
        {
            GenerateBall(ballMaxCount);
        }

        private void GenerateBall(int num)
        {
            for (int i = 0; i < (int)BallType.Max; i++)
            {
                if (i == (int)BallType.Max - 1)
                {
                    GenerateBall((BallType)i, num);
                }
                else
                {
                    int ballNum = Random.Range(0, num + 1);
                    GenerateBall((BallType)i, ballNum);
                    num -= ballNum;
                }
            }
        }

        private void GenerateBall(BallType ballType, int num)
        {
            for (int i = 0; i < num; i++)
            {
                GenerateBall(ballType);
            }
        }

        private void GenerateBall(BallType ballType)
        {
            GameObject ball = FindAnUnusedBall(ballType);

            if (ball != null)
            {
                if (GenerateBallPosition(ball) && GenerateBallNumber(ball))
                {
                    InitBallSpeed(ball);
                    ball.SetActive(true);
                    ballActive.Add(ball);
                    Common.LogD("Generate ball success");
                    return;
                }
            }
            Common.LogD("Generate ball failure");
        }

        public void RecycleBall(GameObject ball)
        {
            ball.SetActive(false);
            ballActive.Remove(ball);
            Common.LogD("Recycle ball");
        }

        public void RecycleAllBall()
        {
            while (ballActive.Count != 0)
            {
                RecycleBall(ballActive[0]);
            }
        }

        public void ReplenishBall()
        {
            if (ballActive.Count < HalfBallMaxCount)
            {
                GenerateBall(ballMaxCount - ballActive.Count);
                Common.LogD("Replenish ball");
            }
        }

        private bool GenerateBallPosition(GameObject ball)
        {
            recursionDepthCounter = 0;
            return RandomBallPosition(ball);
        }

        private bool RandomBallPosition(GameObject ball)
        {
            if (recursionDepthCounter++ >= RecursionDepth)
            {
                return false;
            }

            int randomBox = Random.Range(0, boxes.Count);
            Box box = boxes[randomBox].GetComponent<Box>();

            ball.transform.position = new Vector3(Random.Range(box.leftTop.position.x, box.rightTop.position.x),
                                                  Random.Range(box.leftBottom.position.y, box.leftTop.position.y),
                                                  box.leftTop.position.z);

            ball.GetComponent<Ball>().Boundary = new Rect(box.leftBottom.position.x, 
                                                          box.leftBottom.position.y,
                                                          box.rightTop.position.x - box.leftBottom.position.x,
                                                          box.rightTop.position.y - box.leftBottom.position.y);

            if (IsBallPositionValid(ball))
            {
                return true;
            }
            else
            {
                return RandomBallPosition(ball);
            }
        }

        private bool IsBallPositionValid(GameObject ball)
        {
            // Check the random position is valid
            Bounds bound = ball.GetComponent<SphereCollider>().bounds;

            foreach (GameObject existBall in ballActive)
            {
                Bounds existBound = existBall.GetComponent<SphereCollider>().bounds;

                if (existBound.Intersects(bound))
                {
                    return false;
                }
            }
            return true;
        }

        private bool GenerateBallNumber(GameObject ball)
        {
            recursionDepthCounter = 0;
            return RandomBallNumber(ball);
        }

        private bool RandomBallNumber(GameObject ball)
        {
            if (recursionDepthCounter++ >= RecursionDepth)
            {
                return false;
            }

            ball.GetComponent<Ball>().Number = ballNumberSample[Random.Range(0, ballNumberSample.Count)];

            if (IsBallNumberValid(ball))
            {
                return true;
            }
            else
            {
                return RandomBallNumber(ball);
            }
        }

        private bool IsBallNumberValid(GameObject ball)
        {
            // Check the random number is valid
            //foreach (GameObject existBall in ballActive)
            //{
            //    if (existBall.GetComponent<Ball>().Number == ball.GetComponent<Ball>().Number)
            //    {
            //        return false;
            //    }
            //}
            return true;
        }

        private void InitBallSpeed(GameObject ball)
        {
            ball.GetComponent<Ball>().SpeedMin = ballSpeedMin;
            ball.GetComponent<Ball>().SpeedMax = ballSpeedMax;
        }
    }
}
