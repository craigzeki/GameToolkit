using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_UnlockMulti : MonoBehaviour
{
    [SerializeField] public UnlockableTargetMulti objectToUnlock;

    //private int originalLayer;

    private void Awake()
    {
        //originalLayer = this.gameObject.layer;
    }

    private void Start()
    {
        //Reset();
    }

    //public void Reset()
    //{
    //    if (objectToUnlock == null) return;
    //    this.gameObject.layer = originalLayer;
        
    //}

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            objectToUnlock.Unlock(this.gameObject);
            //this.gameObject.layer = LayerMask.NameToLayer("Default");
            //this.gameObject.SetActive(false);
            Destroy(this.gameObject);
        }
    }
}
