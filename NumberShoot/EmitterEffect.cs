using UnityEngine;

namespace NumberShoot
{
    public class EmitterEffect : MonoBehaviour
    {
        public float effectTime;

        private float timer;

        void OnEnable()
        {
            ParticleSystem particle = GetComponent<ParticleSystem>();
            particle.Play();
            timer = 0;
        }

        void OnDisable()
        {
            ParticleSystem particle = GetComponent<ParticleSystem>();
            particle.Stop();
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer > effectTime)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
