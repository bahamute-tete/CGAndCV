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
    const int postFilterPass = 2;//postfilter pass ��ִ��С�ĸ�˹ģ��


    [HideInInspector]
    public Shader dofShader;

    [NonSerialized]
    Material dofMaterial;


    [Range(0.1f, 100f)]
    public float focusDistance = 10f;


    //����ڽ���CoC ���ڴ˷�Χ�ڴ�0��Ϊmax
    [Range(0.1f, 10f)]
    public float focusRange = 3f;


    private void OnEnable()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }

    //��Ҫ����Ȼ������ж�ȡ�� MSAAЧ���޷�����������
    //��Ϊ���ǽ�������Ȼ�����������Ч�����ῼ��͸�������塣
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (dofMaterial == null)
        { 
            dofMaterial = new Material(dofShader);
            dofMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        //����ڽ���CoC ���ڴ˷�Χ�ڴ����Ϊ���ֵ
        dofMaterial.SetFloat("_FocusDistance", focusDistance);
        dofMaterial.SetFloat("_FocusRange", focusRange);

        //ֻ��Ҫ�洢һ��ֵ������ʹ�õ�ͨ��������㹻��
        //�˻��������� CoC ���ݣ���������ɫֵ��������Ӧ��ʼ�ձ���Ϊ��������
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
