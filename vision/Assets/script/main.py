from trackers.ibvs_tracker import IBVSTracker
import numpy as np
import socket

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("127.0.0.1", 5005))

print("IBVS Python server running...")

tracker = IBVSTracker()

def convert_to_box(u, v, object_size= 10):
    return [u - object_size / 2, v - object_size / 2, u + object_size / 2, v + object_size / 2]

while True:
    data, addr = sock.recvfrom(1024)

    if data.decode() == "None":
        controls = tracker.update(None)
    else:
        u, v = map(float, data.decode().split(","))
        controls = tracker.update(convert_to_box(u, v))

    if controls is not None:
        pan_vel, tilt_vel, zoom_vel = controls
        message = f"{pan_vel},{tilt_vel}"
        sock.sendto(message.encode(), addr)
