<img src="https://avatars.githubusercontent.com/u/232561603?s=200&v=4" width="100px" align="left">

### `Hibou Simulation`

</br>

Simulate vision and audio (not implemented yet) in Unity.

# Vision simulation

Vision simulation aims to simulate PTZ camera in order to test different camera mouvement tracking algorithm.

**Project architecture:**

- **Unity** (/vision)
    - `PTZ_base`: Simulate real camera hardware
    - `Drone`: Simulate real drone mouve

- **Python** (/vision/Python)
    - `main.py`: Receive drone position in real-time through and return camera command.

Internally `main.py` use the same python code as in [Hibou-Server](https://github.com/PST4Hibou/Hibou-Server), it could
also move the real camera as long as the virtual one.

### Run vision simulation

1. First clone the repository and open the `vision` folder in Unity.
2. Check project default settings (see: Vision simulation configuration)
3. In `/vision/Python` run the main.py with `uv run main.py`
4. Start the Unity project

### Supported Hardware

In order to make this project usefull, it must fit real camera hardware for reliable results and tracking.

Hardware configuration is in: `/vision/Assets/PtzHardware`

New camera can be easily added by creating a new as long you know technical details.

Today following PTZ cameras are supported:

- Hikevision `DS_2DY9250IAX_A`

### Vision Simulation Configuration

#### Unity

##### PTZ Base (Script)

| Parameter   | Comment                                               |
|-------------|-------------------------------------------------------|
| Hardware    | Select the camera you want to simulate: Movement only |
| Virtual FPS | Set the FPS of the virtual camera                     |

##### Rotation CSV Logger (Script)

| Parameter | Comment                                   |
|-----------|-------------------------------------------|
| Enable    | Enable or disable the rotation CSV logger |

#### Python