using UnityEngine;


namespace script
{
    public class Drone : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = new Vector3(
                Mathf.Sin(Time.time / 5) * 100,
                0,
                40
            );
        }
    }
}