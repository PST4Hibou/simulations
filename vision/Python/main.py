from src.trackers.ibvs_tracker import IBVSTracker
import socket

HOST = "127.0.0.1"
PORT = 5005


def convert_to_box(u, v, object_size=10):
    half_size = object_size / 2
    return [
        u - half_size,
        v - half_size,
        u + half_size,
        v + half_size,
    ]


def handle_ptz_data(data: str, tracker, sock, addr):
    try:
        if data == "None":
            controls = tracker.update(None)
        else:
            u, v = map(float, data.split(","))
            controls = tracker.update(convert_to_box(u, v))

        if controls is not None:
            pan_vel, tilt_vel, zoom_vel = controls

            # Invert tilt if Unity coordinate system differs
            message = f"PTZ:{pan_vel},{-tilt_vel}"
            sock.sendto(message.encode("utf-8"), addr)

    except Exception as e:
        print("Error processing PTZ data:", e)


def main():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind((HOST, PORT))

    print(f"IBVS Python server running on {HOST}:{PORT}")

    tracker = IBVSTracker()

    try:
        while True:
            block, addr = sock.recvfrom(1024)

            message = block.decode("utf-8").strip()
            print("Received:", message)

            # Validate format
            if ":" not in message:
                print("Invalid packet format")
                continue

            header, data = message.split(":", maxsplit=1)

            if header == "PTZ":
                handle_ptz_data(data, tracker, sock, addr)

    except KeyboardInterrupt:
        print("\nShutting down server...")

    finally:
        sock.close()


if __name__ == "__main__":
    main()
