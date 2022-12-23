using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class UVScale : MonoBehaviour
{
    BoxCollider boxCollider => GetComponent<BoxCollider>();
    [SerializeField] float ScreenAspect = 0;
     public enum ScrennType {Horizon,Vertical };
    public ScrennType st = ScrennType.Horizon;


    public Texture2D pic;
    int w, h;
    public float picAspect = 0;

    static int screenID = Shader.PropertyToID("_ScreenAspect");
    static int picID = Shader.PropertyToID("_TexAspect");
    static int TexID = Shader.PropertyToID("_MainTex");


    Material mat => GetComponent<Renderer>().sharedMaterial;
    //Material kkk = new Material(Shader.Find("......."));

    // Start is called before the first frame update
    void Start()
    {
      

       
        
    }

    // Update is called once per frame
    void Update()
    {
        w = pic.width;
        h = pic.height;

        Vector3 size = boxCollider.size;
        float[] sizes = new float[3] { size.x, size.y, size.z };
        var axies = sizes.OrderByDescending(x => x).ToArray();


        ScreenAspect = (st == ScrennType.Horizon) ? axies[0] / axies[1] : axies[1] / axies[0];


        if (w > h)
        {
            mat.DisableKeyword("_VERTICAL");
            mat.EnableKeyword("_HORIZON");
            picAspect = (float)w / h;
        }
        else
        {
            mat.DisableKeyword("_HORIZON");
            mat.EnableKeyword("_VERTICAL");
           
            picAspect = (float)h / w;
            
        }


        mat.SetFloat(screenID, ScreenAspect);
        mat.SetFloat(picID, picAspect);
        mat.SetTexture(TexID, pic);
    }
}
