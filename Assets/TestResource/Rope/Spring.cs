using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    public Mass mass_a;
    public Mass mass_b;
    float restLength;
    public float ks;

    public float kd;


    LineRenderer lr;
    // Start is called before the first frame update
    void Start()
    {
        restLength = (mass_b.transform.position - mass_a.transform.position).magnitude;


         lr=gameObject.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lr.startColor = lr.endColor = Color.cyan;
        lr.positionCount = 2;
        lr.startWidth = lr.endWidth = 0.01f;
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public  void Simulate()
   {
        Vector3 pos_ab = mass_b.transform.position - mass_a.transform.position;
        Vector3 f_ab = ks * pos_ab.normalized * (pos_ab.magnitude - restLength);


        //damping
        Vector3 v_ab = mass_a.v - mass_b.v;
        Vector3 d_ab = -kd * pos_ab.normalized * Vector3.Dot(v_ab, pos_ab.normalized);

        

        mass_a.F += f_ab+d_ab;
        mass_b.F += -f_ab+d_ab;


        lr.SetPosition(0, mass_a.transform.position);
        lr.SetPosition(1, mass_b.transform.position);


    }
}
