using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBDTEST : MonoBehaviour
{
    public Transform target;
    public Transform endPoint;
    public float mass = 0.1f;
    public float powerIntensity = 5f;
    public float dragIntensity = 0.3f;//air damp
    public Vector3 wind;

   

    Vector3 v;
    Vector3 nextPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(nextPos, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(nextPos + endPoint.localPosition, 0.1f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        Gizmos.color = Color.yellow;
        Vector3 dir = (nextPos + endPoint.localPosition - transform.position).normalized;
        Gizmos.DrawWireSphere(dir, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dis = target.position - nextPos;
        Vector3 f = powerIntensity * dis;

        //f = 0.5 * C * ro * S * pow(V,2) air Force
        Vector3 air = -v.normalized * v.sqrMagnitude * dragIntensity;

        
        f += air+Physics.gravity*mass;

        f += wind.normalized * Random.Range(-0.4f, 1.0f);//模拟简单风力

        Vector3 a = f / mass;
        v += a * Time.deltaTime;
        nextPos += v * Time.deltaTime;

        Vector3 down = -Vector3.up;
        Vector3 dir = (nextPos + endPoint.localPosition - transform.position).normalized;

     

        transform.rotation =Quaternion.Slerp(transform.rotation, Quaternion.FromToRotation(down, dir),Time.deltaTime*30f);
        
        transform.position = target.position;
    }
}
