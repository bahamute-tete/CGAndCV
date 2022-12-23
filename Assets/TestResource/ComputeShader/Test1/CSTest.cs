using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using UnityEngine.UI;


public class CSTest : MonoBehaviour
{
    public RawImage ri;
    public RawImage oringalTex;
    public RawImage BlurTex;



    public ComputeShader cs;
    public Material mat;
    public Texture2D tex;

    float[,] kernal;
    List<float[]> sepreate = new List<float[]>();
    float sum =0;

    int kernalSize;
    float[] horizon;
    float[] vertical;

    Texture2D kernalTex;

    private void Awake()
    {
        //kernal = Average(21, 21);
        kernal = Gaussian(141, 141,23.5f);
        sepreate = Sepreate(kernal, out sum);

        
        kernalSize = sepreate[0].Length;
        kernalTex = new Texture2D(kernalSize, kernalSize, TextureFormat.RGB24,false);

        for (int i = 0; i < kernalSize; i++)
        {
            for (int j = 0; j < kernalSize; j++)
            {
                kernalTex.SetPixel(i, j, new Color(kernal[i, j], kernal[i, j], kernal[i, j]));
                //Debug.Log(kernal[i, j]);
            }
        }

        kernalTex.Apply();

        ri.texture = kernalTex;

        oringalTex.texture = tex;
        oringalTex.SetNativeSize();

    }
    // Start is called before the first frame update
    void Start()
    {
        RenderTexture rt = new RenderTexture(tex.width, tex.height, 16);
        rt.enableRandomWrite = true;
        rt.Create();

        int index = cs.FindKernel("CSMain");

        cs.SetTexture(index, "kernelTex", kernalTex);
        cs.SetInt("kernalSize", kernalSize);
        cs.SetTexture(index, "Input", tex);

        cs.SetTexture(index, "Result", rt);

        //mat.mainTexture = rt;

        cs.Dispatch(index, tex.width / 8, tex.height / 8, 1);

        BlurTex.texture = rt;
        BlurTex.SetNativeSize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    static float[,] Gaussian(float r, float c, float sig)
    {
        int m = (int)r;
        int n = (int)c;

        float[,] LPF = new float[m, n];
        int midr = (m - 1) / 2;
        int midc = (n - 1) / 2;
        //float K = 1f / (2f * PI * Pow(sig, 2));
        float K = 1f;

        for (int s = 0; s < m; s++)
        {
            for (int t = 0; t < n; t++)
            {
                float squareR = Pow(s - midr, 2) + Pow(t - midc, 2);
                float index = -squareR / (2f * Pow(sig, 2));
                LPF[s, t] = K * Exp(index);

            }
        }
        return LPF;
    }

    static List<float[]> Sepreate(float[,] f, out float sum)
    {

        int r = f.GetUpperBound(0) + 1;
        int c = f.GetUpperBound(1) + 1;

        List<float[]> kernels = new List<float[]>();
        kernels.Clear();
        float[] horizon = new float[r];
        float[] vertical = new float[c];

        int midr = (r - 1) / 2;
        int midc = (c - 1) / 2;

        sum = 0;

        for (int s = 0; s < r; s++)
        {
            for (int t = 0; t < c; t++)
            {
                sum += f[s, t];
            }
        }

        for (int s = 0; s < r; s++)
        {
            horizon[s] = f[s, midc];
            //Debug.Log("horizon[" + s + "]== " + horizon[s]);
        }

        for (int t = 0; t < c; t++)
        {
            vertical[t] = f[midr, t];
            //Debug.Log("vertical[" + t + "]== " + vertical[t]);
        }

        kernels.Add(horizon); kernels.Add(vertical);
        //Debug.Log("sum = " + sum);
        return kernels;
    }

    static float[,] Average(float r, float c)
    {
        int m = (int)r;
        int n = (int)c;

        float[,] LPF = new float[m, n];
        for (int s = 0; s < m; s++)
        {
            for (int t = 0; t < n; t++)
            {
                LPF[s, t] = 1f;
            }
        }
        return LPF;
    }
}
