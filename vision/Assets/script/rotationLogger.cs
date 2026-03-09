using UnityEngine;
using System.IO;
using System.Text;

namespace script
{
    public class RotationCsvLogger : MonoBehaviour
    {
        public bool enable = true;

        [Header("References")]
        public Transform tiltNode;   // Assign Tilt_node here

        private string _filePath;
        private StringBuilder _builder = new StringBuilder();

        void Start()
        {
            if (!enable) return;

            _filePath = Path.Combine(Application.persistentDataPath, "rotation_log.csv");
            Debug.Log("Logging to: " + _filePath);

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "Time,PanY,TiltX\n");
            else
                File.AppendAllText(_filePath, "\n---- New Session ----\n");

            InvokeRepeating(nameof(LogRotation), 0f, 0.1f); // 10 Hz logging
        }

        void LogRotation()
        {
            if (tiltNode == null) return;

            string time = System.DateTime.Now.ToString("HH:mm:ss:fff");

            // ---- PAN (signed)
            float pan = transform.eulerAngles.y;
            if (pan > 180f)
                pan -= 360f;

            // ---- TILT (signed, local)
            float tilt = tiltNode.localEulerAngles.x;
            if (tilt > 180f)
                tilt -= 360f;

            _builder.Clear();
            _builder.Append(time).Append(",");
            _builder.Append(pan.ToString("F3")).Append(",");
            _builder.Append(tilt.ToString("F3")).Append("\n");
            
            UdpManager.Instance.Send("PTZ_Rotation", _builder.ToString());
            // File.AppendAllText(_filePath, _builder.ToString());
        }
    }
}