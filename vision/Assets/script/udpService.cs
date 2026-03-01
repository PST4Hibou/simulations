using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Net;
using System;

namespace script
{
    public class UdpService : IDisposable
    {
        private UdpClient _client;
        private IPEndPoint _remoteEndPoint;

        // Dictionary of queues by header
        private Dictionary<string, Queue<string>> _headerQueues = new Dictionary<string, Queue<string>>();

        public UdpService(string ip, int port)
        {
            _client = new UdpClient();
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            // Begin async receive
            _client.BeginReceive(ReceiveCallback, null);
        }

        /// <summary>
        /// Send a message to the remote endpoint
        /// </summary>
        public void Send(string message)
        {
            if (_client == null) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _client.Send(data, data.Length, _remoteEndPoint);
            }
            catch (Exception e)
            {
                Debug.LogError("UDP Send Error: " + e.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                byte[] data = _client.EndReceive(ar, ref _remoteEndPoint);
                string message = Encoding.UTF8.GetString(data).Trim();

                // Expecting format: "HEADER:payload"
                string header = "DEFAULT";
                string payload = message;

                int sepIndex = message.IndexOf(':');
                if (sepIndex > 0)
                {
                    header = message.Substring(0, sepIndex);
                    payload = message.Substring(sepIndex + 1);
                }

                lock (_headerQueues)
                {
                    if (!_headerQueues.ContainsKey(header))
                        _headerQueues[header] = new Queue<string>();

                    _headerQueues[header].Enqueue(payload);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("UDP Receive Error: " + e.Message);
            }
            finally
            {
                // Continue receiving
                _client.BeginReceive(ReceiveCallback, null);
            }
        }

        /// <summary>
        /// Try to dequeue a message from a specific header queue
        /// </summary>
        public bool TryGetMessage(string header, out string message)
        {
            lock (_headerQueues)
            {
                if (_headerQueues.ContainsKey(header) && _headerQueues[header].Count > 0)
                {
                    message = _headerQueues[header].Dequeue();
                    return true;
                }
            }

            message = null;
            return false;
        }

        public void Dispose()
        {
            _client?.Close();
            _client = null;
        }
    }
}