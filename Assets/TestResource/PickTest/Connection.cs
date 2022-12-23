using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Connection : MonoBehaviour
{

    [SerializeField]
    Image g1;
    [SerializeField]
    Image g2;
    [SerializeField]
    Image g3;
    [SerializeField]
    Image g4;

    [SerializeField]
    Material lmat;
    [Range(0.5f,1.0f)]
    public float lineWidth;

    GameObject lr1;
    GameObject lr2;
    GameObject lr3;
    GameObject lr4;

    LineRenderer l1= new LineRenderer();
    LineRenderer l2 = new LineRenderer();
    LineRenderer l3 = new LineRenderer();
    //LineRenderer l4 = new LineRenderer();

    // Start is called before the first frame update
    void Start()
    {

         lr1 = new GameObject("lr1");
         lr2 = new GameObject("lr2");
         lr3 = new GameObject("lr3");
         lr4 = new GameObject("lr4");

        l1 = lr1.AddComponent<LineRenderer>();
        l2 = lr2.AddComponent<LineRenderer>();
        l3 = lr3.AddComponent<LineRenderer>();
       // l4 = lr4.AddComponent<LineRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        l1.SetPosition(0, g1.transform.position);
        l1.SetPosition(1, g2.transform.position);

        l2.SetPosition(0, g2.transform.position);
        l2.SetPosition(1, g3.transform.position);

        l3.SetPosition(0, g1.transform.position);
        l3.SetPosition(1, g4.transform.position);

        l1.material = lmat;
        l2.material = lmat;
        l3.material = lmat;

        l1.startWidth = lineWidth;
        l2.startWidth = lineWidth;
        l3.startWidth = lineWidth;

        l1.endWidth = lineWidth;
        l2.endWidth = lineWidth;
        l3.endWidth = lineWidth;

        l1.sortingOrder = 2;
        l2.sortingOrder = 2;
        l3.sortingOrder = 2;

    }
}
