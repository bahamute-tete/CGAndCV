using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode,ImageEffectAllowedInSceneView]
public class BloomEffectTest : MonoBehaviour
{
    const int BoxDownPrefilterPass = 0;
    const int BoxDownPass = 1;
    const int BoxUpPass = 2;
    const int ApplyBloomPass = 3;
    const int DebugBloomPass = 4; 

    public Shader bloomShader;

    [Range(0, 10)]
    public float intensity = 1;
    [Range(1, 16)]
    public int iterations = 4;
    [Range(1, 10)]
    public float threshold = 1.0f;

    //At 0, harsh transition.
    //At 1, soft threshold that smoothly fades the bloom 
    [Range(0, 1)]
    public float softThreshold = 0.5f;

    public bool debug;

    RenderTexture[] textures = new RenderTexture[16];

    [NonSerialized]
    Material bloom;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (bloom == null)
        {
            bloom = new Material(bloomShader);
            bloom.hideFlags = HideFlags.HideAndDontSave;
        }

        bloom.SetFloat("_Threshold", threshold);
        bloom.SetFloat("_SoftThreshold", softThreshold);
        //通常使用伽马空间中的一个因子来设置光晕的强度，因此将其从伽马空间转换为线性空间。
        bloom.SetFloat("_Intensity", Mathf.GammaToLinearSpace(intensity));


        int width = source.width / 2;
        int height = source.height / 2;
        RenderTextureFormat format = source.format;

        RenderTexture currentDestination = textures[0] =
            RenderTexture.GetTemporary(width, height, 0, format);
        Graphics.Blit(source, currentDestination, bloom, BoxDownPrefilterPass);
        RenderTexture currentSource = currentDestination;

        int i = 1;
        for (; i < iterations; i++)
        {
            width /= 2;
            height /= 2;
            if (height < 2)
            {
                break;
            }
            currentDestination = textures[i] =
                RenderTexture.GetTemporary(width, height, 0, format);
            Graphics.Blit(currentSource, currentDestination, bloom, BoxDownPass);
            currentSource = currentDestination;
        }

        for (i -= 2; i >= 0; i--)
        {
            currentDestination = textures[i];
            textures[i] = null;
            Graphics.Blit(currentSource, currentDestination, bloom, BoxUpPass);
            RenderTexture.ReleaseTemporary(currentSource);
            currentSource = currentDestination;
        }


        if (debug)
        {
            //默认的 Unity 场景非常明亮。除了定向光之外，还有环境照明和反射会影响最终的像素颜色。
            //所有这些一起可以导致亮度值大于 1。所以在Debug里面会有一些白色的表面出现
            Graphics.Blit(currentSource, destination, bloom, DebugBloomPass);
        }
        else
        { 
            bloom.SetTexture("_SourceTex", source);
            Graphics.Blit(currentSource, destination, bloom, ApplyBloomPass);
        }
        RenderTexture.ReleaseTemporary(currentSource);
    }

}



