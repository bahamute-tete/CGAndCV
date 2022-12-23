using System;
using UnityEngine;

public class MovingSphereRoll : MonoBehaviour
{

    [SerializeField]
    Transform playerInputSpace = default, ball = default;

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f, maxClimbSpeed = 2f, maxSwimSpeed = 5f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f, maxClimbAcceleration = 20f, maxSwimAcceleration = 5f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1, climbMask = -1, waterMask = 0;

    Rigidbody body, connetcedBody, previousConnectedBody;


    Vector3 playerInput;
    Vector3 velocity, connectionVelocity;

    Vector3 upAxis, rightAxis, forwardAxis;

    bool desiredJump, desiresClimbing;

    Vector3 contactNormal, steepNormal, climbNormal, lastClimbNormal;

    int groundContactCount, steepContactCount, climbContactCount;

    bool OnGround => groundContactCount > 0;

    bool OnSteep => steepContactCount > 0;

    bool Climbing => climbContactCount > 0 && stepsSinceLastJump > 2;

    int jumpPhase;

    float minGroundDotProduct, minStairsDotProduct, minClimbDotProduct;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    Vector3 connectionWorldPosition, connectionLocalPosition;

    [SerializeField, Range(90, 180)] float maxClimbAngle = 140;
    [SerializeField] Material normalMaterial = default, climbMaterial = default, swimmingMaterial = default;
    MeshRenderer meshRenderer;
    [SerializeField] TrailRenderer trailRenderer;


    float submergence;
    bool InWater => submergence > 0f;

    [SerializeField] float submergenceOffset = 0.5f;
    [SerializeField, Min(0.1f)] float submergenceRange = 1f;
    [SerializeField, Range(0f, 10f)] float waterDrag = 1f;
    [SerializeField, Min(0f)] float buoyancy = 1f;
    [SerializeField, Range(0.01f, 1f)] float swimThreshold = 0.5f;
    bool Swimming => submergence >= swimThreshold;

    [SerializeField, Min(0.1f)] float ballRadius = 0.5f;
    Vector3 lastContactNormal, lastSteepNormal, lastConnetctionVelocity;
    [SerializeField, Min(0f)] float ballAlignSpeed = 180f;
    [SerializeField, Min(0f)] float ballAirRotation = 0.5f,ballSwimRotation= 2f;


    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        meshRenderer =ball.GetChild(1).GetComponent<MeshRenderer>();

        OnValidate();
    }

    void UpdateBall()
    {
        Material ballMat = normalMaterial;

        float rotationFactor = 1f;

        Vector3 rotationPlaneNormal = lastContactNormal;

        if (Climbing)
        {
            ballMat = climbMaterial;
            trailRenderer.material.SetColor("_Color", climbMaterial.GetColor("_EmissionColor"));
        }
        else if (Swimming)
        {
            ballMat = swimmingMaterial;
            trailRenderer.material.SetColor("_Color", swimmingMaterial.GetColor("_EmissionColor"));
            rotationFactor = ballSwimRotation;
        }
        else if (!OnGround)
        {
            if (OnSteep)
            {
                lastContactNormal = lastSteepNormal;
            }
            else
            {
                rotationFactor = ballAirRotation;
            }
        }

        meshRenderer.material = ballMat;
        trailRenderer.material.SetColor("_Color", meshRenderer.material.GetColor("_EmissionColor"));


        Vector3 movment = (body.velocity -lastConnetctionVelocity) * Time.deltaTime;

        movment -= rotationPlaneNormal * Vector3.Dot(movment, rotationPlaneNormal);

        float distance = movment.magnitude;

        Quaternion rotation = ball.localRotation;
        if (connetcedBody && connetcedBody == previousConnectedBody)
        {
            rotation = Quaternion.Euler(connetcedBody.angularVelocity * (Mathf.Rad2Deg * Time.deltaTime)) * rotation;
            if (distance < 0.001f)
            {
                ball.localRotation = rotation;
                return;
            }
        }
        else if  (distance < 0.001f)
        {
            return;
        }
        float angle = rotationFactor * distance * (180.0f / Mathf.PI) / ballRadius;
       

        Vector3 rotationAxis = Vector3.Cross(rotationPlaneNormal, movment).normalized;

        rotation = Quaternion.Euler(rotationAxis * angle) * rotation;
        if (ballAlignSpeed > 0f)
        {
            rotation = AlignBallRotation(rotationAxis, rotation,distance);
        }

        ball.localRotation = rotation;

        

    }

    Quaternion AlignBallRotation(Vector3 rotatingAxis,Quaternion rotation,float distance)
    {
        
        Vector3 ballAxis =ball.up;
        
        float dot = Mathf.Clamp(Vector3.Dot(ballAxis, rotatingAxis), -1f, 1f);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        float maxAngle = ballAlignSpeed * distance;

        Quaternion newAlignment =
            Quaternion.FromToRotation(ballAxis, rotatingAxis) * rotation;
        if (angle <= maxAngle)
        {
           return  newAlignment;
        }
        else
        {
            return Quaternion.SlerpUnclamped(
               rotation, newAlignment, maxAngle / angle
            );
        }
    }

    void Update()
    {
        
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.z = Input.GetAxis("Vertical");
        playerInput.y = Swimming ? Input.GetAxis("UpDown") : 0f;
        playerInput = Vector3.ClampMagnitude(playerInput, 1f);

        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis =
                ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }

        if (Swimming)
        {
            desiredJump = false;
        }
        else
        {
            desiredJump |= Input.GetButtonDown("Jump");
            desiresClimbing = Input.GetButton("Climb");
        }

        UpdateBall();
       
        //meshRenderer.material.color = Color.white * submergence;
    }

    void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
        //Debug.Log("gravity==" + gravity);
        UpdateState();

        if (InWater)
        {
            velocity *= 1f - waterDrag*submergence * Time.deltaTime;
        }

        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            Jump(gravity);
        }

        if (Climbing)
        {
            velocity -= contactNormal * (maxClimbAcceleration * 0.9f * Time.deltaTime);
        }
        else if (InWater)
        {
            velocity += gravity * ((1 - buoyancy * submergence) * Time.deltaTime);
        }
        else if (OnGround && velocity.sqrMagnitude < 0.01f)
        {
            velocity += contactNormal * (Vector3.Dot(gravity, contactNormal) * Time.deltaTime);
        }
        else if (desiresClimbing && OnGround)
        {
            velocity += (gravity - contactNormal * (maxClimbAcceleration * 0.9f)) * Time.deltaTime;
        }
        else
        {
            velocity += gravity * Time.deltaTime;
        }
       

        body.velocity = velocity;
        ClearState();
    }

    void ClearState()
    {
        lastContactNormal = contactNormal;
        lastSteepNormal = steepNormal;
        lastConnetctionVelocity = connectionVelocity;
        groundContactCount = steepContactCount = climbContactCount = 0;
        contactNormal = steepNormal = climbNormal= connectionVelocity = Vector3.zero;
        previousConnectedBody = connetcedBody;
        connetcedBody = null;
        submergence = 0;
    }

    void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        velocity = body.velocity;
        if (CheckSwimming()||CheckClimbing()||OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = upAxis;
        }

        if (connetcedBody)
        {
            if (connetcedBody.isKinematic || connetcedBody.mass >= body.mass)
            UpdateConnectionState();
        }
    }

    private void UpdateConnectionState()
    {
       
        if (connetcedBody == previousConnectedBody)
        {
            Vector3 connectionMovement = connetcedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
            
        }
       
        connectionWorldPosition = body.position;
        connectionLocalPosition = connetcedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2 )
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(
            body.position, -upAxis, out RaycastHit hit,
            probeDistance, probeMask, QueryTriggerInteraction.Ignore
        ))
        {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }

        connetcedBody = hit.rigidbody;
        return true;
    }

    public void PreventSnapGround()
    {
        stepsSinceLastJump = -1;
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                steepContactCount = 0;
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    bool CheckClimbing()
    {
        if (Climbing)
        {
            if (climbContactCount > 1)
            {
                climbNormal.Normalize();
                float upDot = Vector3.Dot(upAxis, climbNormal);
                if (upDot >= minGroundDotProduct)
                {
                    climbNormal = lastClimbNormal;
                }
            }
            groundContactCount = 1;
            contactNormal = climbNormal;
            return true;
        }
        return false;
    }

    bool CheckSwimming()
    {
        if (Swimming)
        {
            groundContactCount = 0;
            contactNormal = upAxis;
            return true;
        }
        return false;
    }

    void AdjustVelocity()
    {
        float acceleration, speed;
        Vector3 xAxis, zAxis;
        if (Climbing)
        {
            acceleration = maxClimbAcceleration;
            speed = maxClimbSpeed;
            xAxis = Vector3.Cross(contactNormal, upAxis);
            zAxis = upAxis;
        }
        else if (InWater)
        {
            float swimFactor = Mathf.Min(1, submergence / swimThreshold);
            acceleration = Mathf.LerpUnclamped( OnGround? maxAcceleration:maxAirAcceleration, maxSwimAcceleration, swimFactor);
            speed = Mathf.LerpUnclamped(maxSpeed, maxSwimSpeed, swimFactor);
            xAxis = rightAxis;
            zAxis = forwardAxis;
        }
        else
        {
            acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            speed = OnGround && desiresClimbing ? maxClimbSpeed : maxSpeed;
            xAxis = rightAxis;
            zAxis = forwardAxis;
        }

         xAxis = ProjectDirectionOnPlane(xAxis, contactNormal);
         zAxis = ProjectDirectionOnPlane(zAxis, contactNormal);

        Vector3 relativeVelocity = velocity - connectionVelocity;

      
        Vector3 adjustment;
        adjustment.x = playerInput.x * speed - Vector3.Dot(relativeVelocity, xAxis);
        adjustment.z = playerInput.z * speed - Vector3.Dot(relativeVelocity, zAxis);
        adjustment.y = Swimming ? playerInput.y * speed - Vector3.Dot(relativeVelocity, upAxis) : 0;

        adjustment = Vector3.ClampMagnitude(adjustment, acceleration * Time.deltaTime);

        velocity += xAxis * adjustment.x + zAxis * adjustment.z;

        if (Swimming)
        {
          velocity += upAxis * adjustment.y ;
        }
    }

    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }

        stepsSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);

        if (InWater)
        {
            jumpSpeed *= Mathf.Max(0,1f -submergence/swimThreshold);
        }

        jumpDirection = (jumpDirection + upAxis).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += jumpDirection * jumpSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        int layer = collision.gameObject.layer;
        float minDot = GetMinDot(layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if ( upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
                connetcedBody = collision.rigidbody;
            }
            else 
            {
                if (upDot > -0.01f)
                {
                    steepContactCount += 1;
                    steepNormal += normal;
                    if (groundContactCount == 0)
                        connetcedBody = collision.rigidbody;
                }

                if (desiresClimbing && upDot >= minClimbDotProduct && (climbMask & (1 << layer)) != 0)
                {
                    climbContactCount += 1;
                    climbNormal += normal;
                    lastClimbNormal = normal;
                    connetcedBody = collision.rigidbody;
                }
            }

           
        }
    }

    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ?//1<<layer equal GetMask("layerName") 
            minGroundDotProduct : minStairsDotProduct;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ( (waterMask & ( 1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if ((waterMask & (1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence(other);
        }
    }

    private void EvaluateSubmergence(Collider collider)
    {
        if (Swimming)
        {
            return;
        }

        if (Physics.Raycast(body.position + upAxis * submergenceOffset, -upAxis, out RaycastHit hit, submergenceRange+1, waterMask, QueryTriggerInteraction.Collide))
        {
            submergence = 1 - hit.distance / submergenceRange;
        }
        else
        {
            submergence = 1f;
        }

        if (Swimming)
        {
            connetcedBody = collider.attachedRigidbody;
        }

       
    }
}

