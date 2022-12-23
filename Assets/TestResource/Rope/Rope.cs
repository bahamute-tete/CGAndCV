using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{

	public float simulateStep = 0.01f;
	public int massCount = 5;
	List<Spring> allSprings;
	Mass[] allMass;

	public float ks = 1f;
	public float kd = 0.1f;
	public float drag = 0.01f;



	
	// Start is called before the first frame update
	void Start()
	{
		allMass = new Mass[massCount];
		float disStep = 0.1f;
		float massSize = 0.01f;


		for (int i = 0; i < massCount; i++)
		{
			var item = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<Mass>();
		   
			item.transform.SetParent(transform);
			item.transform.localScale = Vector3.one * massSize;
			Destroy(item.GetComponent<Collider>());
			item.transform.localPosition = Vector3.right * disStep * i;
			item.drag = drag;
			allMass[i] = item;
		}


		allSprings = new List<Spring>();
		for (int  i= 0;  i< massCount-1; i++)
		{
			var sp = allMass[i].gameObject.AddComponent<Spring>();
			sp.mass_a = sp.GetComponent<Mass>();
			sp.mass_b = allMass[i + 1].GetComponent<Mass>();
			sp.ks = ks;
			sp.kd = kd;
			allSprings.Add(sp);
		}

		allMass[0].isStaticPos = true;


	}

	// Update is called once per frame
	void Update()
	{
		var dt = simulateStep;

		for (int i = 0, len = allSprings.Count; i < len; i++)
		{
			allSprings[i].Simulate();
		}

		for (int i = 0; i < massCount; i++)
		{
			allMass[i].Simulate(dt);
		}

	}
}
