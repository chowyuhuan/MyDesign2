using UnityEngine;
using System;

namespace BALL
{
    public enum Symbol
    {
        Positive = 1,
        Negative = -1,
        Size = 2
    }

    public class Ball : AimTargetObject 
    {
        public TextMesh textMesh;

        public int shootChangeUnit;

        public int edgeChangeUnit;

        private int number;

        private Vector3 velocity;

        private GameObject touchEdge;

        private Rect boundary;

        public Rect Boundary
        {
            get { return boundary; }
            set { boundary = value; }
        }

        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        private float speedMin;

        public float SpeedMin
        {
            get { return speedMin; }
            set { speedMin = value; }
        }

        private float speedMax;

        public float SpeedMax
        {
            get { return speedMax; }
            set { speedMax = value; }
        }

        void OnEnable()
        {
            InitBall();
            Appear();
        }

        void FixedUpdate()
        {
            CheckTouchEdge();
            KeepVelocity();
        }

        private void InitBall()
        {
            textMesh.text = number.ToString();
        }

        private void Appear()
        {
            GUI_TweenerUtility.ResetAndPlayTweener(this.gameObject, Move);
        }

        private void Move()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            int symbolX = UnityEngine.Random.Range(0, (int)Symbol.Size) == 0 ? (int)Symbol.Positive: (int)Symbol.Negative;
            int symbolY = UnityEngine.Random.Range(0, (int)Symbol.Size) == 0 ? (int)Symbol.Positive: (int)Symbol.Negative;
            velocity = new Vector3(UnityEngine.Random.Range(speedMin, speedMax) * symbolX,
                                   UnityEngine.Random.Range(speedMin, speedMax) * symbolY,
                                   0);
            rb.AddForce(velocity, ForceMode.VelocityChange);
        }

        private void KeepVelocity()
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            rb.velocity = rb.velocity.normalized * velocity.magnitude;
        }

        private void ReflectDirection(GameObject edge)
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            switch(edge.name)
            {
                case "Left":
                case "Right":
                    rb.velocity = Vector3.Reflect(rb.velocity, rb.velocity.y < 0 ? Vector3.left : Vector3.right);
                    break;
                case "Top":
                case "Bottom":
                    rb.velocity = Vector3.Reflect(rb.velocity, rb.velocity.x < 0 ? Vector3.up : Vector3.down);
                    break;
            }
        }

        private void RecordTouchEdge(GameObject edge)
        {
            touchEdge = edge;
        }

        private void CheckTouchEdge()
        {
            if (touchEdge != null)
            {
                CheckBoundary();
                ReflectDirection(touchEdge);
                touchEdge = null;
            }
        }

        private void CheckBoundary()
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            float x = rb.position.x;
            float y = rb.position.y;
            
            if (rb.position.x < boundary.xMin)
            {
                x = boundary.xMin;
            }
            else if (rb.position.x > boundary.xMax)
            {
                x = boundary.xMax;
            }

            if (rb.position.y < boundary.yMin)
            {
                y = boundary.yMin;
            }
            else if (rb.position.y > boundary.yMax)
            {
                y = boundary.yMax;
            }

            rb.position = new Vector3(x, y, rb.position.z);
        }

        public void RefreshNumberByShoot()
        {
            number += shootChangeUnit;
            number = BallCommon.RoundNumber(number);
            textMesh.text = number.ToString();
        }

        private void RefreshNumberByEdge()
        {
            number += edgeChangeUnit;
            number = BallCommon.RoundNumber(number);
            textMesh.text = number.ToString();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "BoxEdge")
            {
                RefreshNumberByEdge();
                RecordTouchEdge(other.gameObject);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "BoxEdge")
            {
                RefreshNumberByEdge();
                RecordTouchEdge(collision.gameObject);
            }
        }

        public override void BeHurt(int hitRays)
        {
        }
    }
}
