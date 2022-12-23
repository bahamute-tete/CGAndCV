using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanoramaMapCapture : MonoBehaviour {
    public bool stereoscopic = false;
    public Camera targetCamera;
    public RenderTexture cubeMapRight;
    public RenderTexture cubMapLeft;
    public RenderTexture equirectMap;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CaptureMap();
        }
	}

    public void CaptureMap()
    {
        if (!stereoscopic)
        {
            targetCamera.RenderToCubemap(cubMapLeft);
            cubMapLeft.ConvertToEquirect(equirectMap);
        }
        else
        {
            targetCamera.stereoSeparation = 0.065f;
            targetCamera.RenderToCubemap(cubMapLeft,63,Camera.MonoOrStereoscopicEye.Left);
            targetCamera.RenderToCubemap(cubeMapRight, 63, Camera.MonoOrStereoscopicEye.Right);

            cubMapLeft.ConvertToEquirect(equirectMap, Camera.MonoOrStereoscopicEye.Left);
            cubeMapRight.ConvertToEquirect(equirectMap, Camera.MonoOrStereoscopicEye.Right);
        }

        SavePic(equirectMap);
    }

    public void SavePic(RenderTexture rt)
    {
        Texture2D tex = new Texture2D(rt.width, rt.height);

        RenderTexture.active = rt;

        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes = tex.EncodeToJPG();

        string path = Application.dataPath + "/Panoroma" + ".jpg";

        System.IO.File.WriteAllBytes(path, bytes);
    }
}
