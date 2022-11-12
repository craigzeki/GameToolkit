using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible_Unlock : MonoBehaviour
{
    [SerializeField] private UnlockableObject objectToUnlock;

    private int originalLayer;

    private void Awake()
    {
        originalLayer = this.gameObject.layer;
    }

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        Debug.Log("Reset Called", this.gameObject);
        this.gameObject.layer = originalLayer;
        objectToUnlock.RegisterKey(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            objectToUnlock.Unlock(this.gameObject);
            this.gameObject.layer = LayerMask.NameToLayer("Default");
            this.gameObject.SetActive(false);
        }
    }
}
