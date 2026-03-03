import matplotlib.pyplot as plt
from collections import deque

class Graph:
    def __init__(self, window_seconds=20):
        self.window_seconds = window_seconds
        self.start_time = None

        self.times = deque()
        self.unity_values = deque()
        self.real_values = deque()
        self.diff_values = deque()

        self.last_unity = None
        self.last_real = None

        plt.ion()
        self.fig, self.ax = plt.subplots()

        self.unity_line, = self.ax.plot([], [], label="Unity")
        self.real_line, = self.ax.plot([], [], label="Real")
        self.diff_line, = self.ax.plot([], [], label="Diff", linestyle="--")

        self.ax.set_xlabel("Time (s)")
        self.ax.set_ylabel("PAN (deg)")
        self.ax.set_ylim(-180, 180)
        self.ax.legend()
        self.ax.grid(True)

    def stop(self):
        plt.ioff()
        try:
            if self.fig is not None:
                plt.close(self.fig)
                self.fig = None
        except Exception:
            pass

    def update(self, timestamp, pan_unity=None, pan_real=None):
        if self.start_time is None:
            self.start_time = timestamp

        relative_time = timestamp - self.start_time

        # Update last known values if new ones arrived
        if pan_unity is not None:
            self.last_unity = pan_unity

        if pan_real is not None:
            self.last_real = pan_real

        # If we don't have both yet, don't plot
        if self.last_unity is None or self.last_real is None:
            return

        # diff = self.last_unity - self.last_real
        diff = (self.last_unity - self.last_real + 180) % 360 - 180

        self.times.append(relative_time)
        self.unity_values.append(self.last_unity)
        self.real_values.append(self.last_real)
        self.diff_values.append(diff)

        # Remove old values outside window
        while self.times and (self.times[-1] - self.times[0] > self.window_seconds):
            self.times.popleft()
            self.unity_values.popleft()
            self.real_values.popleft()
            self.diff_values.popleft()

        # Update plot data
        self.unity_line.set_data(self.times, self.unity_values)
        self.real_line.set_data(self.times, self.real_values)
        self.diff_line.set_data(self.times, self.diff_values)

        # Only update X axis scrolling
        self.ax.set_xlim(
            max(0, relative_time - self.window_seconds),
            relative_time
        )

        self.fig.canvas.draw()
        self.fig.canvas.flush_events()