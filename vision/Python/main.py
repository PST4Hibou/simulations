from src.helpers.math import convert_to_box, parse_time_to_seconds, to_signed_angle
from src.camera.vendors.hikvision.ds_2dy9250iax_a import DS2DY9250IAXA
from src.camera.ptz_controller import PTZController
from src.trackers.ibvs_tracker import IBVSTracker
from src.network.socket import UDPSocket
from src.settings import SETTINGS
from src.gui.graph import Graph
from time import sleep, time

PORT = 5005

def handle_ptz_data(data: str):
    try:
        if data == "None":
            controls = tracker.update(None)
        else:
            u, v = map(float, data.split(","))
            box = convert_to_box(u, v)  # Must be converted to box to simulate Yolo box
            controls = tracker.update(box)

            graph.update_box_center(time(), u, v)

        if controls is not None:
            pan_vel, tilt_vel, zoom_vel = controls


            if pan_vel == 0 and tilt_vel == 0:
                current_pan_vel, current_tilt_vel = PTZController(
                    "main_camera"
                ).get_speed()
                if current_pan_vel != 0 or current_tilt_vel != 0:
                    PTZController("main_camera").stop_continuous()
                message = f"{0},{0}"
            else:
                PTZController("main_camera").start_continuous(
                    pan_speed=-pan_vel,
                    tilt_speed=tilt_vel,
                    clamp=True,
                )
                message = f"{pan_vel},{tilt_vel}"
            sock.send("PTZ", message)

    except Exception as e:
        print("Error processing PTZ data:", e)


def handle_ptz_rotation_data(data: str):
        parts = data.split(",")

        time_str = parts[0]
        pan, tilt = map(float, parts[1:])

        timestamp = parse_time_to_seconds(time_str)

        PTZController("main_camera").get_status(force_update=True)
        pan_real, tilt_real, _ = PTZController("main_camera").get_absolute_ptz_position()
        graph.update_pan(timestamp, pan, to_signed_angle(pan_real))
        graph.update_tilt(timestamp, -tilt, tilt_real)
        graph.update()


if __name__ == "__main__":
    sock = UDPSocket(local_port=PORT)
    print(f"IBVS Python server running on 127.0.0.1 :{PORT}")

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

    graph = Graph(window_seconds=30)

    tracker = IBVSTracker()

    PTZController("main_camera").set_absolute_ptz_position(pan=1, tilt=1, zoom=1)
    sleep(2)

    try:
        while True:
            header, data = sock.receive()

            if header == "PTZ":
                handle_ptz_data(data)
            elif header == "PTZ_Rotation":
                handle_ptz_rotation_data(data)
            else:
                print(f"Unknown header: {header}")


    except KeyboardInterrupt:
        print("\nShutting down server...")

    finally:
        PTZController("main_camera").stop_continuous()
        PTZController("main_camera").release_stream()
        graph.stop()
        PTZController.remove()
        sock.send("PTZ", f"{0},{0}")
        sock.close()
