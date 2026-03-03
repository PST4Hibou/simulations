import matplotlib.pyplot as plt
from collections import deque


class Graph:
    def __init__(self, window_seconds=20):
        self.window_seconds = window_seconds

        # ----- PAN -----
        self.start_time_pan = None
        self.times_pan = deque()
        self.unity_pan = deque()
        self.real_pan = deque()
        self.diff_pan = deque()
        self.last_unity_pan = None
        self.last_real_pan = None

        # ----- TILT -----
        self.start_time_tilt = None
        self.times_tilt = deque()
        self.unity_tilt = deque()
        self.real_tilt = deque()
        self.diff_tilt = deque()
        self.last_unity_tilt = None
        self.last_real_tilt = None

        # ----- BOX -----
        self.start_time_box = None
        self.times_box = deque()
        self.box = deque()
        self.last_box = None

        plt.ion()

        # Create subplots FIRST
        self.fig, (self.ax_pan, self.ax_tilt, self.ax_box) = plt.subplots(
            3, 1, sharex=False, figsize=(14, 10)
        )

        # ----- PAN plot -----
        self.unity_line_pan, = self.ax_pan.plot([], [], label="Unity PAN")
        self.real_line_pan, = self.ax_pan.plot([], [], label="Real PAN")
        self.diff_line_pan, = self.ax_pan.plot([], [], linestyle="--", label="Diff PAN")
        self.ax_pan.set_ylabel("PAN (deg)")
        self.ax_pan.set_ylim(-180, 180)
        self.ax_pan.legend()
        self.ax_pan.grid(True)

        # ----- TILT plot -----
        self.unity_line_tilt, = self.ax_tilt.plot([], [], label="Unity TILT")
        self.real_line_tilt, = self.ax_tilt.plot([], [], label="Real TILT")
        self.diff_line_tilt, = self.ax_tilt.plot([], [], linestyle="--", label="Diff TILT")
        self.ax_tilt.set_ylabel("TILT (deg)")
        self.ax_tilt.set_ylim(-95, 50)
        self.ax_tilt.legend()
        self.ax_tilt.grid(True)

        # ----- BOX plot -----
        self.box_line, = self.ax_box.plot([], [], label="Box Center Distance")
        self.ax_box.set_xlabel("Time (s)")
        self.ax_box.set_ylabel("Distance from Center")
        self.ax_box.set_ylim(0, 0.75)
        self.ax_box.legend()
        self.ax_box.grid(True)

    def stop(self):
        plt.ioff()
        try:
            if self.fig is not None:
                plt.close(self.fig)
                self.fig = None
        except Exception:
            pass

    # -------------------- PAN --------------------
    def update_pan(self, timestamp, pan_unity=None, pan_real=None):
        if self.start_time_pan is None:
            self.start_time_pan = timestamp

        relative_time = timestamp - self.start_time_pan

        if pan_unity is not None:
            self.last_unity_pan = pan_unity
        if pan_real is not None:
            self.last_real_pan = pan_real

        if self.last_unity_pan is None or self.last_real_pan is None:
            return

        diff = (self.last_unity_pan - self.last_real_pan + 180) % 360 - 180

        self.times_pan.append(relative_time)
        self.unity_pan.append(self.last_unity_pan)
        self.real_pan.append(self.last_real_pan)
        self.diff_pan.append(diff)

        while self.times_pan and (self.times_pan[-1] - self.times_pan[0] > self.window_seconds):
            self.times_pan.popleft()
            self.unity_pan.popleft()
            self.real_pan.popleft()
            self.diff_pan.popleft()

        self.unity_line_pan.set_data(self.times_pan, self.unity_pan)
        self.real_line_pan.set_data(self.times_pan, self.real_pan)
        self.diff_line_pan.set_data(self.times_pan, self.diff_pan)

        self.ax_pan.set_xlim(
            max(0, relative_time - self.window_seconds),
            relative_time
        )

        # self.fig.canvas.draw()
        # self.fig.canvas.flush_events()

    # -------------------- TILT --------------------
    def update_tilt(self, timestamp, tilt_unity=None, tilt_real=None):
        if self.start_time_tilt is None:
            self.start_time_tilt = timestamp

        relative_time = timestamp - self.start_time_tilt

        if tilt_unity is not None:
            self.last_unity_tilt = tilt_unity
        if tilt_real is not None:
            self.last_real_tilt = tilt_real

        if self.last_unity_tilt is None or self.last_real_tilt is None:
            return

        diff = (self.last_unity_tilt - self.last_real_tilt + 180) % 360 - 180

        self.times_tilt.append(relative_time)
        self.unity_tilt.append(self.last_unity_tilt)
        self.real_tilt.append(self.last_real_tilt)
        self.diff_tilt.append(diff)

        while self.times_tilt and (self.times_tilt[-1] - self.times_tilt[0] > self.window_seconds):
            self.times_tilt.popleft()
            self.unity_tilt.popleft()
            self.real_tilt.popleft()
            self.diff_tilt.popleft()

        self.unity_line_tilt.set_data(self.times_tilt, self.unity_tilt)
        self.real_line_tilt.set_data(self.times_tilt, self.real_tilt)
        self.diff_line_tilt.set_data(self.times_tilt, self.diff_tilt)

        self.ax_tilt.set_xlim(
            max(0, relative_time - self.window_seconds),
            relative_time
        )

    def update_box_center(self, timestamp, u, v):
        if self.start_time_box is None:
            self.start_time_box = timestamp

        relative_time = timestamp - self.start_time_box

        # Compute Euclidean distance from center (0.5, 0.5)
        dist = ((u - 0.5) ** 2 + (v - 0.5) ** 2) ** 0.5

        self.last_box = dist

        self.times_box.append(relative_time)
        self.box.append(dist)

        # Remove old values outside window
        while self.times_box and (self.times_box[-1] - self.times_box[0] > self.window_seconds):
            self.times_box.popleft()
            self.box.popleft()

        # Update plot
        self.box_line.set_data(self.times_box, self.box)

        self.ax_box.set_xlim(
            max(0, relative_time - self.window_seconds),
            relative_time
        )

    def update(self):
        self.fig.canvas.draw_idle()
        self.fig.canvas.flush_events()
