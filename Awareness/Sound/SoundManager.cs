using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    jump,
    footstep,
    throwable
}


public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance
    {
        get;
        private set;
    } = null;

    public List<Sound> SoundTargets
    {
        get;
        private set;
    } = new List<Sound>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Cannot have more than 1 sound manager");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void Add(Sound sound)
    {
        SoundTargets.Add(sound);
    }

    public void Remove(Sound sound)
    {
        SoundTargets.Remove(sound);
    }


    public void EmitSound(GameObject sound, Vector3 location, float decibel)
    {
        for (int idx = 0; idx < SoundTargets.Count; idx++)
        {
            SoundTargets[idx].SoundDetected(sound, location, decibel);
        }
    }
}