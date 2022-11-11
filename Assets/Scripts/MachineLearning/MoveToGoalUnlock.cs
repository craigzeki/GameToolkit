using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalUnlock : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private PlayerMovement followThisPlayersMovement;
    [SerializeField] private List<GameObject> unlockObjects;
    [SerializeField] private GameObject lockedObject;

    [SerializeField] private Material winMat;
    [SerializeField] private Material loseMat;
    [SerializeField] private Material unlockMat;
    [SerializeField] private Material defaultMat;
    [SerializeField] private MeshRenderer groundMeshRenderer;

    

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private PlayerMovement playerMovement;

    private float unlockIncrementReward = 0.5f;

    public override void Initialize()
    {
        base.Initialize();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;

        playerMovement = GetComponent<PlayerMovement>();

        if(unlockObjects.Count > 0) unlockIncrementReward = 0.5f / unlockObjects.Count;

        
    }

    public override void OnEpisodeBegin()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = initialScale;
        lockedObject.GetComponent<UnlockableObject>().Reset();
        foreach (GameObject go in unlockObjects)
        {
            if(!go.activeSelf) go.SetActive(true);
            go.GetComponent<Collectible_Unlock>().Reset();
        }
        //groundMeshRenderer.material = defaultMat;
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        //force the player movement script to receive inputs from ml-agents instead of the controller
        playerMovement.SetInputValues(new Vector2(moveX, moveZ));

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * playerMovement.MoveSpeed;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);

        foreach(GameObject go in unlockObjects)
        {
            sensor.AddObservation(go.transform.localPosition);
            sensor.AddObservation(go.activeSelf);
        }

        sensor.AddObservation(lockedObject.GetComponent<UnlockableObject>().Locked);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (followThisPlayersMovement == null) return;
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        //continuousActions[0] = Input.GetAxisRaw("Horizontal");
        //continuousActions[1] = Input.GetAxisRaw("Vertical");
        continuousActions[0] = followThisPlayersMovement.RawInput.x;
        continuousActions[1] = followThisPlayersMovement.RawInput.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        switch(other.gameObject.tag)
        {
            case "TargetBlock":
                AddReward(1f);
                groundMeshRenderer.material = winMat;
                EndEpisode();
                break;
            case "Object-Key":
                AddReward(unlockIncrementReward);
                groundMeshRenderer.material = unlockMat;
                break;
            case "Object":
                SetReward(-1f);
                groundMeshRenderer.material = loseMat;
                EndEpisode();
                break;
            case "Wall":
                SetReward(-1f);
                groundMeshRenderer.material = loseMat;
                EndEpisode();
                break;

        }

    }
}
