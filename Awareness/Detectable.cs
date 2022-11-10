using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detectable : MonoBehaviour
{
    public Target target;
    public Vector3 Location;
    public float lastUpdate = -1f;
    public float Level;

    private float undetected = 0f;
    private float partiallyDetected = 1f;
    private float fullyDetected = 2f;

    // Refresh is edge detecting detection level changes
    public bool Refresh(Target currTarget, Vector3 location, float detectionLevel, float detectionBase)
    {
        //Debug.Log(Level);
        float prevDetectionLevel = Level;
        target = currTarget;
        Location = location;
        Level = Mathf.Clamp(Mathf.Max(Level, detectionBase) + detectionLevel, undetected, fullyDetected);
        Debug.Log(Level);
        lastUpdate = Time.time;

        if (prevDetectionLevel < 2f && Level >= 2)
        {
            return true;
        }
        if (prevDetectionLevel < 1f && Level >= 1f)
        {
            return true;
        }
        if (prevDetectionLevel <= 0f && Level >= 0f)
        {
            return true;
        }
        return false;
    }

    public bool Decay(float detectionDecayWait, float detectionDecayExpo)
    {
        float prevDetectionLevel = Level;

        if ((Time.time - lastUpdate) < detectionDecayWait)
        {
            return false;
        }

        Level -= detectionDecayExpo;

        if (prevDetectionLevel >= 2f && Level < 2f){
            return true;
        }

        if (prevDetectionLevel >= 1f && Level < 1f){
            return true;
        }
        return Level <= 0f;
    }
}
