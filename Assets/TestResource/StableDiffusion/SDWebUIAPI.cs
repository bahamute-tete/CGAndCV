using MathNet.Numerics.Distributions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SDWebUIAPI : MonoBehaviour
{


    public string response;
    string data;


    [Serializable]
    public class SDImage
    {
        public List<string> images;
        public string parameters;
        public string info;
    }

    [System.Serializable]
    public class RequestPayload
    {
       
        public string prompt;
        public int steps;
        public int cfg_scale;
        public int width;
        public int height;
       
    }


    // Start is called before the first frame update
    void Start()
    {


        RequestPayload payload = new RequestPayload();
       
        payload.prompt = "a girl with red hat";
        payload.steps = 25;
        payload.cfg_scale = 7;
        payload.width = 512;
        payload.height = 512;


        data = JsonUtility.ToJson(payload);
        Debug.Log(data);


       
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SdApi()
    {
        string baseUrl = "http://127.0.0.1:7860/sdapi/v1/sd-models";

        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                var json = request.downloadHandler.text;
                Debug.Log(json);



            }
        }
    }


    public IEnumerator txt2img()
    {
        string baseUrl = "http://127.0.0.1:7860/sdapi/v1/txt2img";

        byte[] databyte = Encoding.UTF8.GetBytes(data);
        using (UnityWebRequest request = new UnityWebRequest(baseUrl, "POST"))
        { 
            request.uploadHandler = new UploadHandlerRaw(databyte);
            request.downloadHandler = new DownloadHandlerBuffer();
        
            request.SetRequestHeader("Content-Type","application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                response = request.downloadHandler.text;
                //var  imgdatas = JsonUtility.FromJson<SDImage>(response);
                ////Debug.Log(imgdatas.images[0]);
                //byte[] images_bytes = Convert.FromBase64String(imgdatas.images[0]);
                //StartCoroutine(trans2Tex2D(images_bytes));

             }
        }

    }


    IEnumerator  trans2Tex2D(byte[] img)
    {
        Texture2D texture = new Texture2D(512, 512, TextureFormat.ARGB32, false);
        texture.LoadImage(img);
        yield return new WaitForEndOfFrame();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f));
        //image.sprite = sprite;
    }

   
}
