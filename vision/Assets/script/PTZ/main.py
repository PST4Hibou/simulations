from PTZ.settings import SETTINGS
from camera.ptz_controller import PTZController
from camera.vendors.hikvision.ds_2dy9250iax_a import DS2DY9250IAXA
from trackers.ibvs_tracker import IBVSTracker
import numpy as np
import socket


if __name__ == "__main__":
    PTZController(
        "main_camera",
        DS2DY9250IAXA,
        host=SETTINGS.PTZ_HOST,
        username=SETTINGS.PTZ_USERNAME,
        password=SETTINGS.PTZ_PASSWORD,
        start_azimuth=SETTINGS.PTZ_START_AZIMUTH,
        end_azimuth=SETTINGS.PTZ_END_AZIMUTH,
        rtsp_port=SETTINGS.PTZ_RTSP_PORT,
        video_channel=SETTINGS.PTZ_VIDEO_CHANNEL,
    )

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
