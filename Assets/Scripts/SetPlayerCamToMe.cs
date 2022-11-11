using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayerCamToMe : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cinemachine.CinemachineBrain brain;
        if(Camera.main.TryGetComponent<CinemachineBrain>(out brain))
        {
            brain.ActiveVirtualCamera.LookAt = this.gameObject.transform;
            brain.ActiveVirtualCamera.Follow = this.gameObject.transform;

        }
    }
}
 
