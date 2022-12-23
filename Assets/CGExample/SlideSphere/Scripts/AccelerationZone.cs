
using System;
using UnityEngine;

public class AccelerationZone : MonoBehaviour
{

    [SerializeField, Min(0f)] float speed = 10f, acceleration = 10f;


    private void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body)
        {
            Accelerate(body);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body)
        {
            Accelerate(body);
        }
    }

    private void Accelerate(Rigidbody body)
    {
        Vector3 velocity = transform.InverseTransformDirection(body.velocity);
        if (body.TryGetComponent(out MovingSphere sphere))
        {
            sphere.PreventSnapGround();
        }


        if (velocity.y >= speed)
        {
            return;
        }

        if (acceleration > 0)
        {
            velocity.y = Mathf.MoveTowards(velocity.y, speed, acceleration * Time.deltaTime);
        }
        else
        {
            velocity.y = speed;
        }
       
        body.velocity = transform.TransformDirection(velocity);
    }
}
