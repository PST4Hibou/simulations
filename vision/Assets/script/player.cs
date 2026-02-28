using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System.Net;

namespace script
{
    public class Player : MonoBehaviour
    {
        [Header("PTZ Limits")] public float panMaxSpeed = 100f; // deg/sec
        public float tiltMaxSpeed = 50f; // deg/sec

        public float tiltMinAngle = -90f;
        public float tiltMaxAngle = 40f;

        [Header("Motor Dynamics")] public float acceleration = 200f; // deg/sec²
        public float commandTimeout = 0.3f; // seconds before auto stop

        private float currentPanSpeed;
        private float currentTiltSpeed;

        private float commandedPan; // last commanded velocity (-10..10)
        private float commandedTilt;

        private float lastCommandTime;

        // public Transform tilt;
        public Camera cam;
        public Transform target;

        public float virtualFPS = 30f;

        private UdpClient _client;
        private IPEndPoint _remoteEndPoint;
        private float _sendTimer = 0f;

        void Start()
        {
            _client = new UdpClient();
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5005);
        }

        void Update()
        {
            // -------------------------
            // SEND TRACKING DATA
            // -------------------------
            _sendTimer += Time.deltaTime;

            float interval = 1f / virtualFPS;
            if (_sendTimer >= interval)
            {
                _sendTimer -= interval; // keep leftover precision
                SendTargetCoordinates();
            }

            // -------------------------
            // RECEIVE COMMANDS
            // -------------------------
            ReceivePtzCommands();

            // -------------------------
            // SAFETY TIMEOUT (like ONVIF PTZ)
            // -------------------------
            if (Time.time - lastCommandTime > commandTimeout)
            {
                commandedPan = 0f;
                commandedTilt = 0f;
            }

            // -------------------------
            // ALWAYS UPDATE MOTOR
            // -------------------------
            ApplyPtzVelocity(commandedTilt, commandedPan);
        }

        private void ApplyPtzVelocity(float virtualTilt, float virtualPan)
        {
            // Clamp input
            virtualPan = Mathf.Clamp(virtualPan, -10f, 10f);
            virtualTilt = Mathf.Clamp(virtualTilt, -10f, 10f);

            // Optional deadband (removes jitter)
            if (Mathf.Abs(virtualPan) < 0.05f) virtualPan = 0f;
            if (Mathf.Abs(virtualTilt) < 0.05f) virtualTilt = 0f;

            // Convert to real motor speeds (deg/sec)
            float targetPanSpeed = (virtualPan / 10f) * panMaxSpeed;
            float targetTiltSpeed = (virtualTilt / 10f) * tiltMaxSpeed;

            // Smooth acceleration
            currentPanSpeed = Mathf.MoveTowards(
                currentPanSpeed,
                targetPanSpeed,
                acceleration * Time.deltaTime);

            currentTiltSpeed = Mathf.MoveTowards(
                currentTiltSpeed,
                targetTiltSpeed,
                acceleration * Time.deltaTime);

            // ----------------------
            // PAN (endless)
            // ----------------------
            float panDelta = currentPanSpeed * Time.deltaTime;
            transform.Rotate(0f, -panDelta, 0f, Space.Self);

            // ----------------------
            // TILT (limited)
            // ----------------------
            // float tiltDelta = -currentTiltSpeed * Time.deltaTime;

            // float currentTilt = tilt.localEulerAngles.x;
            // if (currentTilt > 180f)
                // currentTilt -= 360f;

            // float newTilt = currentTilt + tiltDelta;
            // newTilt = Mathf.Clamp(newTilt, tiltMinAngle, tiltMaxAngle);

            // tilt.localRotation = Quaternion.Euler(newTilt, 0f, 0f);
        }

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
                        commandedPan = omega_x;
                        commandedTilt = omega_y;
                        lastCommandTime = Time.time;
                    }
                }
            }
        }

        private void SendTargetCoordinates()
        {
            if (cam == null || target == null)
            {
                SendNone();
                return;
            }

            Vector3 viewportPoint = cam.WorldToViewportPoint(target.position);

            bool isVisible =
                viewportPoint.z > 0f && // in front of camera
                viewportPoint.x >= 0f &&
                viewportPoint.x <= 1f &&
                viewportPoint.y >= 0f &&
                viewportPoint.y <= 1f;

            if (!isVisible)
            {
                SendNone();
                return;
            }

            float u = (viewportPoint.x);
            float v = (viewportPoint.y);

            string message = u + "," + v;
            byte[] data = Encoding.ASCII.GetBytes(message);
            _client.Send(data, data.Length, _remoteEndPoint);
        }

        private void SendNone()
        {
            byte[] data = Encoding.ASCII.GetBytes("None");
            _client.Send(data, data.Length, _remoteEndPoint);
        }
    }
}