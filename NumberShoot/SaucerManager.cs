using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace NumberShoot
{
    public class SaucerManager : MonoBehaviour
    {
        public Transform emitStartPos;
        public int emitNumX;
        public int emitNumY;
        public Transform emitSpaceX;
        public Transform emitSpaceY;
        public Transform emitRoot;

        public GameObject emitter;
        public GameObject normalSaucer;
        public Transform saucerRoot;

        public int saucerRound;
        public float saucerRoundInterval;

        public float saucerBonusDelay;
        public float bonusSaucerInterval;
        public float bonusSaucerFlyTime;

        private List<GameObject> emitters;
        // Saucer type map
        private Dictionary<SaucerType, GameObject> saucerTypes;
        // Saucer pool for each type
        private const int SaucerPoolSize = 16;
        private List<GameObject>[] saucerPool;
        // Record ball active
        private List<GameObject> saucerActive;

        public List<GameObject> SaucerActive
        {
            get { return saucerActive; }
        }

        // Generate ball random function recursion depth
        private const int RecursionDepth = 50;
        private int recursionDepthCounter;

        public List<int[]> saucerAnswers;
        private List<Queue<int>> saucerNumbers;
        private float saucerInterval;
        private float saucerFlyTime;

        public Action OnSaucerRoundFinished;
        public Action OnAllSaucerRoundFinished;
        public Action OnAllSaucerBonusFinished;
        public Action<int[]> OnSaucerRoundAnswer;

        private bool isBonusTime;

        public bool IsBonusTime
        {
            get { return isBonusTime; }
            set { isBonusTime = value; }
        }

        private int[,] bonusEmiterPos = new int[10, 2] { { 0, 2 }, { 1, 1 }, { 2, 0 }, { 3, 1 }, { 4, 2 }, { 4, 4 }, { 3, 5 }, { 2, 6 }, { 1, 5 }, { 0, 4 } };

        void Start()
        {
            InitEmitter();
            InitSaucerType();
            InitSaucerPool();
            InitData();

            //InvokeRepeating("GenerateSaucer", 1.5f, 1.5f);
        }

        void Update()
        {
        }

        private void InitEmitter()
        {
            emitters = new List<GameObject>();

            for (int j = 0; j < emitNumY; j++)
            {
                for (int i = 0; i < emitNumX; i++)
                {
                    GameObject newEmitter = Instantiate(emitter);
                    newEmitter.SetActive(true);
                    newEmitter.transform.localPosition = emitStartPos.transform.localPosition + i * emitSpaceX.transform.localPosition + j * emitSpaceY.transform.localPosition;
                    newEmitter.transform.SetParent(emitRoot);
                    emitters.Add(newEmitter);
                }
            }
        }

        private void InitSaucerType()
        {
            saucerTypes = new Dictionary<SaucerType, GameObject>();
            saucerTypes.Add(SaucerType.Normal, normalSaucer);
        }

        private void InitSaucerPool()
        {
            saucerPool = new List<GameObject>[(int)SaucerType.Max];

            for (int i = 0; i < saucerPool.Length; i++)
            {
                saucerPool[i] = new List<GameObject>(SaucerPoolSize);

                for (int j = 0; j < saucerPool[i].Capacity; j++)
                {
                    GameObject saucer = Instantiate(saucerTypes[(SaucerType)i]);
                    saucer.GetComponent<Saucer>().OnRecycle = RecycleSaucer;
                    saucer.SetActive(false);
                    saucerPool[i].Add(saucer);
                    saucer.transform.SetParent(saucerRoot);
                }
            }
        }

        private GameObject FindAnUnusedSaucer(SaucerType saucerType)
        {
            List<GameObject> saucerTypePool = saucerPool[(int)saucerType];

            foreach (GameObject saucer in saucerTypePool)
            {
                if (!saucer.activeSelf)
                {
                    return saucer;
                }
            }
            return null;
        }

        private void InitData()
        {
            saucerActive = new List<GameObject>();
        }

        public void InitSaucerBonus()
        {
            this.isBonusTime = true;
            this.saucerInterval = bonusSaucerInterval;
            this.saucerFlyTime = bonusSaucerFlyTime;

            saucerNumbers = new List<Queue<int>>();

            Queue<int> numbers = new Queue<int>();
            for (int i = 0; i < bonusEmiterPos.GetLength(0); i++)
            {
                numbers.Enqueue(i % ((int)SaucerNumberRange.Max + 1));
            }
            saucerNumbers.Add(numbers);

            StartCoroutine("StartSaucerBonus");
        }

        public void InitSaucerRound(List<int[]> samples, int saucerNum, float saucerInterval, float saucerFlyTime)
        {
            if (samples.Count == 0)
            {
                Common.LogE("InitSaucerRound error");
            }

            this.isBonusTime = false;
            this.saucerInterval = saucerInterval;
            this.saucerFlyTime = saucerFlyTime;

            saucerAnswers = new List<int[]>();
            saucerNumbers = new List<Queue<int>>();

            for (int i = 0; i < saucerRound; i++)
            {
                int[] sample = samples[UnityEngine.Random.Range(0, samples.Count)];

                Queue<int> numbers = new Queue<int>(sample);

                for (int j = numbers.Count; j < saucerNum; j++)
                {
                    recursionDepthCounter = 0;
                    RandomBallNumber(numbers);
                }

                numbers = new Queue<int>(numbers.Select(a => new { a, newID = Guid.NewGuid() }).OrderBy(b => b.newID).Select(c => c.a));

                saucerAnswers.Add(sample);
                saucerNumbers.Add(numbers);
            }
        }

        public void StartEmitSaucer()
        {
            StartCoroutine("StartSaucerRound");
        }

        IEnumerator StartSaucerRound()
        {
            OnSaucerRoundAnswer(saucerAnswers[0]);

            yield return new WaitForSeconds(saucerRoundInterval);

            StartCoroutine("AutoGenerateRoundSaucer");
        }

        IEnumerator AutoGenerateRoundSaucer()
        {
            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0, saucerInterval));

                if (IsAllRoundFinished() || IsCurrentRoundFinished())
                {
                    StopCoroutine("AutoGenerateRoundSaucer");
                }
                else
                {
                    GenerateSaucer(SaucerType.Normal, saucerNumbers[0].Dequeue());
                }
            }
        }

        IEnumerator StartSaucerBonus()
        {
            yield return new WaitForSeconds(saucerBonusDelay);

            StartCoroutine("AutoGenerateBonusSaucer");
        }

        IEnumerator AutoGenerateBonusSaucer()
        {
            while (true)
            {
                yield return new WaitForSeconds(saucerInterval);

                if (IsAllRoundFinished() || IsCurrentRoundFinished())
                {
                    StopCoroutine("AutoGenerateBonusSaucer");
                }
                else
                {
                    int number = saucerNumbers[0].Dequeue();
                    GenerateSaucer(SaucerType.Normal, number, bonusEmiterPos[number, 0], bonusEmiterPos[number, 1]);
                }
            }
        }

        private void CheckSaucerRoundFinished()
        {
            if (saucerActive.Count == 0)
            {
                if (IsCurrentRoundFinished())
                {
                    saucerAnswers.RemoveAt(0);
                    saucerNumbers.RemoveAt(0);

                    if (IsAllRoundFinished())
                    {
                        OnAllSaucerRoundFinished();
                    }
                    else
                    {
                        StartCoroutine("StartSaucerRound");
                        OnSaucerRoundFinished();
                    }
                }
            }
        }

        private void CheckSaucerBonusFinished()
        {
            if (saucerActive.Count == 0)
            {
                if (IsCurrentRoundFinished())
                {
                    saucerNumbers.RemoveAt(0);

                    if (IsAllRoundFinished())
                    {
                        OnAllSaucerBonusFinished();
                    }
                    else
                    {
                        StartCoroutine("StartSaucerBonus");
                    }
                }
            }
        }

        public void SetCurrentRoundFinished()
        {
            if (!IsAllRoundFinished())
            {
                saucerNumbers[0].Clear();
            }
            StopCoroutine("AutoGenerateRoundSaucer");
        }

        private bool IsCurrentRoundFinished()
        {
            return saucerNumbers[0].Count == 0;
        }

        private bool IsAllRoundFinished()
        {
            return saucerNumbers.Count == 0;
        }

        public void GenerateSaucer()
        {
            GenerateSaucer(SaucerType.Normal, 1);
        }

        private void GenerateSaucer(SaucerType saucerType, int number)
        {
            GameObject saucer = FindAnUnusedSaucer(saucerType);

            if (saucer != null)
            {
                if (GenerateSaucerDifferentRowColTrack(saucer) && GenerateSacuerNumber(saucer, number))
                {
                    saucer.GetComponent<Saucer>().FlyTime = saucerFlyTime;
                    saucer.SetActive(true);
                    saucerActive.Add(saucer);
                    Common.LogD("Generate saucer success");
                    return;
                }
            }
            Common.LogD("Generate saucer failure");
        }

        private void GenerateSaucer(SaucerType saucerType, int number, int row, int col)
        {
            GameObject saucer = FindAnUnusedSaucer(saucerType);

            if (saucer != null)
            {
                if (GenerateSaucerDetermineTrack(saucer, row, col) && GenerateSacuerNumber(saucer, number))
                {
                    saucer.GetComponent<Saucer>().FlyTime = saucerFlyTime;
                    saucer.SetActive(true);
                    saucerActive.Add(saucer);
                    Common.LogD("Generate saucer success");
                    return;
                }
            }
            Common.LogD("Generate saucer failure");
        }

        public void RecycleSaucer(GameObject saucer)
        {
            saucer.SetActive(false);
            saucerActive.Remove(saucer);
            Common.LogD("Recycle saucer");

            if (isBonusTime)
            {
                CheckSaucerBonusFinished();
            }
            else
            {
                CheckSaucerRoundFinished();
            }
        }

        public void RecycleAllSaucer()
        {
            while (saucerActive.Count != 0)
            {
                RecycleSaucer(saucerActive[0]);
            }
        }

        private GameObject GetRandomEmitter()
        {
            return emitters[UnityEngine.Random.Range(0, emitters.Count)];
        }

        private GameObject GetRandomEmitter(int row)
        {
            return emitters[row * emitNumX + UnityEngine.Random.Range(0, emitNumX)];
        }

        private GameObject GetEmitter(int row, int col)
        {
            return emitters[row * emitNumX + col];
        }

        private void ShowEmitterEffect(GameObject emitter)
        {
            emitter.transform.GetChild(0).gameObject.SetActive(true);
        }

        private void SetSaucerTrack(GameObject saucer, GameObject startEmitter, GameObject endEmitter)
        {
            Saucer saucerScript = saucer.GetComponent<Saucer>();
            saucerScript.StartPos = startEmitter.transform.position;
            saucerScript.EndPos = endEmitter.transform.position;
            ShowEmitterEffect(startEmitter);
        }

        private bool GenerateSaucerSameRowTrack(GameObject saucer)
        {
            int row = UnityEngine.Random.Range(0, emitNumY);
            GameObject startEmitter = GetRandomEmitter(row);
            GameObject endEmitter = GetRandomEmitter(row);
            SetSaucerTrack(saucer, startEmitter, endEmitter);
            return true;
        }

        private bool GenerateSaucerDifferentRowColTrack(GameObject saucer)
        {
            int startRow = UnityEngine.Random.Range(0, emitNumY);
            int startCol = UnityEngine.Random.Range(0, emitNumX);
            recursionDepthCounter = 0;
            int endRow = RandomDifferentNumber(0, emitNumY, startRow);
            int endCol = RandomDifferentNumber(0, emitNumX, startCol);
            GameObject startEmitter = GetEmitter(startRow, startCol);
            GameObject endEmitter = GetEmitter(endRow, endCol);
            SetSaucerTrack(saucer, startEmitter, endEmitter);
            return true;
        }

        private bool GenerateSaucerDetermineTrack(GameObject saucer, int row, int col)
        {
            GameObject emitter = GetEmitter(row, col);
            SetSaucerTrack(saucer, emitter, emitter);
            return true;
        }

        private bool GenerateSacuerNumber(GameObject saucer, int number)
        {
            saucer.GetComponent<Saucer>().Number = number;
            return true;
        }

        private bool RandomBallNumber(Queue<int> existNumbers)
        {
            if (recursionDepthCounter++ >= RecursionDepth)
            {
                return false;
            }

            int number = UnityEngine.Random.Range((int)SaucerNumberRange.Min, (int)SaucerNumberRange.Max + 1);

            if (IsBallNumberValid(existNumbers, number))
            {
                existNumbers.Enqueue(number);
                return true;
            }
            else
            {
                return RandomBallNumber(existNumbers);
            }
        }

        private bool IsBallNumberValid(Queue<int> existNumbers, int number)
        {
            // Check the random number is valid
            return !existNumbers.Contains(number);
        }

        private int RandomDifferentNumber(int min, int max, int existNumber)
        {
            if (recursionDepthCounter++ >= RecursionDepth)
            {
                return 0;
            }

            int number = UnityEngine.Random.Range(min, max);

            if (existNumber != number)
            {
                return number;
            }
            else
            {
                return RandomDifferentNumber(min, max, existNumber);
            }
        }
    }
}
