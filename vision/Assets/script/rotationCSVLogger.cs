using UnityEngine;
using System.IO;

namespace script
{
    public class RotationCsvLogger : MonoBehaviour
    {
        string _filePath;

        void Start()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "rotation_log.csv");

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "Time,X,Y,Z\n");

            InvokeRepeating(nameof(LogRotation), 0f, 0.1f); // every 0.1 sec
        }

        void LogRotation()
        {
            // float time = Time.time;
            string time = System.DateTime.Now.ToString("HH:mm:ss:fff");
            Vector3 rot = transform.eulerAngles;

            // string line = $"{time:F3},{rot.x:F3},{rot.y:F3},{rot.z:F3}\n";
            string line = $"{time},{rot.x:F3},{rot.y:F3},{rot.z:F3}\n";
            File.AppendAllText(_filePath, line);
        }
    }   
}