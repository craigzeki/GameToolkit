using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockableObject : MonoBehaviour
{


    [SerializeField] private Dictionary<int, bool> keys = new Dictionary<int, bool>();
    [SerializeField] private bool locked = true;

    public bool Locked { get => locked; }

    public void RegisterKey(GameObject key)
    {
        int instanceId = key.GetInstanceID();
        if (!keys.ContainsKey(instanceId)) keys.Add(instanceId, false);
    }

    public void Unlock(GameObject key)
    {
        int instanceId = key.GetInstanceID();
        int unlockCount = 0;

        if (keys.ContainsKey(instanceId)) keys[instanceId] = true;

        foreach(bool lockStatus in keys.Values)
        {
            if (lockStatus == true) unlockCount++;
        }

        if(unlockCount == keys.Count)
        {
            locked = false;
        }

    }

    public void Reset()
    {
        keys.Clear();
        locked = true;
    }

    private void Update()
    {
        this.GetComponent<Renderer>().enabled = locked;
        this.GetComponent<Collider>().enabled = locked;
        
    }
}
