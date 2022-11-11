using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoal : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private PlayerMovement followThisPlayersMovement;

    [SerializeField] private Material winMat;
    [SerializeField] private Material loseMat;
    [SerializeField] private MeshRenderer groundMeshRenderer;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private PlayerMovement playerMovement;

    public override void Initialize()
    {
        base.Initialize();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;

        playerMovement = GetComponent<PlayerMovement>();

    }

    public override void OnEpisodeBegin()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = initialScale;
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
                SetReward(1f);
                groundMeshRenderer.material = winMat;
                EndEpisode();
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
