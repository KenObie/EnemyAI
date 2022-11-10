using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{

    public static TargetManager Instance
    {
        get;
        private set;
    } = null;

    public List<Target> Targets
    {
        get;
        private set;
    } = new List<Target>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("cannot have more than 1 target manager");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Add(Target target)
    {
        Targets.Add(target);
    }

    public void Remove(Target target)
    {
        Targets.Remove(target);
    }
}