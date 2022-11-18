using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Runtime.CompilerServices;
using Unity.MLAgents.Policies;
using UnityEditor.PackageManager.Requests;

public class MoveToGoalUnlockRPMulti : Agent
{
    [SerializeField] private PlayerMovement followThisPlayersMovement;
    [SerializeField] private UnlockableTargetMulti lockedObject;

    [SerializeField] private Material winMat;
    [SerializeField] private Material loseMat;
    [SerializeField] private Material unlockMat;
    [SerializeField] private MeshRenderer groundMeshRenderer;


    

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private PlayerMovement playerMovement;

    //private float unlockIncrementReward = 0.5f;
    private float existentialPenalty = 0f;
    private bool episodeReady = false;

    private GameObject nearestKey = null;

    public override void Initialize()
    {
        base.Initialize();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;

        playerMovement = GetComponent<PlayerMovement>();

        if (MaxStep > 0) existentialPenalty = -1f / MaxStep;

        
    }

    public override void OnEpisodeBegin()
    {
        SpawnPlayer();
        ResetUnlockable();
        //unlockIncrementReward = 0.5f / lockedObject.KeysRemaining;
        nearestKey = lockedObject.GetNearestKey(transform.position);
        episodeReady = true;
    }

    private void SpawnPlayer()
    {
        if (!Helper.SetRandomLocalPositionUnRestricted(transform, 0.5f, new Vector3(0.5f, 1.1f, 0.5f))) Debug.Log("Failed to randomly place player");
        
        transform.rotation = initialRotation;
        transform.localScale = initialScale;

        //ensure the agent does not carry any momentum from the last episode
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

    }

    private void ResetUnlockable()
    {
        if (!Helper.SetRandomLocalPositionUnRestricted(lockedObject.gameObject.transform, 0.5f, Vector3.one)) Debug.Log("Failed to randomly place player");
        lockedObject.SpawnKeys();
    }

    

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        //force the player movement script to receive inputs from ml-agents instead of the controller
        playerMovement.SetInputValues(new Vector2(moveX, moveZ));

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.fixedDeltaTime * playerMovement.MoveSpeed;

        AddReward(existentialPenalty);

        if(!lockedObject.Locked)
        {
            float maxReward = 1f / MaxStep;
            float proximityThreshold = 10f;

            float distance = Vector3.Distance(lockedObject.gameObject.transform.position, transform.position);

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
        sensor.AddObservation(lockedObject.Locked);

        //the distance to the target [Float - 1 Values]
        sensor.AddObservation(Vector3.Distance(lockedObject.gameObject.transform.position, transform.position));

        //the direction to the target [Vector3 - 3 Values]
        sensor.AddObservation((lockedObject.gameObject.transform.position - transform.position).normalized);

        //the direction we are facing [Vector3 - 3 Values]
        sensor.AddObservation(transform.forward);


        //the distance and direction to the nearest key [Float - 1 value, Vector3 - 3 values]
        if (nearestKey != null && lockedObject.KeysRemaining != 0)
        {
            sensor.AddObservation(Vector3.Distance(nearestKey.transform.position, transform.position));
            sensor.AddObservation((nearestKey.transform.position - transform.position).normalized);
        }
        else
        {
            
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector3.zero);
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
                if (lockedObject.Locked)
                {
                    AddReward(-1f);
                    groundMeshRenderer.material = loseMat;
                }
                else
                {
                    AddReward(1f);
                    groundMeshRenderer.material = winMat;
                }
                
                EndEpisode();
                break;
            case "Object-Key":
                AddReward(0.6f);
                //set to null instead of recalulating as we won't be sure if it has been destroyed yet, instead check in Update
                nearestKey = null;
                
                break;
            case "Object":
                SetReward(-0.05f);
                //groundMeshRenderer.material = loseMat;
                //EndEpisode();
                break;
            case "Wall":
                SetReward(-0.1f);
                groundMeshRenderer.material = loseMat;
                
                break;

        }
    }

    private void FixedUpdate()
    {
        if (!episodeReady) return;
        //fixedUpdate is called before OnTriggerxx therefore it is OK to check for null here from previous cycle
        if (nearestKey == null) nearestKey = lockedObject.GetNearestKey(transform.position);
        if (lockedObject.KeysRemaining == 0) groundMeshRenderer.material = unlockMat;
    }



}
