using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Awareness))]
public class EnemyAI : MonoBehaviour
{

    // AI Parent
    Awareness ai;

    // vision ai
    public float visionConeAngle = 60f;
    public float visionConeRadius = 30f;
    public Color visionConeColor = new Color(1, 0, 0, 0.2f);

    // hearing ai
    public float hearingRadius = 20f;
    public Color hearingRadiusColor = new Color(0, 0, 1, 0.2f);

    // proximity ai
    public float proximityRadius = 3f;
    public Color proximityRadiusColor = new Color(1, 1, 1, 0.2f);

    public float CosVisionCone { get; private set; } = 0f;


    public Vector3 VisionStart => transform.position + new Vector3(0, 1.5f, 0);
    public Vector3 VisionDirection => transform.forward;

    public static float defaultSpeed = 1.0f;
    public static float investigateSpeed = 1.5f;
    public static float followSpeed = 2.5f;
    public static float chaseSpeed = 4.0f;
    private NavMeshAgent navMeshAgent;
    private float enemyIdleTime = 3.0f;

    // for state machine
    public enum EnemyAIState {
        idle,
        waypoints,
        investigate,
        follow,
        chase
    }

    IEnumerator IdleTime(float time)
    {
        navMeshAgent.isStopped = true;
        yield return new WaitForSeconds(time);
        currWaypoint = findClosestWaypoint();
        navMeshAgent.isStopped = false;
        enemyAIState = EnemyAIState.waypoints;
    }


    public EnemyAIState enemyAIState;
    public GameObject[] waypoints;
    public int currWaypoint;

    public Vector3 currTargetLocation;


    void Awake()
    {
        // for state machine
        enemyAIState = EnemyAIState.waypoints;
        
        CosVisionCone = Mathf.Cos(visionConeAngle * Mathf.Deg2Rad);
        ai = GetComponent<Awareness>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        currWaypoint = -1;

        setNextWaypoint();

    }



    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        
        switch (enemyAIState)
        {
            case EnemyAIState.idle:
                StartCoroutine(IdleTime(enemyIdleTime));
                break;
            case EnemyAIState.waypoints:
                navMeshAgent.speed = defaultSpeed;
                if ((navMeshAgent.remainingDistance - navMeshAgent.stoppingDistance < 0.1) && (navMeshAgent.pathPending != true)) {
                    setNextWaypoint();
                }
                break;
            case EnemyAIState.investigate:
                // transform.LookAt(currTargetLocation);
                // StartCoroutine(IdleTime(2.0f));
                navMeshAgent.speed = investigateSpeed;
                navMeshAgent.destination = currTargetLocation;
                break;
            case EnemyAIState.follow:
                navMeshAgent.speed = followSpeed;
                //transform.LookAt(currTargetLocation);
                navMeshAgent.destination = currTargetLocation;
                break;
            case EnemyAIState.chase:
                navMeshAgent.speed = chaseSpeed;
                navMeshAgent.destination = currTargetLocation;
                break;
            default:
                break;
        }
    }

    public void Seen(Target target)
    {
        ai.ReportVision(target);
    }

    public void HeardSound(GameObject target, Vector3 location, float decibel)
    {
        ai.ReportSound(target, location, decibel);
    }

    public void InProximity(Target target)
    {
        ai.ReportVision(target);
    }

    // Methods for update the EnemyAI state machine
    public void SetIdle()
    {
        enemyAIState = EnemyAIState.idle;
    }

    public void SetWayPoints()
    {
        enemyAIState = EnemyAIState.waypoints;
    }

    public void SetInvestigate(GameObject GO, Vector3 location)
    {
        currTargetLocation = location;
        enemyAIState = EnemyAIState.investigate;
    }

    public void SetFollow(GameObject GO, Vector3 location)
    {
        currTargetLocation = GO.transform.position;
        enemyAIState = EnemyAIState.follow;
    }

    public void SetChase(GameObject GO, Vector3 location)
    {
        currTargetLocation = GO.transform.position;
        enemyAIState = EnemyAIState.chase;
    }

    public void RefreshTargetLocation(Vector3 location)
    {
        currTargetLocation = location;
    }

    // public void LostAwareness(GameObject GO)
    // {
    //     SetIdle();
    // }

    // public void LostChase(GameObject GO)
    // {
    //     SetFollow();
    // }

    // public void LostInvestigation(GameObject GO)
    // {
    //     SetInvestigate();
    // }



    //--------------------------------------------------------------
    // start search
    public void Investigate(GameObject detection, Vector3 location)
    {
        navMeshAgent.speed = investigateSpeed;
        navMeshAgent.destination = location;
        // navMeshAgent.isStopped = false;

    }


    // start follow
    public void Follow(GameObject detection, Vector3 location)
    {
        enemyAIState = EnemyAIState.follow;
        navMeshAgent.speed = followSpeed;
        navMeshAgent.destination = location;
        // navMeshAgent.isStopped = false;
    }

    
    // start chase
    public void Chase(GameObject detection, Vector3 location)
    {
        enemyAIState = EnemyAIState.chase;
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.destination = detection.transform.position;
        // navMeshAgent.isStopped = false;
    }

    public void StopChase(GameObject detection)
    {
        Debug.Log("no more chase");

    }

    public void StopFollow(GameObject detection)
    {
        Debug.Log("no more follow");

    }

    public void StopInvestigate()
    {
        Debug.Log("stopping investigation and returning to waypoints");
        currWaypoint = findClosestWaypoint();
        enemyAIState = EnemyAIState.waypoints;
    }

    private void setNextWaypoint()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogError("waypoint length is zero");
        }
        currWaypoint = (currWaypoint + 1) % waypoints.Length;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(waypoints[currWaypoint].transform.position);
    }

    private int findClosestWaypoint()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogError("waypoint length is zero");
        }

        int closestWaypoint = -1;
        var shortestDistance = -1f;

        for (int idx = 0; idx < waypoints.Length; idx++)
        {
            var distance = Vector3.Distance(transform.position, waypoints[idx].transform.position);
            if (shortestDistance < 0)
            {
                shortestDistance = distance;
            }

            if (distance < shortestDistance)
            {
                closestWaypoint = idx;
                shortestDistance = distance;
            }
        }
        return closestWaypoint;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyAI))]
public class EnemyAIEditor : Editor
{
    public void OnSceneGUI()
    {
        var enemyAI = target as EnemyAI;

        
        Handles.color = enemyAI.proximityRadiusColor;
        Handles.DrawSolidDisc(enemyAI.transform.position, Vector3.up, enemyAI.proximityRadius);

        // draw the hearing range
        Handles.color = enemyAI.hearingRadiusColor;
        Handles.DrawSolidDisc(enemyAI.transform.position, Vector3.up, enemyAI.hearingRadius);

        // work out the start point of the vision cone
        Vector3 arch = Mathf.Cos(-enemyAI.visionConeAngle * Mathf.Deg2Rad) * enemyAI.transform.forward +
                             Mathf.Sin(-enemyAI.visionConeAngle * Mathf.Deg2Rad) * enemyAI.transform.right;

        // draw the vision cone
        Handles.color = enemyAI.visionConeColor;
        Handles.DrawSolidArc(enemyAI.transform.position, Vector3.up, arch, enemyAI.visionConeAngle * 2f, enemyAI.visionConeRadius);        
    }
}
#endif // UNITY_EDITOR
