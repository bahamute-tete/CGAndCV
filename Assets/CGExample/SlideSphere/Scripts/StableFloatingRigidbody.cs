﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StableFloatingRigidbody : MonoBehaviour
{

    [SerializeField]
    bool floatToSleep = false;
    [SerializeField]
    bool safeFloating = false;

    Rigidbody body;

    float floatDelay;

    [SerializeField] float submergenceOffset = 0.5f;
    [SerializeField, Min(0.1f)] float submergenceRange = 1f;
    [SerializeField, Min(0)] float buoyancy = 1f;
    [SerializeField, Range(0f, 1f)] float waterDrag = 1f;
    [SerializeField] LayerMask waterMask;

   
    Vector3 gravity;
    [SerializeField] Vector3[] buoyancyOffsets = default;
    float[] submergence;


    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        submergence = new float[buoyancyOffsets.Length];
    }

    void FixedUpdate()
    {
        if (floatToSleep)
        {
            if (body.IsSleeping())
            {
                floatDelay = 0f;
                return;
            }

            if (body.velocity.sqrMagnitude < 0.0001f)
            {
                floatDelay += Time.deltaTime;
                if (floatDelay >= 1f)
                {
                    return;
                }
            }
            else
            {
                floatDelay = 0f;
            }
        }

        gravity = CustomGravity.GetGravity(body.position);

        float dragFactor = waterDrag * Time.deltaTime / buoyancyOffsets.Length;
        float buoyancyFactor = -buoyancy / buoyancyOffsets.Length;

        for (int i = 0; i < buoyancyOffsets.Length; i++)
        {
            if (submergence[i] > 0f)
            {
                float drag = Mathf.Max(0, 1f - dragFactor * submergence[i]);
                body.velocity *= drag;
                body.angularVelocity *= drag;
                body.AddForceAtPosition(gravity * (buoyancyFactor * submergence[i]), transform.TransformPoint(buoyancyOffsets[i]), ForceMode.Acceleration);

                submergence[i] = 0;
            }
        }

       
        body.AddForce(
           gravity, ForceMode.Acceleration
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((waterMask & (1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if ( !body.IsSleeping() &&(waterMask & (1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence();
        }
    }

    private void EvaluateSubmergence()
    {

        Vector3 down = gravity.normalized;
        Vector3 offset = down * -submergenceOffset;

        for (int i = 0; i < buoyancyOffsets.Length; i++)
        {
            Vector3 p = offset + transform.TransformPoint(buoyancyOffsets[i]);
            if (Physics.Raycast(
            p, down, out RaycastHit hit, submergenceRange + 1f,
            waterMask, QueryTriggerInteraction.Collide
            ))
            {
                submergence[i] = 1f - hit.distance / submergenceRange;
            }
            else if (!safeFloating || Physics.CheckSphere(p, 0.01f, waterMask, QueryTriggerInteraction.Collide)) 
            {
                submergence[i] = 1f;
            }
        }


    }
}
