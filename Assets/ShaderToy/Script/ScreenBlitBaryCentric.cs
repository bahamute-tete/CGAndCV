using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ScreenBlitBaryCentric : MonoBehaviour
{
    [SerializeField]
    private Material material;
    private Vector2 mousePos = new Vector2(0, 0);
    private int _MousePosID;

    private Vector3 barycentricPos = new Vector3();

    [SerializeField]
    private Slider p1x;
    [SerializeField]
    private Slider p1y;

    [SerializeField]
    private Slider p2x;
    [SerializeField]
    private Slider p2y;

    [SerializeField]
    private Slider p3x;
    [SerializeField]
    private Slider p3y;
    ////////////////////////
    [SerializeField]
    private Slider p1cr;
    [SerializeField]
    private Slider p1cg;
    [SerializeField]
    private Slider p1cb;

    [SerializeField]
    private Slider p2cr;
    [SerializeField]
    private Slider p2cg;
    [SerializeField]
    private Slider p2cb;

    [SerializeField]
    private Slider p3cr;
    [SerializeField]
    private Slider p3cg;
    [SerializeField]
    private Slider p3cb;

    [SerializeField]
    private Text barycentricText;

    [SerializeField]
    private Slider rotationSpeedSlider;


    private Vector2 p1 = new Vector2(-3.0f, -1.5f);
    private Vector2 p2 = new Vector2(-1.0f, 1.0f);
    private Vector2 p3 = new Vector2(1.0f, -1f);
    private Vector3 p1c = new Vector3(1.0f,0.0f,0.0f);
    private Vector3 p2c = new Vector3(0.0f, 1.0f, 0.0f);
    private Vector3 p3c = new Vector3(0.0f, 0.0f, 1.0f);
    private float speed = 0.0f;

    private float deltaTime = 0;

    private int p1PosID = 0;
    private int p2PosID = 0;
    private int p3PosID = 0;
    private int p1cPosID = 0;
    private int p2cPosID = 0;
    private int p3cPosID = 0;
    private int anitimeID = 0;
    private int deltaTimeID = 0;



    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //RenderTexture scrRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        if (material)
        {

            Graphics.Blit(source, destination, material);

        }
        else
        {
            Graphics.Blit(source, destination);
        }

    }

    private void OnEnable()
    {
        p1 = new Vector2(-2.0f, -1.0f);
        p2 = new Vector2(1.4f, -1.3f);
        p3 = new Vector2(0f, 1.5f);
        p1c = new Vector3(1.0f, 0.0f, 0.0f);
        p2c = new Vector3(0.0f, 1.0f, 0.0f);
        p3c = new Vector3(0.0f, 0.0f, 1.0f);

        p1x.value = p1.x;
        p1y.value = p1.y;
        p2x.value = p2.x;
        p2y.value = p2.y;
        p3x.value = p3.x;
        p3y.value = p3.y;

        p1cr.value = p1c.x;
        p1cg.value = p1c.y;
        p1cb.value = p1c.z;

        p2cr.value = p2c.x;
        p2cg.value = p2c.y;
        p2cb.value = p2c.z;

        p3cr.value = p3c.x;
        p3cg.value = p3c.y;
        p3cb.value = p3c.z;

        barycentricText.text = barycentricPos.ToString();
        rotationSpeedSlider.value = speed;
    }
    // Start is called before the first frame update
    void Start()
    {

        _MousePosID = Shader.PropertyToID("_MousePos");
        mousePos = new Vector2(0, 0);

        p1PosID = Shader.PropertyToID("_p1");
        p2PosID = Shader.PropertyToID("_p2");
        p3PosID = Shader.PropertyToID("_p3");
        p1cPosID = Shader.PropertyToID("_p1c");
        p2cPosID = Shader.PropertyToID("_p2c");
        p3cPosID = Shader.PropertyToID("_p3c");

       anitimeID = Shader.PropertyToID("_Speed");
       deltaTimeID = Shader.PropertyToID("_deltaTime");




        p1x.onValueChanged.AddListener(delegate {
            p1.x = p1x.value;
        });

        p1y.onValueChanged.AddListener(delegate {
            p1.y = p1y.value;
        });

        p2x.onValueChanged.AddListener(delegate {
            p2.x = p2x.value;
        });

        p2y.onValueChanged.AddListener(delegate {
            p2.y = p2y.value;
        });

        p3x.onValueChanged.AddListener(delegate {
            p3.x = p3x.value;
        });

        p3y.onValueChanged.AddListener(delegate {
            p3.y = p3y.value;
        });



        p1cr.onValueChanged.AddListener(delegate {
            p1c.x = p1cr.value;
        });

        p1cg.onValueChanged.AddListener(delegate {
            p1c.y = p1cg.value;
        });
        p1cb.onValueChanged.AddListener(delegate {
            p1c.z = p1cb.value;
        });


        p2cr.onValueChanged.AddListener(delegate {
            p2c.x = p2cr.value;
        });

        p2cg.onValueChanged.AddListener(delegate {
            p2c.y = p2cg.value;
        });
        p2cb.onValueChanged.AddListener(delegate {
            p2c.z = p2cb.value;
        });


        p3cr.onValueChanged.AddListener(delegate {
            p3c.x = p3cr.value;
        });

        p3cg.onValueChanged.AddListener(delegate {
            p3c.y = p3cg.value;
        });
        p3cb.onValueChanged.AddListener(delegate {
            p3c.z = p3cb.value;
        });


        rotationSpeedSlider.onValueChanged.AddListener(delegate
        {
            speed = rotationSpeedSlider.value;
        });

    }

    // Update is called once per frame
    void Update()
    {
        deltaTime = Time.deltaTime;

        if (Input.GetMouseButton(0))
        {
            mousePos = Input.mousePosition;
            if (mousePos.x > Screen.width)
            {
                mousePos.x = Screen.width;
            }
            if (mousePos.x < 0)
            {
                mousePos.x = 0;
            }

            if (mousePos.y > Screen.height)
            {
                mousePos.y = Screen.height;
            }
            if (mousePos.y < 0)
            {
                mousePos.y =0;
            }

            barycentricText.rectTransform.position = mousePos;
           

            mousePos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
            Vector2 mouseInShader = new Vector2(2 * mousePos.x - 1, 2 * mousePos.y - 1);

            barycentricPos = Barycentric(mousePos, p1, p2, p3);
            barycentricText.text = barycentricPos.ToString("##.###");

            material.SetVector(_MousePosID, mouseInShader);
        }

        material.SetVector("_p1", p1);
        material.SetVector("_p2", p2);
        material.SetVector("_p3", p3);
        material.SetVector("_p1c", p1c);
        material.SetVector("_p2c", p2c);
        material.SetVector("_p3c", p3c);


       

        material.SetFloat("_Speed", speed);
        material.SetFloat("_deltaTime", deltaTime);
    }

    Vector3 Barycentric(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector3 vp =new Vector3(p.x,p.y, 0), va =new Vector3(a.x,a.y ,0), vb =new Vector3(b.x,b.y, 0), vc =new Vector3(c.x,c.y, 0);
        Vector3 pa = vp - va, pb = vp - vb, pc = vp - vc;
        Vector3 ba = vb - va, cb = vc - vb, ac = va - vc;

        float beta = 0;
        float gama = 0;
        float alpha = 0;
        
        Vector3 n = Vector3.Cross(ba, -ac);
        alpha = Vector3.Dot(n, Vector3.Cross(ba, pa)) / (Vector3.Magnitude(n) * Vector3.Magnitude(n));
        beta = Vector3.Dot(n, Vector3.Cross(cb, pb)) / (Vector3.Magnitude(n) * Vector3.Magnitude(n));
        gama = Vector3.Dot(n, Vector3.Cross(ac, pc)) / (Vector3.Magnitude(n) * Vector3.Magnitude(n));

        return new Vector3(alpha, beta, gama);
    }
}
