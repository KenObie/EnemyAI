using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class Sound : MonoBehaviour
{
    EnemyAI enemyAI;

    void OnDestroy()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Remove(this);
        }
    }

    void Start()
    {
        SoundManager.Instance.Add(this);
        enemyAI = GetComponent<EnemyAI>();
    }

    void update()
    {

    }

    public void SoundDetected(GameObject target, Vector3 location, float decibel)
    {
        if (Vector3.Distance(location, enemyAI.transform.position) <= enemyAI.hearingRadius)
        {
            enemyAI.HeardSound(target, location, decibel);
        }
        return;
    }
}