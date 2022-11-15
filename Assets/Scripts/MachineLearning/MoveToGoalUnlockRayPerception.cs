using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Runtime.CompilerServices;
using Unity.MLAgents.Policies;
using UnityEditor.PackageManager.Requests;

public class MoveToGoalUnlockRayPerception : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private PlayerMovement followThisPlayersMovement;
    [SerializeField] private List<GameObject> unlockObjects;
    [SerializeField] private GameObject lockedObject;

    [SerializeField] private Material winMat;
    [SerializeField] private Material loseMat;
    [SerializeField] private Material unlockMat;
    [SerializeField] private MeshRenderer groundMeshRenderer;


    

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private PlayerMovement playerMovement;

    private float unlockIncrementReward = 0.5f;
    private float existentialPenalty = 0f;


    public override void Initialize()
    {
        base.Initialize();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;

        playerMovement = GetComponent<PlayerMovement>();

        if (unlockObjects.Count > 0) unlockIncrementReward = 0.5f / unlockObjects.Count;

        if (MaxStep > 0) existentialPenalty = -1f / MaxStep;

        
    }

    public override void OnEpisodeBegin()
    {
        SpawnPlayer();
        SpawnCollectibles();

        lockedObject.GetComponent<UnlockableObject>().Reset();
        

    }

    private void SpawnPlayer()
    {
        
        transform.localPosition = RandomPosition(initialPosition.y, new Vector3(0.5f,1.1f,0.5f));
        transform.rotation = initialRotation;
        transform.localScale = initialScale;
    }

    private void SpawnCollectibles()
    {
        foreach (GameObject go in unlockObjects)
        {
            go.transform.localPosition = RandomPosition(0.5f, new Vector3(0.5f, 0.5f, 0.5f));
            if (!go.activeSelf) go.SetActive(true);
            go.GetComponent<Collectible_Unlock>().Reset();
        }
    }

    private Vector3 RandomPosition(float y, Vector3 halfExtents)
    {
        LayerMask mask = LayerMask.GetMask("MLAgent-RayTest");
        Vector3 targetPos = Vector3.zero;
        targetPos.y = y;

        do
        {
            targetPos.x = Random.Range(-10, 10);
            targetPos.z = Random.Range(-8, 8);

            if ((targetPos.x > 1.5) && (targetPos.z < -2.6))
            {
                if (Random.Range((int)0, (int)1) == 0)
                {
                    targetPos.x = 1.5f;
                }
                else
                {
                    targetPos.z = -2.6f;
                }
            }

            
        } while (Physics.CheckBox(targetPos, halfExtents,Quaternion.identity,mask));

        return targetPos;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        //force the player movement script to receive inputs from ml-agents instead of the controller
        playerMovement.SetInputValues(new Vector2(moveX, moveZ));

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * playerMovement.MoveSpeed;

        AddReward(existentialPenalty);

        if(!lockedObject.GetComponent<UnlockableObject>().Locked)
        {
            float maxReward = 100f / MaxStep;
            float proximityThreshold = 10f;

            float distance = Vector3.Distance(targetTransform.position, transform.position);

            if (distance < proximityThreshold)
            {
                distance = proximityThreshold - distance;
                AddReward((distance * maxReward) / proximityThreshold);
            }
            
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //total 12 values observed

        //is the wall unlocked? [bool - 1 value]
        sensor.AddObservation(lockedObject.GetComponent<UnlockableObject>().Locked);

        //the distance to the target [Float - 1 Values]
        sensor.AddObservation(Vector3.Distance(targetTransform.position, transform.position));

        //the direction to the target [Vector3 - 3 Values]
        sensor.AddObservation((targetTransform.position - transform.position).normalized);

        //the direction we are facing [Vector3 - 3 Values]
        sensor.AddObservation(transform.forward);


        //the distance and direction to the collectible objects [Float - 1 value, Vector3 - 3 values]
        foreach (GameObject go in unlockObjects)
        {
            sensor.AddObservation(Vector3.Distance(go.transform.position, transform.position));
            sensor.AddObservation((go.transform.position - transform.position).normalized);
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("Heuristic Called");
        if (followThisPlayersMovement == null) return;
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = followThisPlayersMovement.RawInput.x;
        continuousActions[1] = followThisPlayersMovement.RawInput.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "TargetBlock":
                AddReward(1f);
                groundMeshRenderer.material = winMat;
                EndEpisode();
                break;
            case "Object-Key":
                AddReward(2f);
                groundMeshRenderer.material = unlockMat;
                break;
            case "Object":
                //SetReward(-1f);
                //groundMeshRenderer.material = loseMat;
                //EndEpisode();
                break;
            case "Wall":
                SetReward(-1f);
                groundMeshRenderer.material = loseMat;
                EndEpisode();
                break;

        }
    }


   
}
