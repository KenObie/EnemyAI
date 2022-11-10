using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyAI))]

public class Awareness : MonoBehaviour
{
    [SerializeField] AnimationCurve PeripheralVision;

    // Detection class to adjust Detection Level/Refresh/Decay Methods
    Dictionary<GameObject, Detectable> DetectableTargets = new Dictionary<GameObject, Detectable>();

    Animator anim;
    EnemyAI enemyAI;
    NavMeshAgent agent;

    // AI Decay System Variables
    public float visionDetection = 1f;
    public float visionExpo = 10f;
    public float hearingDetection = 0f;
    public float hearingExpo = 0.5f;
    public float detectionDecayExpo = 0.1f;
    public float detectionDecayWait = 0.1f;

    Collider coll;
    public GameObject manager;

    void OnCollisionEnter(Collision collision)
    {
        string name = collision.gameObject.name;
        if (name == "Goose" || name == "Character_1")
        {
            manager.GetComponent<Manager>().Lose();
        }
    }


    void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        coll = GetComponent<Collider>();
    }

    void Update()
    {
        List<GameObject> undetected = new List<GameObject>();
        anim.SetFloat("Speed", agent.velocity.magnitude);
        anim.SetFloat("Angle", Vector3.SignedAngle(agent.transform.forward, agent.velocity, Vector3.up)*Mathf.Deg2Rad);
        foreach(var currTarget in DetectableTargets.Keys)
        {
            if (DetectableTargets[currTarget].Decay(detectionDecayWait, detectionDecayExpo * Time.deltaTime))
            {
                if (DetectableTargets[currTarget].Level <= 0f)
                {
                    //Debug.Log(DetectableTargets[currTarget].Level);
                    // enemyAI.LostAwareness();
                    enemyAI.SetIdle();
                    undetected.Add(currTarget);
                } else {
                    Debug.Log("Not going to idle");
                    if (DetectableTargets[currTarget].Level >= 1f)
                    {
                        // enemyAI.LostChase(currTarget);
                        enemyAI.SetFollow(currTarget, currTarget.transform.position);
                    } else {
                        // enemyAI.LostInvestigations(currTarget);
                        Debug.Log("Setting to investigate");
                        enemyAI.SetInvestigate(currTarget, currTarget.transform.position);
                    }
                }
            }
        }
        updateTargets(undetected);
    }

    void updateTargets(List<GameObject> undetected)
    {
        foreach(var t in undetected)
        {
            DetectableTargets.Remove(t);
        }
    }




    public void ReportVision(Target target)
    {
        var currTargetVector = (target.transform.position - enemyAI.VisionStart).normalized;
        //Debug.Log("Vector To Target = " + currTargetVector);
        var reactionTime = Vector3.Dot(currTargetVector, enemyAI.VisionDirection);
        //Debug.Log("Reaction Time = " + reactionTime);
        var awarenessLevel = PeripheralVision.Evaluate(reactionTime) * visionExpo * Time.deltaTime;
        //Debug.Log("Build Rate = " + visionExpo + " TimeDetlta = " + Time.deltaTime);
        //Debug.Log("Peripheral = " + PeripheralVision.Evaluate(reactionTime));
        //Debug.Log("Vision Awareness = " + awarenessLevel);
        if (isDetectable(target.gameObject))
        {
            refreshAwarenessLevel(target, target.transform.position, awarenessLevel, visionDetection, target.gameObject);
        }
    }


    public void ReportSound(GameObject sound, Vector3 location, float decibel)
    {
        var awarenessLevel = decibel * hearingExpo * Time.deltaTime;
        if (isDetectable(sound))
        {
            Debug.Log(awarenessLevel);
            refreshAwarenessLevel(null, location, awarenessLevel, hearingDetection, sound);
        }
    }


    bool isDetectable(GameObject d)
    {
        if (!DetectableTargets.ContainsKey(d))
        {
            DetectableTargets[d] = new Detectable();
            return true;
        }
        return true;
    }

    void refreshAwarenessLevel(Target target, Vector3 location, float awarenessLevel, float detectionBase, GameObject GO)
    {
        var isRefreshed = DetectableTargets[GO].Refresh(target, location, awarenessLevel, detectionBase);
        if (isRefreshed)
        {
            //Debug.Log(DetectableTargets[GO].Level);
            //switch (awarenessLevel)
            switch (DetectableTargets[GO].Level)
            {
                case float awareness when (awareness >= 2f):
                    enemyAI.SetChase(GO, location);
                    break;
                case float awareness when (awareness >= 1f):
                    enemyAI.SetFollow(GO, location);
                    break;
                case float awareness when (awareness >= 0f):
                    // Debug.Log("Setting State to Investigate");
                    enemyAI.SetInvestigate(GO, location);
                    break;
                default:
                    break;
            }
        } else {
            enemyAI.RefreshTargetLocation(location);
        }
    }

}