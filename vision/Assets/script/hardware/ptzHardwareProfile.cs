using UnityEngine;

[CreateAssetMenu(fileName = "PtzHardwareProfile", menuName = "PTZ/Hardware Profile")]
public class PtzHardwareProfile : ScriptableObject
{
    [Header("Pan Speeds (index = |virtual| 0..10)")]
    public float[] panSpeeds = new float[11];

    [Header("Tilt Speeds (index = |virtual| 0..10)")]
    public float[] tiltSpeeds = new float[11];
    
    [Header("PTZ Limits")]
    public float tiltMinAngle = -90f;
    public float tiltMaxAngl = 40f;
    public float acceleration = 200f;

    public float GetTiltSpeed(int virtualTilt)
    {
        int index = Mathf.Clamp(Mathf.Abs(virtualTilt), 0, tiltSpeeds.Length - 1);
        float speed = tiltSpeeds[index];
        if (virtualTilt < 0) speed *= -1f; // negative direction
        return speed;
    }

    public float GetPanSpeed(int virtualPan)
    {
        int index = Mathf.Clamp(Mathf.Abs(virtualPan), 0, panSpeeds.Length - 1);
        float speed = panSpeeds[index];
        if (virtualPan < 0) speed *= -1f; // negative direction
        return speed;
    }
    
}