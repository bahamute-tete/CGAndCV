using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public class PhyicsMove : MonoBehaviour
{
    [Range(1f, 10f)]
    public float maxSpeed = 10;

    [Range(1f, 100f)]
    public float maxAcceleration = 10, maxAirAcceleration = 1f;

    [Range(0, 10f)]
    public float jumpHeight = 2f;

    Vector3 velocity, desiredVelocity;
    


    Rigidbody body;

    bool desiredJump;
    [SerializeField]bool onGround;

    [Range(0, 5)]
    public int maxAirJump = 0;
    int jumpPhase;

    [Range(0f, 90f)]
    public float maxGroundAngle = 25f;
    float minGroundDotProduct;

    Vector3 contactNormal;


    private void OnValidate()
    {
        minGroundDotProduct = Cos(maxGroundAngle*Deg2Rad);// slop_normal projection on vertical Y 
    }
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 playerInpute;
        playerInpute.x = Input.GetAxis("Horizontal");
        playerInpute.y = Input.GetAxis("Vertical");
        playerInpute = Vector2.ClampMagnitude(playerInpute, 1f);


        desiredVelocity = new Vector3(playerInpute.x, 0, playerInpute.y) * maxSpeed;


        desiredJump |= Input.GetButtonDown("Jump");
        //Vector3 displament = velocity * Time.deltaTime;
        //Vector3 newPosition = transform.localPosition + displament;
        //transform.localPosition = newPosition;
       

    }

    private void FixedUpdate()
    {



        UpdateState();

        float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;////Time.deltaTime = Time.fixedDeltaTime

        velocity.x =
            Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);//increases delta until  target
        velocity.z =
            Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);



        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }


        body.velocity = velocity;

        onGround = false;


    }

    private void UpdateState()
    {
        //retrieve it from the body before adjusting it to match the desired velocity.
        velocity = body.velocity;
        if (onGround)
        {
            jumpPhase = 0;
        }
        else
        {
            contactNormal = Vector3.up;//air jumps still go straight up
        }
            
    }

    private void Jump()
    {
        if (onGround || jumpPhase < maxAirJump)
        {
            jumpPhase += 1;
            float jumpSpeed= Sqrt(-2f * Physics.gravity.y * jumpHeight); //v= sqrt(-2gh)
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if (alignedSpeed > 0f)
            {
                //an upward speed then subtract it from the jump speed before adding it to the velocity's Y component.
                //That way we'll never exceed the jump speed.
                jumpSpeed = Max(jumpSpeed - alignedSpeed,0);//jump speed never goes negative.
            }
            velocity += jumpSpeed*contactNormal;
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }
    private void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;//Get contancPoint normal

            if (normal.y >= minGroundDotProduct)
            {
                onGround = true;
                contactNormal = normal;
            }    
        }
    }

}
