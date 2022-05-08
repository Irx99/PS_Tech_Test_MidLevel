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
    public float fixedVelocityOnWall = 5f;
    public float maxVelocity = 30f, maxFallVelocity = 50f;

    public float jumpBufferTime = 0.2f, jumpCoyoteTime = 0.2f;

    public CinemachineFreeLook freeLookCamera;

    public ParticleSystem floorEffects, jetpackEffects, jumpEffects;
    public TrailRenderer wallEffects;

    private Vector2 auxVector2;
    private float inputForward, inputLateral;

    private Vector3 calculatedVelocity;
    private Vector3 playerForward, playerRight;
    private Vector3 rigidbodyLastDirection;

    private bool onFloor, onWall;
    private Vector3 floorNormal;
    private RaycastHit hit;
    private Vector3 wallNormal;

    private LayerMask ignorePlayer;

    private bool jetpackActive;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        ignorePlayer = ~LayerMask.GetMask("Player");

        rigidbodyLastDirection = this.transform.forward;
    }

    private void Update()
    {
        CharacterRotation();
        FloorDetection();
        WallDetection();

        auxVector2 = Vector2.zero;
    }

    private void FixedUpdate()
    {
        CalculateMovement();
        ConstantVelocityOnWall();
        ReduceIceEffect();
        LimitMaxVelocity();
    }

    private void CharacterRotation()
    {
        if (myRigidbody.velocity.magnitude > 0.1f && Mathf.Abs(Vector3.Dot(myRigidbody.velocity.normalized, Vector3.up)) != 1)
        {
            rigidbodyLastDirection = myRigidbody.velocity.normalized;
        }

        if (Mathf.Abs(Vector3.Dot(rigidbodyLastDirection, Vector3.up)) != 1)
        {
            Quaternion toQuaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(rigidbodyLastDirection, Vector3.up).normalized, Vector3.up);
            float maxDegreesDelta = Time.deltaTime * 50f + Time.deltaTime * 2f * Vector3.Angle(visuals.transform.forward, Vector3.ProjectOnPlane(rigidbodyLastDirection, Vector3.up).normalized);

            visuals.transform.rotation = Quaternion.RotateTowards(visuals.transform.rotation, toQuaternion, maxDegreesDelta);
        }
    }

    private void FloorDetection()
    {
        if (Physics.SphereCast(this.transform.position, 0.25f, Vector3.down, out hit, 1.2f, ignorePlayer))
        {
            onFloor = true;
            floorNormal = hit.normal;

            if (floorEffects.isStopped)
            {
                floorEffects.Play();
            }
        }
        else
        {
            onFloor = false;
            floorNormal = Vector3.up;

            if (floorEffects.isPlaying)
            {
                floorEffects.Stop();
            }
        }
    }

    private void WallDetection()
    {
        wallNormal = Vector3.zero;
        onWall = false;

        for (int i = 0; i < 16; i++)
        {
            if (Physics.Raycast(this.transform.position, Quaternion.AngleAxis(360f * (i / (16f - 1f)), Vector3.up) * this.transform.forward, out hit, 1f, ignorePlayer))
            {
                wallNormal += hit.normal;
                onWall = true;

                Debug.DrawLine(this.transform.position, this.transform.position + Quaternion.AngleAxis(360f * (i / (16f - 1f)), Vector3.up) * this.transform.forward * 1f, Color.green);
            }
            else
            {
                Debug.DrawLine(this.transform.position, this.transform.position + Quaternion.AngleAxis(360f * (i / (16f - 1f)), Vector3.up) * this.transform.forward * 1f, Color.red);
            }
        }

        wallNormal.Normalize();

        if (!onFloor && onWall && !wallEffects.emitting)
        {
            wallEffects.emitting = true;
        }
        else if ((onFloor || !onWall) && wallEffects.emitting)
        {
            wallEffects.emitting = false;
        }
    }

    private void CalculateMovement()
    {
        playerForward = Vector3.ProjectOnPlane(this.transform.position - Camera.main.transform.position, Vector3.up).normalized;
        playerRight = Vector3.Cross(Vector3.ProjectOnPlane(this.transform.position - Camera.main.transform.position, Vector3.up).normalized, Vector3.down).normalized;

        calculatedVelocity =
            -floorNormal * gravity +
            (jetpackActive && myRigidbody.velocity.y < 10 ? Vector3.up * gravity * 1.5f : Vector3.zero) +
            Vector3.ProjectOnPlane(playerForward, floorNormal).normalized * inputForward * velocity +
            Vector3.ProjectOnPlane(playerRight, floorNormal).normalized * inputLateral * velocity;

        myRigidbody.AddForce(calculatedVelocity * Time.fixedDeltaTime);
    }

    private void ConstantVelocityOnWall()
    {
        if (!onFloor && onWall && !jetpackActive && myRigidbody.velocity.y < 0)
        {
            myRigidbody.velocity += (myRigidbody.velocity.y - -fixedVelocityOnWall) * Vector3.down * 10 * Time.fixedDeltaTime;
        }
    }

    // Se frena la velocidad al soltar el input o intentarse moverse en direccion contraria a la velocidad del rigidbody
    private void ReduceIceEffect()
    {
        if ((inputForward == 0 || Vector3.Dot(Vector3.Project(myRigidbody.velocity, playerForward), playerForward * inputForward) < 0) && Vector3.Project(myRigidbody.velocity, playerForward).magnitude != 0)
        {
            myRigidbody.velocity -= Vector3.Project(myRigidbody.velocity, playerForward) * breakFactor * Time.deltaTime;
        }

        if ((inputLateral == 0 || Vector3.Dot(Vector3.Project(myRigidbody.velocity, playerRight), playerRight * inputLateral) < 0) && Vector3.Project(myRigidbody.velocity, playerRight).magnitude != 0)
        {
            myRigidbody.velocity -= Vector3.Project(myRigidbody.velocity, playerRight) * breakFactor * Time.deltaTime;
        }
    }

    private void LimitMaxVelocity()
    {
        // Se limita la velocidad maxima
        if (Vector3.ProjectOnPlane(myRigidbody.velocity, Vector3.up).magnitude > maxVelocity)
        {
            myRigidbody.velocity = Vector3.Project(myRigidbody.velocity, Vector3.up) + Vector3.ProjectOnPlane(myRigidbody.velocity, Vector3.up).normalized * maxVelocity;
        }

        // Se limita la velocidad maxima de caida
        if (Vector3.Project(myRigidbody.velocity, Vector3.up).magnitude > maxFallVelocity)
        {
            myRigidbody.velocity = Vector3.ProjectOnPlane(myRigidbody.velocity, Vector3.up) + Vector3.Project(myRigidbody.velocity, Vector3.up).normalized * maxFallVelocity;
        }
    }

    public void OnMove(InputValue value)
    {
        auxVector2 = value.Get<Vector2>();

        inputForward = auxVector2.y;
        inputLateral = auxVector2.x;
    }

    public void OnJump(InputValue value)
    {
        if(value.Get<float>() > 0)
        {
            if(onFloor)
            {
                myRigidbody.velocity = new Vector3(myRigidbody.velocity.x, 0, myRigidbody.velocity.z);
                myRigidbody.AddForce(Vector3.up * jumpForce);

                jumpEffects.Play();
            }
            else if(onWall)
            {
                myRigidbody.velocity = new Vector3(myRigidbody.velocity.x, 0, myRigidbody.velocity.z);
                myRigidbody.AddForce((wallNormal * 2f + Vector3.up).normalized * 2f * jumpForce);

                jumpEffects.Play();
            } 
            else if(!jetpackActive)
            {
                jetpackActive = true;
                jetpackEffects.Play();
            }
        }
        else
        {
            if(jetpackActive)
            {
                jetpackActive = false;
                jetpackEffects.Stop();
            }
        }
    }

    public void OnMoveCamera(InputValue value)
    {
        auxVector2 = value.Get<Vector2>();

        freeLookCamera.m_XAxis.m_InputAxisValue = -auxVector2.x * mouseSensibility;
        freeLookCamera.m_YAxis.m_InputAxisValue = -auxVector2.y * mouseSensibility;
    }
}
