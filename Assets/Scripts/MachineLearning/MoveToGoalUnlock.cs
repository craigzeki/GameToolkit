using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

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
    [SerializeField] private float radiusDefault = 2;
    [SerializeField] private float degreesDefault = 60f;

    

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private PlayerMovement playerMovement;

    private float unlockIncrementReward = 0.5f;
    private float existentialPenalty = 0f;
    private float radius;
    private float degrees;

    public override void Initialize()
    {
        base.Initialize();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;

        playerMovement = GetComponent<PlayerMovement>();

        if(unlockObjects.Count > 0) unlockIncrementReward = 0.5f / unlockObjects.Count;

        if(MaxStep > 0) existentialPenalty = -1f / MaxStep;

        
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
        radius = radiusDefault;
        degrees = degreesDefault;
        //groundMeshRenderer.material = defaultMat;
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        

        //force the player movement script to receive inputs from ml-agents instead of the controller
        playerMovement.SetInputValues(new Vector2(moveX, moveZ));

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * playerMovement.MoveSpeed;

        float reward = RayCastArc(4, degrees, transform.position, radius, transform.localEulerAngles.y);
        if (reward == 0)
        {
            radius += 0.1f;
            degrees -= 1f;

            if(radius > 8)
            {
                AddReward(0.005f);
            }
        }
        else
        {
            radius = radiusDefault;
            degrees = degreesDefault;
        }
        AddReward(reward);

        AddReward(existentialPenalty);
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
        Debug.Log("Heuristic Called");
        if (followThisPlayersMovement == null) return;
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        //continuousActions[0] = Input.GetAxisRaw("Horizontal");
        //continuousActions[1] = Input.GetAxisRaw("Vertical");
        continuousActions[0] = followThisPlayersMovement.RawInput.x;
        continuousActions[1] = followThisPlayersMovement.RawInput.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "TargetBlock":
                SetReward(1f);
                groundMeshRenderer.material = winMat;
                EndEpisode();
                break;
            case "Object-Key":
                AddReward(unlockIncrementReward);
                groundMeshRenderer.material = unlockMat;
                break;
            case "Object":
                SetReward(-5f);
                groundMeshRenderer.material = loseMat;
                EndEpisode();
                break;
            case "Wall":
                SetReward(-5f);
                groundMeshRenderer.material = loseMat;
                EndEpisode();
                break;

        }
    }


    private float RayCastArc(int numOfRays, float degrees, Vector3 centrePos, float radiusIn, float lookingAngleDeg)
    {
        float reward = 0f;

        float step = degrees / (numOfRays - 1);
        float startAngle = lookingAngleDeg - (degrees / 2);
        float angle = startAngle;
        centrePos.y /= 2;

        LayerMask mask = LayerMask.GetMask("MLAgent-RayTest");

        for(int i = 0; i < numOfRays; i++)
        {
            
            Vector3 targetPos = centrePos + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * radiusIn;
            angle += step;

            RaycastHit hit;
            if (Physics.Raycast(centrePos, (targetPos - centrePos).normalized, out hit, radiusIn, mask))
            {
                Debug.DrawLine(centrePos, hit.point, Color.red);
                //Debug.DrawRay(centrePos, dir, Color.red);
                switch (hit.collider.tag)
                {
                    case "Wall":
                        reward -= 0.01f;
                        break;
                    case "Object":
                        reward -= 0.01f;
                        break;
                    case "Object-Key":
                        reward += 0.1f;
                        break;
                    case "TargetBlock":
                        reward += 0.25f;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //Debug.DrawRay(centrePos, dir, Color.green);
                Debug.DrawLine(centrePos, targetPos, Color.green);
                //reward += 0.01f;
            }
        }


        //float angle = 0;
        //float offset = ((Mathf.Deg2Rad * degrees) / 2) - (lookingAngleDeg * Mathf.Deg2Rad);
        //float step = (Mathf.Deg2Rad * degrees) / (numOfRays-1);

        //centrePos.y /= 2;

        //LayerMask mask = LayerMask.GetMask("MLAgent-RayTest");

        //for(int i = 0; i<numOfRays; i++)
        //{
        //    float x = Mathf.Sin(angle - offset);
        //    float z = Mathf.Cos(angle - offset);
        //    angle += step;

        //    Vector3 dir = new Vector3((centrePos.x + x)*radius, centrePos.y, (centrePos.z + z)*radius);
        //    RaycastHit hit;
        //    if(Physics.Raycast(centrePos, dir, out hit, radius, mask))
        //    {
        //        Debug.DrawLine(centrePos, hit.point, Color.red);
        //        //Debug.DrawRay(centrePos, dir, Color.red);
        //        switch (hit.collider.tag)
        //        {
        //            case "Wall":
        //                reward -= 0.01f;
        //                break;
        //            case "Object":
        //                reward -= 0.01f;
        //                break;
        //            case "Object-Key":
        //                reward += 0.05f;
        //                break;
        //            case "TargetBlock":
        //                reward += 0.1f;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        //Debug.DrawRay(centrePos, dir, Color.green);
        //        Debug.DrawLine(centrePos, dir, Color.green);
        //        reward += 0.01f;
        //    }
        //}


        return reward;
    }

    //private float RewardScan360(Vector3 centre, float radius)
    //{
    //    float reward = 0f;

    //    var hits = Physics.OverlapSphere(centre, radius);

    //    for(int i = 0; i < hits.Length; i++)
    //    {

    //    }

    //}
}
