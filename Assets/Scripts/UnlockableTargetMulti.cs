using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnlockableTargetMulti : MonoBehaviour
{

    [SerializeField] private bool locked = true;
    [SerializeField] private int numberOfKeys = 1;
    [SerializeField] private GameObject keyPrefab;

    private int keysRemaining;
    private Dictionary<int, GameObject> keys = new Dictionary<int, GameObject>();

    public bool Locked { get => locked; }
    public int KeysRemaining { get => keysRemaining;}


    public void Unlock(GameObject key)
    {
        if(keys.ContainsKey(key.GetInstanceID()))
        {
            if (keysRemaining > 0) keysRemaining--;
            if (keysRemaining == 0) locked = false;
            keys.Remove(key.GetInstanceID());
        }
    }

    public void SpawnKeys()
    {
        if (keyPrefab == null) return;
        

        //remove any existing keys
        foreach (GameObject go in keys.Values)
        {
            Destroy(go);
        }

        keys.Clear();
        //reset number of keys (+1 as maximum is exclusive)
        keysRemaining = Random.Range(0, numberOfKeys+1);
        
        //respawn keys to the number specified
        for(int i = 0; i < keysRemaining; i++)
        {
            GameObject go = Instantiate(keyPrefab, this.gameObject.transform.parent);
            keys.Add(go.GetInstanceID(), go);

            if(!Helper.SetRandomLocalPositionUnRestricted(go.transform, 0.5f)) Debug.Log("Failed to randomly place key");

            go.GetComponent<Collectible_UnlockMulti>().objectToUnlock = this;
            //go.GetComponent<Collectible_UnlockMulti>().Reset();
        }

        locked = true;

        if (keysRemaining == 0) locked = false;
    }

    private void Update()
    {
        //this.GetComponent<Renderer>().enabled = locked;

        //foreach (Collider col in this.GetComponents<Collider>())
        //{
        //    col.enabled = locked;
        //}
    }

    public GameObject GetNearestKey(Vector3 position)
    {
        GameObject closestKey = null;
        float distance = 0f;

        if(keys.Count > 0)
        {
            closestKey = keys.ElementAt(0).Value;
            distance = Vector3.Distance(closestKey.transform.position, position);
        }
        else
        {
            return null;
        }

        foreach(GameObject go in keys.Values)
        {
            float tempDistance;
            //skip if already set to the current
            if (closestKey.GetInstanceID() == go.GetInstanceID()) continue;

            tempDistance = Vector3.Distance(go.transform.position, position);
            if(tempDistance < distance)
            {
                distance = tempDistance;
                closestKey = go;
            }

        }

        return closestKey;
    }
}
