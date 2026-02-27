using UnityEngine;

public class player : MonoBehaviour
{
    public Transform tilt;
    public Camera cam;

    public float panSpeed;
    public float tiltSpeed;
    public float zoomSpeed;

    void Update()
    {
        float pan = Input.GetAxis("Horizontal");
        float tiltInput = Input.GetAxis("Vertical");

        transform.Rotate(0, pan * panSpeed * Time.deltaTime, 0);
        tilt.Rotate(tiltInput * tiltSpeed * Time.deltaTime, 0, 0);

        if (Input.GetKey(KeyCode.Z))
            cam.fieldOfView -= zoomSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.X))
            cam.fieldOfView += zoomSpeed * Time.deltaTime;
    }
}
