from trackers.ibvs_tracker import IBVSTracker
import numpy as np
import socket

lambda_gain = 2.0


def ibvs_ptz(u, v):
    omega_x = -lambda_gain * v
    omega_y = -lambda_gain * u
    return omega_x, omega_y


sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("127.0.0.1", 5005))

print("IBVS Python server running...")

tracker = IBVSTracker()

while True:
    data, addr = sock.recvfrom(1024)

    u, v = map(float, data.decode().split(","))

    print(u, v)

    u = u - 5
    v = v - 5

    controls = tracker.update([u, v, u + 10, v + 10])

    # controls = tracker.update(best_box)

    if controls is not None:
        pan_vel, tilt_vel, zoom_vel = controls

        message = f"{pan_vel},{tilt_vel}"
        
        # if pan_vel == 0 and tilt_vel == 0:
        #     current_pan_vel, current_tilt_vel = PTZController(
        #         "main_camera"
        #     ).get_speed()
        #     if current_pan_vel != 0 or current_tilt_vel != 0:
        #         PTZController("main_camera").stop_continuous()
        # else:
        sock.sendto(message.encode(), addr)
            
            # PTZController("main_camera").start_continuous(
            #     pan_speed=-pan_vel,
            #     tilt_speed=tilt_vel,
            #     clamp=True,
            # )

    # omega_x, omega_y = ibvs_ptz(u, v)

    
