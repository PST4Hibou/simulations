from dataclasses import dataclass
from dotenv import load_dotenv

import logging
import shutil
import os


current_file_path = os.path.abspath(__file__)
script_dir = os.path.dirname(current_file_path)
project_root = os.path.abspath(os.path.join(script_dir, ".."))

# Paths
source_file = os.path.join(project_root, ".env.exemple")
target_file = os.path.join(project_root, "../.env")

# Copy .env if it does not exist
if not os.path.exists(target_file) and os.path.exists(source_file):
    shutil.copy2(source_file, target_file)
    logging.info(f"Copied {source_file} → {target_file}")

if not load_dotenv():
    raise FileNotFoundError("Failed to load .env file.")


@dataclass
class Settings:
    PTZ_USERNAME: str
    PTZ_PASSWORD: str
    PTZ_HOST: str
    PTZ_VIDEO_CHANNEL: int
    PTZ_RTSP_PORT: int
    PTZ_START_AZIMUTH: int
    PTZ_END_AZIMUTH: int



def parse_list(value: str):
    """Split a comma-separated string and strip whitespace."""
    return [v.strip() for v in value.split(",") if v.strip()]


def parse_bool(value: str) -> bool:
    """Parse a boolean from string (True/False, yes/no)."""
    return str(value).strip().lower() in ("true", "1", "yes")


try:
    if Settings.CV_VIDEO_PLAYBACK and (
        Settings.AUDIO_RADAR or Settings.AUDIO_ENERGY_SPECTRUM
    ):
        logging.warning(
            "Both CV video and audio visualization are enabled. Disabling CV video."
        )
        Settings.CV_VIDEO_PLAYBACK = False

    SETTINGS = Settings(
        PTZ_USERNAME=os.getenv("PTZ_USERNAME"),
        PTZ_PASSWORD=os.getenv("PTZ_PASSWORD"),
        PTZ_HOST=os.getenv("PTZ_HOST"),
        PTZ_VIDEO_CHANNEL=int(os.getenv("PTZ_VIDEO_CHANNEL")),
        PTZ_RTSP_PORT=int(os.getenv("PTZ_RTSP_PORT")),
        PTZ_START_AZIMUTH=int(os.getenv("PTZ_START_AZIMUTH")),
        PTZ_END_AZIMUTH=int(os.getenv("PTZ_END_AZIMUTH")),
    )


except TypeError as e:
    raise ValueError(f"Invalid value in .env: {e}. Please check the .env file.")
