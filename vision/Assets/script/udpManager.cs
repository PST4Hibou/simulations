using UnityEngine;

namespace script
{
    public class UdpManager : MonoBehaviour
    {
        public static UdpManager Instance;

        public string ip = "127.0.0.1";
        public int port = 5005;

        private UdpService _udp;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _udp = new UdpService(ip, port);
        }

        public bool TryGetMessage(string header, out string message)
        {
            if (_udp == null)
            {
                message = null;
                return false;
            }

            return _udp.TryGetMessage(header, out message);
        }

        public void Send(string header, string data)
        {
            if (_udp == null)
            {
                Debug.LogWarning("UdpService not initialized!");
                return;
            }

            string message = $"{header}:{data}";
            _udp.Send(message);
        }

        void OnDestroy()
        {
            _udp?.Dispose();
        }
    }
}