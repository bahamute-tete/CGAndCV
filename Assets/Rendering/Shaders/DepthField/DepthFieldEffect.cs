using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
[ExecuteInEditMode,ImageEffectAllowedInSceneView]
public class DepthFieldEffect : MonoBehaviour
{
    const int circleOfConfusionPass = 0;
    const int bokehPass = 1;
    const int postFilterPass = 2;//postfilter pass 将执行小的高斯模糊


    [HideInInspector]
    public Shader dofShader;

    [NonSerialized]
    Material dofMaterial;


    [Range(0.1f, 100f)]
    public float focusDistance = 10f;


    //相对于焦距CoC 将在此范围内从0变为max
    [Range(0.1f, 10f)]
    public float focusRange = 3f;


    private void OnEnable()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }

    //需要从深度缓冲区中读取， MSAA效果无法正常工作。
    //因为我们将依赖深度缓冲区，所以效果不会考虑透明几何体。
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (dofMaterial == null)
        { 
            dofMaterial = new Material(dofShader);
            dofMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        //相对于焦距CoC 将在此范围内从零变为最大值
        dofMaterial.SetFloat("_FocusDistance", focusDistance);
        dofMaterial.SetFloat("_FocusRange", focusRange);

        //只需要存储一个值，所以使用单通道纹理就足够了
        //此缓冲区包含 CoC 数据，而不是颜色值。所以它应该始终被视为线性数据
        RenderTexture coc = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.RHalf,RenderTextureReadWrite.Linear);


        int width = source.width / 2;
        int height = source.height / 2;
        RenderTextureFormat format = source.format;

        RenderTexture dof0 = RenderTexture.GetTemporary(width, height, 0, format);
        RenderTexture dof1 = RenderTexture.GetTemporary(width,height, 0, format);   


        Graphics.Blit(source, coc, dofMaterial, circleOfConfusionPass);

        Graphics.Blit(source, dof0);
        Graphics.Blit(dof0, dof1, dofMaterial, bokehPass);
        Graphics.Blit (dof1, dof0,dofMaterial,postFilterPass);
        Graphics.Blit(dof0, destination);

        //Graphics.Blit(source, destination, dofMaterial, bokehPass);
        //Graphics.Blit(coc, destination);

        RenderTexture.ReleaseTemporary(coc);
        RenderTexture.ReleaseTemporary(dof0);
        RenderTexture.ReleaseTemporary(dof1);
    }
}
