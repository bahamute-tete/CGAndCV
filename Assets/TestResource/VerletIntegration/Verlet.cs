using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Particle
{
    public Vector3 position;
    public Vector3 prePosition;
    public bool isLocked;
}

public class Stick
{
    public Particle particalA;
    public Particle particalB;
    public float length;

    public Stick(Particle a, Particle b)
    {
        particalA = a;
        particalB = b;
        length = Vector3.Magnitude(a.position - b.position);
    }
}



public class Verlet : MonoBehaviour
{
   
    [SerializeField] Vector3 accleration;
    [SerializeField] int stiffness = 2;
    [SerializeField] float k = 0.2f;
    [SerializeField] private bool startPointLock;
    [SerializeField] private bool endPointLock;
    [SerializeField] float damping = 10f;

    List<Particle> particles = new List<Particle>();
    List<Stick> sticks = new List<Stick>();
    LineRenderer lr;


    private void Awake()
    {
       
    }
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }



    private void FixedUpdate()
    {
        Simulation();
    }
    
    private void LateUpdate()
    {
       Render();
    }

    void Initialize()
    {
        lr = GetComponent<LineRenderer>();

        for (int i = 0; i < lr.positionCount; i++)
        {
            lr.SetPosition(i, new Vector3(i*5f/ lr.positionCount, 0, 0));
        }

        for (int i = 0; i < lr.positionCount; i++)
        {
            Vector3 intialPos = lr.GetPosition(i);
            particles.Add(new Particle() { position = intialPos, prePosition = intialPos });
        }

        for (int j = 0; j < particles.Count - 1; j++)
        {
            Stick stick = new Stick(particles[j], particles[j + 1]);
            sticks.Add(stick);
        }

        if (startPointLock)
        {
            particles[0].isLocked = true;
        }

        if (endPointLock)
        {
            particles[particles.Count - 1].isLocked = true;
        }
    }


    void Simulation()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            Particle p = particles[i];

            if (!p.isLocked)
            {
                Vector3 temp = p.position;
                p.position = p.position +(1-damping)* (p.position - p.prePosition) + accleration * Time.fixedDeltaTime * Time.fixedDeltaTime;
                p.prePosition = temp;
            }

        }

        for (int i = 0; i < stiffness; i++)
        {
            for (int j = 0; j < sticks.Count; j++)
            {
                Stick s = sticks[j];

                

                Vector3 delta = s.particalB.position - s.particalA.position;
                float length = delta.magnitude;

                //Vector3 force = -k * delta.normalized * (length - s.length);
                //force += Physics.gravity;
                //Vector3 a = force;

                float diff = (length - s.length) / length;

                if (!s.particalA.isLocked)
                    s.particalA.position += 0.5f * diff * delta;
                if (!s.particalB.isLocked)
                    s.particalB.position -= 0.5f * diff * delta;
            }
        }

        particles[0].position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,10f));
    }


    void Render()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            lr.SetPosition(i, particles[i].position);
        }
    }
}

