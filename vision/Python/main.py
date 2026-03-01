from src.trackers.ibvs_tracker import IBVSTracker
from src.network.socket import UDPSocket

PORT = 5005


def convert_to_box(u, v, object_size=10):
    half_size = object_size / 2
    return [
        u - half_size,
        v - half_size,
        u + half_size,
        v + half_size,
    ]


def handle_ptz_data(data: str, tracker, sock):
    try:
        if data == "None":
            controls = tracker.update(None)
        else:
            u, v = map(float, data.split(","))
            controls = tracker.update(convert_to_box(u, v))

        if controls is not None:
            pan_vel, tilt_vel, zoom_vel = controls

            # Invert tilt if Unity coordinate system differs
            message = f"{pan_vel},{-tilt_vel}"
            # sock.sendto(message.encode("utf-8"))
            sock.send("PTZ", message)

    except Exception as e:
        print("Error processing PTZ data:", e)


if __name__ == "__main__":
    sock = UDPSocket(local_port=PORT)
    print(f"IBVS Python server running on 127.0.0.1 :{PORT}")

    tracker = IBVSTracker()

    try:
        while True:
            header, data = sock.receive()

            if header == "PTZ":
                handle_ptz_data(data, tracker, sock)

    except KeyboardInterrupt:
        print("\nShutting down server...")

    finally:
        sock.close()
