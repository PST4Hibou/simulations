import socket
import numpy as np

lambda_gain = 2.0

def ibvs_ptz(u, v):
    omega_x = -lambda_gain * v
    omega_y = -lambda_gain * u
    return omega_x, omega_y

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("127.0.0.1", 5005))

print("IBVS Python server running...")

while True:
    data, addr = sock.recvfrom(1024)

    u, v = map(float, data.decode().split(","))

    print(u, v)

    omega_x, omega_y = ibvs_ptz(u, v)

    # message = f"{omega_x},{omega_y}"
    # sock.sendto(message.encode(), addr)