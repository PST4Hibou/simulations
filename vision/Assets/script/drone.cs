using UnityEngine;

namespace script
{
    public class Drone : MonoBehaviour
    {
        [Header("Reproducibility")]
        public int seed = 12345;

        [Header("Start Position")]
        public Vector3 startPosition = new Vector3(0, 40, 0);

        [Header("Movement Area")]
        public float areaSize = 100f;
        public float minHeight = 10f;
        public float maxHeight = 80f;

        [Header("Movement Settings")]
        public float minSpeed = 5f;
        public float maxSpeed = 15f;
        public float turnSpeed = 2f;
        public float waypointThreshold = 3f;

        [Header("Noise Settings")]
        public float heightNoiseScale = 0.2f;
        public float heightNoiseAmplitude = 10f;

        private Vector3 targetPosition;
        private float currentSpeed;
        private float noiseOffset;

        void Start()
        {
            // Set deterministic random seed
            Random.InitState(seed);

            // Set start position
            transform.position = startPosition;

            Renderer renderer = GetComponent<Renderer>();
            renderer.material.color = Color.red;

            noiseOffset = Random.Range(0f, 100f);
            PickNewTarget();
        }

        void Update()
        {
            MoveDrone();
        }

        void MoveDrone()
        {
            Vector3 direction = (targetPosition - transform.position).normalized;

            Vector3 smoothDirection = Vector3.Slerp(
                transform.forward,
                direction,
                turnSpeed * Time.deltaTime
            );
            
            transform.rotation = Quaternion.LookRotation(smoothDirection);
            transform.position += transform.forward * currentSpeed * Time.deltaTime;

            float noise = Mathf.PerlinNoise(Time.time * heightNoiseScale, noiseOffset);
            float heightOffset = (noise - 0.5f) * heightNoiseAmplitude;

            Vector3 pos = transform.position;
            pos.y = Mathf.Clamp(
                pos.y + heightOffset * Time.deltaTime,
                minHeight,
                maxHeight
            );

            transform.position = pos;

            if (Vector3.Distance(transform.position, targetPosition) < waypointThreshold)
                PickNewTarget();
        }

        void PickNewTarget()
        {
            targetPosition = new Vector3(
                Random.Range(-areaSize, areaSize),
                Random.Range(minHeight, maxHeight),
                Random.Range(-areaSize, areaSize)
            );

            currentSpeed = Random.Range(minSpeed, maxSpeed);
        }
    }
}