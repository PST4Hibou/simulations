using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System.Net;

public class player : MonoBehaviour
{
    public Transform tilt;
    public Camera cam;
    public Transform target;

    UdpClient client;
    IPEndPoint remoteEndPoint;

    public float rotationScale = 50f;

    void Start()
    {
        client = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5005);
    }
    
    void Update()
    {
        Vector3 viewportPoint = cam.WorldToViewportPoint(target.position);

        bool isVisible =
            viewportPoint.x is >= 0 and <= 1 &&
            viewportPoint.y is >= 0 and <= 1;

        if (isVisible)
        {
            // Normalized coordinates in [-1, 1]
            float u = 2f * (viewportPoint.x - 0.5f);
            float v = 2f * (viewportPoint.y - 0.5f);

            string message = u + "," + v;
            byte[] data = Encoding.ASCII.GetBytes(message);

            client.Send(data, data.Length, remoteEndPoint);
        }
        else
        {
            // Debug.Log("Target not visible");
        }
        
        

        //     // // Receive response
        //     // if (client.Available > 0)
        //     // {
        //     //     byte[] received = client.Receive(ref remoteEndPoint);
        //     //     string response = Encoding.ASCII.GetString(received);
        //     //
        //     //     string[] values = response.Split(',');
        //     //     float omega_x = float.Parse(values[0]);
        //     //     float omega_y = float.Parse(values[1]);
        //     //
        //     //     // Apply PTZ rotation
        //     //     transform.Rotate(0, omega_y * rotationScale * Time.deltaTime, 0);
        //     //     tilt.Rotate(omega_x * rotationScale * Time.deltaTime, 0, 0);
        //     // }
    }
}
