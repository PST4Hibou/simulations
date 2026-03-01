import socket

class UDPSocket:
    _instance = None

    def __new__(cls, local_port=None):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
            cls._instance._initialized = False
        return cls._instance

    def __init__(self, local_port=None):
        if getattr(self, "_initialized", False):
            return  # Already initialized

        self.local_port = local_port

        self.remote_ip = None # Fill once receive the first message
        self.remote_port = None

        # UDP socket
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

        # Bind only if local_port is given
        if self.local_port is not None:
            self.sock.bind(("0.0.0.0", self.local_port))

        self._initialized = True

    def send(self, header, message):
        if self.remote_ip is None or self.remote_port is None:
            raise ValueError("Remote IP/port not set")
        payload = f"{header}:{message}"
        self.sock.sendto(payload.encode("utf-8"), (self.remote_ip, self.remote_port))

    def receive(self, buffer_size=1024):
        payload, addr = self.sock.recvfrom(buffer_size)
        self.remote_ip, self.remote_port = addr
        message = payload.decode("utf-8").strip()

        if ":" not in message:
            return None

        header, data = message.split(":", maxsplit=1)
        return header, data

    def close(self):
        self.sock.close()
