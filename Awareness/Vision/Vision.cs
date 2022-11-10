using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(EnemyAI))]
public class Vision : MonoBehaviour
{

    public LayerMask layerMask = ~0;
    EnemyAI enemyAI;

    // Start is called before the first frame update
    void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
       
    }

    // Update is called once per frame
    void Update()
    {
        // on each update call -> check each vision for both player characters
        
        // get total target count
        var targetCount = TargetManager.Instance.Targets.Count;
        for (int idx = 0; idx < targetCount; idx++)
        {
            var currentTarget = TargetManager.Instance.Targets[idx];
         

            // don't want to detect ourselves
            if (currentTarget.gameObject == gameObject)
            {
                Debug.Log("Detecting Ourselves? Why?");
                continue;
            }

            // if (Vector3.Distance(enemyAI.VisionStart, currentTarget.transform.position) <= enemyAI.proximityRadius)
            // {
            //     enemyAI.InProximity(currentTarget);
            //     break;
            // }

            // get vector of currentTarget
            var currentTargetVector = (currentTarget.transform.position + new Vector3(0,0.25f,0)) - enemyAI.VisionStart;
            // var currentTargetVectorTest = currentTarget.transform.position - enemyAI.transform.position;

            // determine if target is in radius
            if (currentTargetVector.magnitude > enemyAI.visionConeRadius)
            {
                //Debug.Log("Target not in vision radius");
                continue;
            }

            // check cos of angle to cos of target using dot product of vector of cone to vector to current target
            // determine if target is in vision cone
            if (Vector3.Dot(currentTargetVector.normalized, enemyAI.transform.forward) < enemyAI.CosVisionCone)
            {
                //Debug.Log("Target Not in vision cone");
                continue;
            }

            // final raycast check
            RaycastHit hitResult;

            Debug.DrawRay(enemyAI.VisionStart, currentTargetVector, Color.red);

            if (Physics.Raycast(enemyAI.VisionStart, currentTargetVector, out hitResult, 
                                enemyAI.visionConeRadius, layerMask, QueryTriggerInteraction.Collide))
            {
                if (hitResult.collider.GetComponentInParent<Target>() == currentTarget)
                {
                    enemyAI.Seen(currentTarget);
                }
            }            
        }
       
    }
}
