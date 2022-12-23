using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTest : MonoBehaviour
{
	public float maxSpeed = 10f;
	public float maxAcceleration = 10f;

	Vector3 velocity;

	Vector3 playerInput;
	RaycastHit hit;
	bool isCollider = false;
	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		Move();
	   
	}

	void Move()
	{
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");

		playerInput = new Vector3(x, 0f, z);
		playerInput.Normalize();
		playerInput = Vector3.ClampMagnitude(playerInput, 1f);



		//Vector3 acceleration = playerInput * maxSpeed;
		//velocity += acceleration * Time.deltaTime;

		
		

		Vector3 desierVelocity = playerInput * maxSpeed;
		float maxSpeedChange = maxAcceleration *Time.deltaTime;

		//velocity += acceleration * Time.deltaTime;

		//Vector3 velocity = maxSpeed* playerInput ;

	   


		//if (velocity.x < desierVelocity.x)
		//    velocity.x = Mathf.Min(velocity.x + maxSpeedChange, desierVelocity.x);
		//else if (velocity.x > desierVelocity.x)
		//    velocity.x = Mathf.Min(velocity.x - maxSpeedChange, desierVelocity.x);

		velocity.x = Mathf.MoveTowards(velocity.x, desierVelocity.x, maxSpeedChange);
		velocity.z = Mathf.MoveTowards(velocity.z, desierVelocity.z, maxSpeedChange);


 
		collisionTest();
		

		Vector3 displacement = velocity * Time.deltaTime;
		transform.localPosition += displacement;
	}

	void collisionTest()
	{
		for (int i = 0; i < 360; i += 2)
		{
			float angel = i * Mathf.Deg2Rad;
			Vector3 dir = new Vector3(Mathf.Cos(angel), 0, Mathf.Sin(angel));
			Ray ray = new Ray(transform.position, dir);

			bool isHit = Physics.Raycast(ray, out hit, 0.5f);
			if (isHit && hit.transform.tag.Equals("Wall"))
			{
				if (Vector3.Dot(velocity.normalized, hit.normal) < 0)
				{
					//Debug.Log("Hit<0");
					Vector3 slideVelocity = SlideDir(velocity) * maxSpeed;

					velocity.x = slideVelocity.x;
					velocity.z = slideVelocity.z;
				}
			}
			continue;
		}
	}


	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		//for (int i = 0; i < 360; i += 2)
		//{
		//    float angel = i * Mathf.Deg2Rad;
		//    Vector3 dir = new Vector3(Mathf.Cos(angel), 0, Mathf.Sin(angel));
		//    Gizmos.DrawLine(transform.position, transform.position + dir * 0.5f);
		//}
		Gizmos.DrawLine(transform.position, transform.position + velocity.normalized * 1f);

	}

	Vector3 SlideDir(Vector3 velocity)
	{
	    //ball center
		Vector3 q = transform.position;

		//desierDis
		Vector3 p2 = q + velocity * Time.deltaTime;

		//dis<0;
		float dis = Vector3.Dot(p2 - q, hit.normal);

		Vector3 p3 = p2 -dis * hit.normal;

		return (p3 - q).normalized;
	}
}
