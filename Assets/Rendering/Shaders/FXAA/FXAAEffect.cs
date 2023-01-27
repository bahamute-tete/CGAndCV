using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// FXAA works by selectively reducing the contrast of the image, 
/// smoothing out visually obvious jaggies and isolated pixels. 
/// Contrast is determined by comparing the light intensity of pixels. 
/// The exact colors of pixels doesn't matter, it's their luminance that counts. 
/// Effectively, FXAA works on a grayscale image containing only the pixel brightness. 
/// This means that hard transitions between 
/// different colors won't be smoothed out much when their luminance is similar. 
/// Only visually obvious transitions are strongly affected.
/// </summary>
[ExecuteInEditMode,ImageEffectAllowedInSceneView]
public class FXAAEffect : MonoBehaviour
{
   


    [HideInInspector]
    public Shader fxaaShader;

    [NonSerialized]
    Material fxaaMaterial;

    // the luminance data has to be put in the alpha channel by an earlier pass
    //calculate luminance  would be expensive
    //or  FXAA can use green as luminance instead
    public enum LuminanceMode { Alpha,Green,Calculate}
    public LuminanceMode luminancesource;

    const int luminancePass = 0;
    const int fxaaPass = 1;


    // Trims the algorithm from processing darks.
    //   0.0833 - upper limit (default, the start of visible unfiltered edges)
    //   0.0625 - high quality (faster)
    //   0.0312 - visible limit (slower)
    [Range(0.0312f, 0.0833f)]
    public float contrastThreshold = 0.0312f;

    // The minimum amount of local contrast required to apply algorithm.
    //   0.333 - too little (faster)
    //   0.250 - low quality
    //   0.166 - default
    //   0.125 - high quality 
    //   0.063 - overkill (slower)
    [Range(0.063f, 0.333f)]
    public float relativeThreshold = 0.063f;

    // It affects high-contrast edges, but also a lot of lower-contrast details in our textures.
    // While this helps mitigate fireflies, the blurriness can be considered too much. 
    // The strength of this effect can by tuned via a 0¨C1 range factor to modulate the final offset.
    //// Choose the amount of sub-pixel aliasing removal.
    // This can effect sharpness.
    //   1.00 - upper limit (softer)
    //   0.75 - default amount of filtering
    //   0.50 - lower limit (sharper, less sub-pixel aliasing removal)
    //   0.25 - almost off
    //   0.00 - completely off
    [Range(0f, 1f)]
    public float subpixelBlending = 1f;

    public bool lowQuality;
    public bool gammaBlending;
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (fxaaMaterial == null)
        { 
            fxaaMaterial = new Material(fxaaShader);
            fxaaMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        fxaaMaterial.SetFloat("_ContrastThreshold", contrastThreshold);
        fxaaMaterial.SetFloat("_RelativeThreshold", relativeThreshold);
        fxaaMaterial.SetFloat("_SubpixelBlending", subpixelBlending);

        if (lowQuality)
        {
            fxaaMaterial.EnableKeyword("LOW_QUALITY");
        }
        else
        {
            fxaaMaterial.DisableKeyword("LOW_QUALITY");
        }

        if (gammaBlending)
        {
            fxaaMaterial.EnableKeyword("GAMMA_BLENDING");
        }
        else
        {
            fxaaMaterial.DisableKeyword("GAMMA_BLENDING");
        }



        if (luminancesource == LuminanceMode.Calculate)
        {
            fxaaMaterial.DisableKeyword("LUMINANCE_GREEN");
            RenderTexture luminanceTex = RenderTexture.GetTemporary(
                                        source.width, source.height, 0, source.format
                );

            Graphics.Blit(source, luminanceTex, fxaaMaterial, luminancePass);
            Graphics.Blit(luminanceTex, destination, fxaaMaterial, fxaaPass);
            RenderTexture.ReleaseTemporary(luminanceTex);
        }
        else
        {
            if (luminancesource == LuminanceMode.Green)
            {
                fxaaMaterial.EnableKeyword("LUMINANCE_GREEN");
            }
            else
            {
                fxaaMaterial.DisableKeyword("LUMINANCE_GREEN");
            }
            Graphics.Blit(source, destination,fxaaMaterial,fxaaPass);
        }






    }
}
