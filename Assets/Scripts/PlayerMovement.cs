using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    
    public enum PlayerState
    {
        MOVING = 0,
        IDLE,
        NUM_OF_STATES
    }

    public enum NoseState
    {
        MOVING = 0,
        IDLE,
        NUM_OF_STATES
    }

    [SerializeField] bool mlAgentControlled = false;
    [SerializeField] float inputStickDeadband = 0.1f;
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float rotationSpeed = 1.0f;
    [SerializeField] public Transform nose;
    [SerializeField] float noseSpeed = 1.0f;
    [SerializeField] float noseMinY = -0.4f;
    [SerializeField] float noseMaxY = 0.4f;
    [SerializeField] Animator myAnimator;

    [SerializeField] private PlayerState playerState = PlayerState.IDLE;
    private Vector2 moveInput = Vector2.zero;
    private Vector2 rawInput = Vector2.zero;
    private Rigidbody myRB;
    private Vector3 targetPos = Vector3.zero;
    private float targetAngle = 0.0f;
    private Vector3 lerpAngles = Vector3.zero;
    private NoseState noseState = NoseState.IDLE;
    private Vector2 noseInput = Vector2.zero;

    public Vector2 MoveInput { get => moveInput; }
    public float MoveSpeed { get => moveSpeed; }
    public Vector2 RawInput { get => rawInput; }

    private void Awake()
    {
        myRB = GetComponent<Rigidbody>();
        myAnimator = GetComponent<Animator>();
        targetAngle = 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LerpYAngle());
    }

    // Update is called once per frame
    void Update()
    {
        switch (playerState)
        {
            case PlayerState.MOVING:

                myAnimator.SetBool("isMoving", true);
                targetPos = transform.position;
                targetPos.x += (moveInput.x * moveSpeed * Time.deltaTime);
                targetPos.z += (moveInput.y * moveSpeed * Time.deltaTime);
                //myRB.MovePosition(targetPos); //not as smooth as needed - introduced jerkiness into the movement, especially during camera follow
                if(!mlAgentControlled) transform.position = targetPos;
                // rotation target pos handled in Coroutine for smooth turning
                transform.eulerAngles = lerpAngles;
                break;
            case PlayerState.IDLE:
                myAnimator.SetBool("isMoving", false);
                break;
            case PlayerState.NUM_OF_STATES:
            default:
                break;
        }

        switch (noseState)
        {
            case NoseState.MOVING:

                float noseY;
                noseY = nose.localPosition.y + (noseInput.y * noseSpeed * Time.deltaTime);
                noseY = Mathf.Clamp(noseY, noseMinY, noseMaxY);

                nose.localPosition = new Vector3(nose.localPosition.x, noseY, nose.localPosition.z); 

                break;
            case NoseState.IDLE:
                break;
            case NoseState.NUM_OF_STATES:
            default:
                break;
        }

    }

    IEnumerator LerpYAngle()
    {
        while(true)
        {
            if(playerState == PlayerState.MOVING)
            {
                lerpAngles = transform.eulerAngles;
                targetAngle = Vector2.SignedAngle(moveInput, Vector2.up);
                lerpAngles.y = Mathf.LerpAngle(lerpAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }

    public void SetInputValues(Vector2 inputs)
    {
        if ((Mathf.Abs(inputs.x) > inputStickDeadband) || (Mathf.Abs(inputs.y) > inputStickDeadband))
        {
            playerState = PlayerState.MOVING;
            moveInput = inputs;
        }
        else
        {
            playerState = PlayerState.IDLE;
            moveInput = Vector2.zero;
        }

    }

    private void OnLeft_Stick(InputValue value)
    {
        rawInput = value.Get<Vector2>();
        //ignore the input if disableMovement is set - allows ml-agents (or any other external script) to control player via SetInputValues
        if(!mlAgentControlled) SetInputValues(rawInput);
    }

    private void OnRight_Stick(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        if ((Mathf.Abs(inputVector.x) > inputStickDeadband) || (Mathf.Abs(inputVector.y) > inputStickDeadband))
        {
            noseState = NoseState.MOVING;
            noseInput = inputVector;
        }
        else
        {
            noseState = NoseState.IDLE; 
            noseInput = Vector2.zero;
        }
    }

    private void OnSave_Game()
    {
        //SaveLoadManager.Instance.SaveGame();
        SaveLoadManager.Instance.RequestSaveGame();
    }

    private void OnLoad_Game()
    {
        //SaveLoadManager.Instance.LoadGameFromLocal();
        SaveLoadManager.Instance.RequestLoadGame();
    }

}