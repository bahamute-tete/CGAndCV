using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DeferredFogEffect : MonoBehaviour
{
    public Shader deferredFog;

     Material fogMat;

    Camera deferredCamera => GetComponent<Camera>();
    Vector3[] frustumCorners;

    Vector4[] vectorArray;

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (fogMat == null)
        {
            
            frustumCorners = new Vector3[4];
            vectorArray = new Vector4[4];
            fogMat = new Material(deferredFog);
        }

        deferredCamera.CalculateFrustumCorners(
        new Rect(0f, 0f, 1f, 1f),
        deferredCamera.farClipPlane,
        deferredCamera.stereoActiveEye,
        frustumCorners
        );

        //The CalculateFrustumCorners orders them
        //bottom-left, top-left, top-right, bottom-right.
        //However, the quad used to render the image effect has its corner vertices ordered
        //bottom-left, bottom-right, top-left, top-right. So let's reorder them to match the quad's vertices.
        vectorArray[0] = frustumCorners[0];
        vectorArray[1] = frustumCorners[3];
        vectorArray[2] = frustumCorners[1];
        vectorArray[3] = frustumCorners[2];

        fogMat.SetVectorArray("_FrustumCorners", vectorArray);

        Graphics.Blit(source, destination, fogMat);
    }
}
