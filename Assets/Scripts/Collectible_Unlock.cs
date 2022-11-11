using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Unlock : MonoBehaviour
{
    [SerializeField] private UnlockableObject objectToUnlock;

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        Debug.Log("Reset Called", this.gameObject);
        objectToUnlock.RegisterKey(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            objectToUnlock.Unlock(this.gameObject);
            this.gameObject.SetActive(false);
        }
    }
}
