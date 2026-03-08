def map_range(value, in_min, in_max, out_min, out_max):
    return (value - in_min) * (out_max - out_min) / (in_max - in_min) + out_min


def convert_to_box(u, v, object_size=10):
    half_size = object_size / 2
    return [
        u - half_size,
        ((1 - v) - half_size),
        u + half_size,
        ((1 - v) + half_size),
    ]


def parse_time_to_seconds(time_str: str) -> float:
    h, m, s, ms = map(int, time_str.split(":"))
    return h * 3600 + m * 60 + s + ms / 1000.0


def to_signed_angle(angle_0_360: float) -> float:
    return (angle_0_360 + 180) % 360 - 180
