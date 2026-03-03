using UnityEngine;


namespace script
{
    public class Drone : MonoBehaviour
    {
        void Start()
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.color = Color.red;
        }
        // Update is called once per frame
        void Update()
        {
            // transform.position = new Vector3(
            //     Mathf.Sin(Time.time / 5) * 100,
            //     Mathf.Sin(Time.time / 5) * 10,
            //     40
            // );
            transform.position = new Vector3(
                Mathf.Sin(Time.time / 5) * 100,
                Mathf.Sin(Time.time / 6) * 50,
                40
            );
        }
    }
}