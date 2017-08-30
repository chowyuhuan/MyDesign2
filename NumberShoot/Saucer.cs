using UnityEngine;
using System;
using System.Collections.Generic;

namespace NumberShoot
{
    public class Saucer : HitableObject 
    {
        public TextMesh textMesh;

        public float highMin;
        public float highMax;
        public float flyDelay;

        private float flyTime;

        public float FlyTime
        {
            get { return flyTime; }
            set { flyTime = value; }
        }

        private int number;

        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        private Vector3 startPos;

        public Vector3 StartPos
        {
            get { return startPos; }
            set { startPos = value; }
        }

        private Vector3 endPos;

        public Vector3 EndPos
        {
            get { return endPos; }
            set { endPos = value; }
        }

        public Action<GameObject> OnRecycle;

        private Vector3 Invisible = new Vector3(0, -1000, 0); 

        protected override void Awake()
        {
            base.Awake();

            effectBuffer = new List<GameObject>();

            PlayOutGlow();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            InitBall();
            Invoke("Appear", flyDelay);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            CancelInvoke("Appear");

            Destroy(gameObject.GetComponent<iTween>());
        }

        private void InitBall()
        {
            gameObject.transform.position = Invisible;
            Time = UnityEngine.Time.time;
            textMesh.text = number.ToString();

            ClearEffectBuffer();
        }

        private void Appear()
        {
            Move();
        }

        private void Move()
        {
            gameObject.transform.position = startPos;
            //Vector3 intermediate = (start + end) / 2 + new Vector3(0, UnityEngine.Random.Range(highMin, highMax), 0);
            //Vector3[] points = { start, intermediate, end};
            //Vector3[] path = Curve.Bezier(points, 50);
            Vector3[] path = UpCast(startPos, endPos, UnityEngine.Random.Range(highMin, highMax), 50);
            iTween.MoveTo(gameObject, iTween.Hash("path", path, "easeType", "linear", "time", flyTime, "oncomplete", "MoveFinished"));
        }

        private void MoveFinished()
        {
            OnRecycle(this.gameObject);
        }

        private Vector3[] UpCast(Vector3 start, Vector3 end, float high, int division)
        {
            Vector3[] path = new Vector3[division];

            float g = 9.8f;
            float v0 = Mathf.Sqrt(2 * g * high);
            float tAll = v0 / g * 2;
            float tUnit = tAll / division;
            float vx = (end.x - start.x) / division;
            float vz = (end.z - start.z) / division;

            for (int i = 0; i < path.Length; i++ )
            {
                float ti = tUnit * i;
                float y = v0 * ti - g * ti * ti / 2;
                float x = vx * i;
                float z = vz * i;
                path[i] = new Vector3(start.x + x, start.y + y, start.z + z);
            }

            return path;
        }

        public override void BeHurt(int hitRays)
        {
            OnEnterHurt(hitRays);
        }

        public GameObject SaucerSmokeEffect;
        public GameObject SaucerHitEffect;
        public GameObject SaucerPerfectHitEffect;
        public GameObject SaucerBrokenPattern;
        public GameObject SaucerOutGlow;

        private List<GameObject> effectBuffer;

        private float brokenSaucerGravity = 2.0f;

        private void ClearEffectBuffer()
        {
            foreach (GameObject gameObject in effectBuffer)
            {
                Destroy(gameObject);
            }
            effectBuffer.Clear();
        }

        void OnEnterHurt(int damageLevel)
        {
            GameObject hitEffect = Instantiate(SaucerHitEffect, transform.position, new Quaternion());
            if (DataCenter.soundEnable != 0)
            {
                AudioSource audioSaucer = hitEffect.GetComponent<AudioSource>();
                audioSaucer.Play();
            }
            effectBuffer.Add(hitEffect);

            if (null != SaucerPerfectHitEffect && damageLevel >= ScoreConfig.Instance.PerfectHitRays)
            {
                GameObject EffectObj = Instantiate(SaucerPerfectHitEffect, transform.position, new Quaternion());
                effectBuffer.Add(EffectObj);
            }

            this.PlaySmoke();


            MeshCollider saucerCollider = GetComponent<MeshCollider>();

            //将碎片炸开
            GameObject brokenSaucer = Instantiate(SaucerBrokenPattern);
            brokenSaucer.transform.position = transform.position;
            brokenSaucer.transform.localEulerAngles = transform.localEulerAngles;
            brokenSaucer.transform.Rotate(new Vector3(1, 0, 0), 90);
            Vector3 anglurVelocity = brokenSaucer.transform.up * _rigidBody.angularVelocity.magnitude;

            //根据damageLevel随机出N块被炸飞
            int childCount = brokenSaucer.transform.childCount;
            int flyCount = (int)(childCount * damageLevel / BattleCommon.MaxHitRays);
            int startIndex = UnityEngine.Random.Range(0, childCount - 1);
            Common.LogD("childCount=" + childCount + ", flyCount=" + flyCount + ", startIndex=" + startIndex);
            GameObject[] flyObjects = new GameObject[flyCount];
            for (int i = 0; i < flyCount; i++)
            {
                int targetIndex = startIndex;
                if (targetIndex >= brokenSaucer.transform.childCount)
                {
                    targetIndex = 0;
                }
                GameObject flyObj = brokenSaucer.transform.GetChild(targetIndex).gameObject;
                flyObj.transform.parent = transform.parent;
                flyObjects[i] = flyObj;
                Rigidbody body = flyObj.AddComponent<Rigidbody>();
                body.mass = 1.0f / (float)childCount;
                body.velocity = _rigidBody.velocity;
                body.drag = _rigidBody.drag * 0.6f;
                body.angularVelocity = anglurVelocity;
                body.angularDrag = _rigidBody.angularDrag;
                body.useGravity = false;
                body.AddExplosionForce(80.0f, transform.position + UnityEngine.Random.insideUnitSphere + new Vector3(0f, -0.4f, 0f), 10);
                //碰撞器
                MeshCollider collider = flyObj.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.material = saucerCollider.material;

                //控制重力
                BrokenSaucer broken = flyObj.AddComponent<BrokenSaucer>();
                broken.Gravity = brokenSaucerGravity;
            }
            if (flyCount < childCount)
            {
                //其余块位置略有浮动
                for (int i = 0; i < brokenSaucer.transform.childCount; i++)
                {
                    Transform child = brokenSaucer.transform.GetChild(i);
                    child.position += new Vector3(0, UnityEngine.Random.Range(-0.005f, 0.005f), 0);

                    //碰撞器
                    MeshCollider collider = child.gameObject.AddComponent<MeshCollider>();
                    collider.convex = true;
                    collider.material = saucerCollider.material;
                }

                Rigidbody brokenBody = brokenSaucer.AddComponent<Rigidbody>();
                brokenBody.mass = (childCount - flyCount) * 1.0f / (float)childCount;
                brokenBody.velocity = _rigidBody.velocity;
                brokenBody.drag = _rigidBody.drag * 0.6f;
                brokenBody.angularVelocity = anglurVelocity;
                brokenBody.angularDrag = _rigidBody.angularDrag;
                brokenBody.useGravity = false;


                //控制重力
                BrokenSaucer bs = brokenSaucer.AddComponent<BrokenSaucer>();
                bs.Gravity = brokenSaucerGravity;
            }
            else
            {
                Destroy(brokenSaucer);
            }
        }

        void PlaySmoke()
        {
            if (SaucerSmokeEffect == null)
            {
                return;
            }
            GameObject smokeEffect = Instantiate(SaucerSmokeEffect, transform.position, Quaternion.LookRotation(_rigidBody.velocity));
            effectBuffer.Add(smokeEffect);
        }

        void PlayOutGlow()
        {
            Instantiate(SaucerOutGlow, transform, false);
        }
    }
}
