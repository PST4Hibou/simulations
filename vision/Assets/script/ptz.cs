using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System.Net;

namespace script
{
    public class PtzBase : MonoBehaviour
    {
        [Header("PTZ Limits")]
        public float panMaxSpeed = 100f; // deg/sec
        public float tiltMaxSpeed = 50f; // deg/sec

        public float tiltMinAngle = -90f;
        public float tiltMaxAngle = 40f;

        [Header("Motor Dynamics")]
        public float acceleration = 200f; // deg/sec²
        // public float commandTimeout = 0.3f; // seconds before auto stop

        private float _currentPanSpeed;
        private float _currentTiltSpeed;

        private float _commandedPan; // last commanded velocity (-10..10)
        private float _commandedTilt;

        private float _lastCommandTime;

        // public Transform tilt;
        public Transform tiltNode;
        public Camera cam;
        public Transform target;

        public float virtualFPS = 30f;

        private float _sendTimer = 0f;

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
            // if (Time.time - _lastCommandTime > commandTimeout)
            // {
            //     _commandedPan = 0f;
            //     _commandedTilt = 0f;
            // }

            // -------------------------
            // ALWAYS UPDATE MOTOR
            // -------------------------
            ApplyPtzVelocity(_commandedTilt, _commandedPan);
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
            _currentPanSpeed = Mathf.MoveTowards(
                _currentPanSpeed,
                targetPanSpeed,
                acceleration * Time.deltaTime);

            _currentTiltSpeed = Mathf.MoveTowards(
                _currentTiltSpeed,
                targetTiltSpeed,
                acceleration * Time.deltaTime);

            // ----------------------
            // PAN (endless)
            // ----------------------
            float panDelta = _currentPanSpeed * Time.deltaTime;
            transform.Rotate(0f, -panDelta, 0f, Space.Self);

            // ----------------------
            // TILT (limited)
            // ----------------------
            float tiltDelta = _currentTiltSpeed * Time.deltaTime;

            // Get current tilt in signed form (-180 to 180)
            float currentTilt = tiltNode.localEulerAngles.x;
            if (currentTilt > 180f)
                currentTilt -= 360f;

            // Apply movement (invert sign if direction feels wrong)
            float newTilt = currentTilt - tiltDelta;

            // Clamp to physical limits
            newTilt = Mathf.Clamp(newTilt, tiltMinAngle, tiltMaxAngle);

            tiltNode.localRotation = Quaternion.Euler(newTilt, 0f, 0f);
        }

        private void ReceivePtzCommands()
        {
            if (UdpManager.Instance == null) return;

            // Keep reading all PTZ messages from the queue
            while (UdpManager.Instance.TryGetMessage("PTZ", out string data))
            {
                if (string.IsNullOrEmpty(data))
                    continue;
                
                string[] values = data.Split(',');
                if (values.Length >= 2)
                {
                    if (float.TryParse(values[0], out float omega_x) &&
                        float.TryParse(values[1], out float omega_y))
                    {
                        _commandedPan = omega_x;
                        _commandedTilt = omega_y;
                        _lastCommandTime = Time.time;
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to parse PTZ values: {data}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid PTZ message format: {data}");
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
            UdpManager.Instance.Send("PTZ", message);
        }

        private void SendNone()
        {
            UdpManager.Instance.Send("PTZ", "None");
        }
    }
}