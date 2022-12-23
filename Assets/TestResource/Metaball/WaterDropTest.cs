using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDropTest : MonoBehaviour
{

    [SerializeField] GameObject waterDrop;
    [SerializeField] Transform Emiter;
    [SerializeField] int maxCount=100;
    [SerializeField]float radius = 1f;

    List<GameObject> waters = new List<GameObject>();

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CreateWaterDrop());
    }


    private IEnumerator CreateWaterDrop()
    {

        WaitForSeconds wait = new WaitForSeconds(0.01f);

        while (waters.Count < maxCount)
        {
            Vector2 pos2D = Random.insideUnitCircle;
            Vector3 dir = new Vector3(pos2D.x, pos2D.y, 0).normalized;

            Vector3 InitialPos = Emiter.transform.position + dir * radius;

            GameObject temp = Instantiate(waterDrop, InitialPos, Quaternion.identity, Emiter);
            waters.Add(temp);

            yield return wait;
        }
        
    }


    private IEnumerator Test()
    {

        WaitForSeconds wait = new WaitForSeconds(1f);
        Debug.Log("1111");
        yield return wait;
    }
    // Update is called once per frame
    void Update()
    {
        if (waters.Count >= maxCount)
        {
            StopCoroutine(CreateWaterDrop());
        }
    }
}
