using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ScreenBlitBezier2D : MonoBehaviour
{
    [SerializeField]
    private Material material;
    private Vector2 mousePos = new Vector2(0,0);
    private int _MousePosID;
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

    [SerializeField]
    private Slider p4x;
    [SerializeField]
    private Slider p4y;

    [SerializeField]
    private Slider timeSlider;
    [SerializeField]
    private Slider clipValue;

    [SerializeField]
    private Toggle gizimoToggle;
    [SerializeField]
    private Toggle giziAniToggle;
    [SerializeField]
    private Toggle hermiteToggle;
    [SerializeField]
    private Toggle cV3Toggle;
    [SerializeField]
    private Toggle cV4Toggle;
    [SerializeField]
    private Toggle originBezier;
    [SerializeField]
    private Toggle clipBezier;
    [SerializeField]
    private Toggle clipGizimo;
    [SerializeField]
    private Toggle catmull;

    [SerializeField]
    private Toggle blackBackGround;



    private Vector2 p1 = new Vector2(-3.0f, -1.5f);
    private Vector2 p2 = new Vector2(-1.0f,1.0f);
    private Vector2 p3 = new Vector2(1.0f, -1f);
    private Vector2 p4 = new Vector2(2.0f, 1.33f);
    private float anitime = 3.0f;
    private float clipvalue = .5f;
    private int isAnimation = 0;
    private int isShowOrigin = 1;
    private int isClipShow = 0;
    private int isClipGizimoShow = 0;

    private int p1PosID = 0;
    private int p2PosID = 0;
    private int p3PosID = 0;
    private int p4PosID = 0;
    private int anitimeID = 0;
    private int clipID = 0;
    private int isAnimationID = 0;
    private int isShowOriginID = 0;
    private int isClipShowID = 0;
    private int isClipGizimoShowID = 0;
    private int isShowCatmul_RomID = 0;


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //RenderTexture scrRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        if (material)
        {
           
            Graphics.Blit(source, destination, material);

        }else
        {
            Graphics.Blit(source, destination);
        }

    }

    private void Initialized()
    {
        p1 = new Vector2(-3.0f, -1.5f);
        p2 = new Vector2(-1.0f, 1.0f);
        p3 = new Vector2(1.0f, -1f);
        p4 = new Vector2(2.0f, 1.33f);

        p1x.value = p1.x;
        p1y.value = p1.y;

        p2x.value = p2.x;
        p2y.value = p2.y;

        p3x.value = p3.x;
        p3y.value = p3.y;

        p4x.value = p4.x;
        p4y.value = p4.y;

        timeSlider.value = anitime;

        clipValue.value = clipvalue;

        gizimoToggle.isOn = false;
        cV3Toggle.isOn = false;

        cV4Toggle.isOn = true;
        isShowOrigin = 1;
        isClipShow = 0;
        isClipGizimoShow = 0;
        originBezier.isOn = true;
        clipBezier.isOn = false;
        clipGizimo.isOn = false;
        material.SetInt("_showOrigin", 1);
        material.SetInt("_showClip", 0);
        material.SetInt("_clipBezierGizimo", 0);


        hermiteToggle.isOn = false;
        catmull.isOn = false;
        isAnimation = 0;
        blackBackGround.isOn = false; 
    }

    private void OnEnable()
    {
        p1PosID = Shader.PropertyToID("_p1");
        p2PosID = Shader.PropertyToID("_p2");
        p3PosID = Shader.PropertyToID("_p3");
        p4PosID = Shader.PropertyToID("_p4");

        anitimeID = Shader.PropertyToID("_timelength");
        clipID = Shader.PropertyToID("_ClipValue");
        isAnimationID = Shader.PropertyToID("_isAnimation");
        isShowOriginID = Shader.PropertyToID("_showOrigin");
        isClipShowID = Shader.PropertyToID("_showClip");
        isClipGizimoShowID = Shader.PropertyToID("_clipBezierGizimo");
        isShowCatmul_RomID = Shader.PropertyToID("_showCatmull_Rom");

    }
    // Start is called before the first frame update
    void Start()
    {

        Initialized();

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

        p4x.onValueChanged.AddListener(delegate {
            p4.x = p4x.value;
        });

        p4y.onValueChanged.AddListener(delegate {
            p4.y = p4y.value;
        });


        timeSlider.onValueChanged.AddListener(delegate {
            anitime = timeSlider.value;
        });

        clipValue.onValueChanged.AddListener(delegate {
            clipvalue = clipValue.value;
        });

        gizimoToggle.onValueChanged.AddListener(delegate
        {
            if (gizimoToggle.isOn)
            {
                material.EnableKeyword("_GIZIMO_ON");
            }
            else
            {
                material.DisableKeyword("_GIZIMO_ON");
            }
        });

        giziAniToggle.onValueChanged.AddListener(delegate
        {
            if (giziAniToggle.isOn)
            {
                material.SetInt("_isAnimation",1);
            }
            else
            {
                material.SetInt("_isAnimation", 0);
            }
        });

        cV3Toggle.onValueChanged.AddListener(delegate
        {
            if (cV3Toggle.isOn)
            {
                material.EnableKeyword("_RANK3_ON");
            }
            else
            {
                material.DisableKeyword("_RANK3_ON");
            }
        });

        cV4Toggle.onValueChanged.AddListener(delegate
        {
            if (cV4Toggle.isOn)
            {
                material.EnableKeyword("_RANK4_ON");
            }
            else
            {
                material.DisableKeyword("_RANK4_ON");
            }
        });
        originBezier.onValueChanged.AddListener(delegate
        {
            if (originBezier.isOn)
            {
                material.SetInt("_showOrigin", 1);
            }
            else
            {
                material.SetInt("_showOrigin", 0); ;
            }
        });
        clipBezier.onValueChanged.AddListener(delegate
        {
            if (clipBezier.isOn)
            {
                material.SetInt("_showClip", 1);
            }
            else
            {
                material.SetInt("_showClip", 0); ;
            }
        });
        clipGizimo.onValueChanged.AddListener(delegate
        {
            if (clipGizimo.isOn)
            {
                material.SetInt("_clipBezierGizimo", 1);
            }
            else
            {
                material.SetInt("_clipBezierGizimo", 0); 
            }
        });

        hermiteToggle.onValueChanged.AddListener(delegate
        {
            if (hermiteToggle.isOn)
            {
                material.EnableKeyword("_HERMITE_ON");
            }
            else
            {
                material.DisableKeyword("_HERMITE_ON");
            }
        });

        catmull.onValueChanged.AddListener(delegate
        {
            if (catmull.isOn)
            {
                material.SetInt("_showCatmull_Rom", 1);
            }
            else
            {
                material.SetInt("_showCatmull_Rom", 0);
            }
        });

        blackBackGround.onValueChanged.AddListener(delegate
        {
            if (blackBackGround.isOn)
            {
                material.EnableKeyword("_BACKGROUND");
            }
            else
            {
                material.DisableKeyword("_BACKGROUND");
            }
        });


    }

    // Update is called once per frame
    void Update()
    {

        material.SetVector("_p1", p1);
        material.SetVector("_p2", p2);
        material.SetVector("_p3", p3);
        material.SetVector("_p4", p4);

        material.SetFloat("_timelength", anitime);
        material.SetFloat("_ClipValue", clipvalue);

        if (cV4Toggle.isOn)
        {
            clipValue.gameObject.SetActive(true);
            originBezier.gameObject.SetActive(true);
            clipBezier.gameObject.SetActive(true);

            if (clipBezier.isOn)
            {
                clipGizimo.gameObject.SetActive(true);
                clipValue.gameObject.SetActive(true);
            }
            else
            {
                clipGizimo.gameObject.SetActive(false);
                clipValue.gameObject.SetActive(false);
            }

        }
        else
        {
            originBezier.gameObject.SetActive(false);
            clipBezier.gameObject.SetActive(false);
            clipGizimo.gameObject.SetActive(false);
            clipValue.gameObject.SetActive(false);

        }

        if (gizimoToggle.isOn)
        {
            giziAniToggle.gameObject.SetActive(true);
        }
        else
        {
            giziAniToggle.gameObject.SetActive(false);
        }
        


    }
}
