using UnityEngine;

namespace script
{
    public class PtzBase : MonoBehaviour
    {

        [Header("Hardware")]
        public PtzHardwareProfile hardware;
        
        private float _currentPanSpeed;
        private float _currentTiltSpeed;

        private int _commandedPan; // last commanded velocity (-10..10)
        private int _commandedTilt;

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
            ApplyPtzVelocity(_commandedPan, _commandedTilt);
        }

        private void ApplyPtzVelocity(int virtualPan, int virtualTilt)
        {
            if (hardware == null)
            {
                Debug.LogError("No hardware profile assigned!");
                return;
            }
            
            // Clamp input
            virtualPan = Mathf.Clamp(virtualPan, -10, 10);
            virtualTilt = Mathf.Clamp(virtualTilt, -10, 10);
            
            float targetPanSpeed = hardware.GetPanSpeed(virtualPan);
            float targetTiltSpeed = hardware.GetTiltSpeed(virtualTilt);

            // Smooth acceleration
            _currentPanSpeed = Mathf.MoveTowards(
                _currentPanSpeed,
                targetPanSpeed,
                hardware.acceleration * Time.deltaTime);

            _currentTiltSpeed = Mathf.MoveTowards(
                _currentTiltSpeed,
                targetTiltSpeed,
                hardware.acceleration * Time.deltaTime);

            // ----------------------
            // PAN (endless)
            // ----------------------
            float panDelta = _currentPanSpeed * Time.deltaTime;
            transform.Rotate(0f, -panDelta, 0f, Space.Self);

            // ----------------------
            // TILT (limited)
            // ----------------------
            float tiltDelta = _currentTiltSpeed * Time.deltaTime;

            float currentTilt = tiltNode.localEulerAngles.x;
            if (currentTilt > 180f)
                currentTilt -= 360f;

            float newTilt = currentTilt - tiltDelta;
            newTilt = Mathf.Clamp(newTilt, hardware.tiltMaxAngl * -1, hardware.tiltMinAngle * -1);

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
                    if (int.TryParse(values[0], out int omegaX) &&
                        int.TryParse(values[1], out int omegaY))
                    {
                        _commandedPan = omegaX;
                        _commandedTilt = omegaY;
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