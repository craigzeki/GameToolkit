using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private GameObject objectToFollow;
    [SerializeField] private bool lookAtObject = false;

    private Vector3 offset = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        offset = objectToFollow.transform.position - transform.position;
    }

    

    private void LateUpdate()
    {
        if (objectToFollow == null) return;

        if (lookAtObject) transform.LookAt(objectToFollow.transform.position);
        transform.position = objectToFollow.transform.position - offset;
    }
}
