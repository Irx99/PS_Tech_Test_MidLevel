using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public GameObject visuals;
    public Rigidbody myRigidbody;
    public float gravity = 1000;
    public float velocity = 400;
    public float breakFactor = 4;
    public float mouseSensibility = 10;
    public float jumpForce = 1000;

    public CinemachineFreeLook freeLookCamera;

    private Vector2 auxVector2;
    private float inputForward, inputLateral;

    private Vector3 calculatedVelocity;
    private Vector3 playerForward, playerRight;

    private bool onFloor, onWall;
    private RaycastHit hit;

    private LayerMask ignorePlayer;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        ignorePlayer = ~LayerMask.GetMask("Player");
    }

    private void Update()
    {
        visuals.transform.rotation = Quaternion.RotateTowards(visuals.transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(this.transform.position - Camera.main.transform.position, Vector3.up).normalized, Vector3.up), Time.deltaTime * 50f + Time.deltaTime * 2f * Vector3.Angle(visuals.transform.forward, Vector3.ProjectOnPlane(this.transform.position - Camera.main.transform.position, Vector3.up).normalized));

        if(Physics.SphereCast(this.transform.position, 0.5f, Vector3.down, out hit, 1.2f, ignorePlayer))
        {
            onFloor = true;
        }
        else
        {
            onFloor = false;
        }
    }

    private void FixedUpdate()
    {
        playerForward = Vector3.ProjectOnPlane(this.transform.position - Camera.main.transform.position, Vector3.up).normalized;
        playerRight = Vector3.Cross(Vector3.ProjectOnPlane(this.transform.position - Camera.main.transform.position, Vector3.up).normalized, Vector3.down).normalized;

        calculatedVelocity = 
            Vector3.down * gravity + 
            playerForward * inputForward * velocity +
            playerRight * inputLateral * velocity;

        myRigidbody.AddForce(calculatedVelocity * Time.fixedDeltaTime);

        // Se frena para reducir el efecto hielo
        if((inputForward == 0 || Vector3.Dot(Vector3.Project(myRigidbody.velocity, playerForward), playerForward * inputForward) < 0) && Vector3.Project(myRigidbody.velocity, playerForward).magnitude != 0)
        {
            myRigidbody.velocity -= Vector3.Project(myRigidbody.velocity, playerForward) * breakFactor * Time.deltaTime;
        }

        if ((inputLateral == 0 || Vector3.Dot(Vector3.Project(myRigidbody.velocity, playerRight), playerRight * inputLateral) < 0) && Vector3.Project(myRigidbody.velocity, playerRight).magnitude != 0)
        {
            myRigidbody.velocity -= Vector3.Project(myRigidbody.velocity, playerRight) * breakFactor * Time.deltaTime;
        }
    }

    public void OnMove(InputValue value)
    {
        auxVector2 = value.Get<Vector2>();

        inputForward = auxVector2.y;
        inputLateral = auxVector2.x;
    }

    public void OnJump()
    {
        if(onFloor)
        {
            myRigidbody.velocity = new Vector3(myRigidbody.velocity.x, 0, myRigidbody.velocity.z);
            myRigidbody.AddForce(Vector3.up * jumpForce);
        }
    }

    public void OnMoveCamera(InputValue value)
    {
        auxVector2 = value.Get<Vector2>();

        freeLookCamera.m_XAxis.m_InputAxisValue = -auxVector2.x * Time.deltaTime * mouseSensibility;
        freeLookCamera.m_YAxis.m_InputAxisValue = -auxVector2.y * Time.deltaTime * mouseSensibility;
    }
}
