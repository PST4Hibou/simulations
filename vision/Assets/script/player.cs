using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System.Net;

namespace script
{
    public class Player : MonoBehaviour
    {
        public Transform tilt; // tilt transform of camera (optional for IBVS response)
        public Camera cam; // main camera
        public Transform target; // object to track

        public float virtualFPS; // packets per second

        private UdpClient _client;
        private IPEndPoint _remoteEndPoint;
        private float _sendTimer = 0f;

        void Awake()
        {
            // Keep Unity running even if window loses focus
            Application.runInBackground = true;
        }

        void Start()
        {
            _client = new UdpClient();
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5005);
        }

        void Update()
        {
            _sendTimer += Time.deltaTime;

            Debug.Log(_sendTimer + " >= interval? " + (_sendTimer >= 1f / virtualFPS));

            if (_sendTimer >= 1.00f / virtualFPS)
            {
                _sendTimer = 0; // keep leftover time
                SendTargetCoordinates();
            }
            
            ReceivePtzCommands();
        }

        private void SendTargetCoordinates()
        {
            if (cam == null || target == null) return;

            Vector3 viewportPoint = cam.WorldToViewportPoint(target.position);

            // Check if target is in the camera's field of view
            bool isVisible =
                viewportPoint.z > 0 &&
                viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                viewportPoint.y >= 0 && viewportPoint.y <= 1;

            if (isVisible)
            {
                // Normalize coordinates to [-1, 1]
                float u = 2f * (viewportPoint.x - 0.5f);
                float v = 2f * (viewportPoint.y - 0.5f);

                string message = u + "," + v;
                byte[] data = Encoding.ASCII.GetBytes(message);

                _client.Send(data, data.Length, _remoteEndPoint);

                // Debug.Log("Sending: " + message); // Uncomment for debug
            }
            else
            {
                // Target not visible; optional: send zeros or do nothing
            }
        }

        
        // Optional method to receive PTZ velocities from Python
        private void ReceivePtzCommands()
        {
            if (_client.Available > 0)
            {
                byte[] received = _client.Receive(ref _remoteEndPoint);
                string response = Encoding.ASCII.GetString(received);

                string[] values = response.Split(',');
                if (values.Length >= 2)
                {
                    if (float.TryParse(values[0], out float omega_x) &&
                        float.TryParse(values[1], out float omega_y))
                    {
                        transform.Rotate(0, -omega_y * Time.deltaTime, 0);
                        tilt.Rotate(-omega_x * Time.deltaTime, 0, 0);
                    }
                }
            }
        }
        
    }
}