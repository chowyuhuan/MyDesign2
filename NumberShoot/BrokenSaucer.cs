using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NumberShoot
{
    public class BrokenSaucer : MonoBehaviour {

        [HideInInspector]
        public float Gravity = 0f;

        protected Rigidbody rigidBody;
        bool readyToDeath = false;
        float deathTimerStart = 0;

        // Use this for initialization
        void Start () {
            rigidBody = this.GetComponent<Rigidbody>();
        }
        
        // Update is called once per frame
        void Update () {
            if (!rigidBody.IsSleeping())
            {
                rigidBody.AddForce(new Vector3(0, -Gravity, 0));
                if (!readyToDeath && transform.position.y < 2.3f)
                {
                    readyToDeath = true;
                    deathTimerStart = Time.time;
                }
            }
            else
            {
                Destroy(gameObject);
            }

            if (readyToDeath)
            {
                if (Time.time - deathTimerStart >= 3f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
