using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpectrumGet : MonoBehaviour
{
    const float pi = 3.1415f;
    AudioSource audioSource;
    [SerializeField]AudioClip audioClip;
    [SerializeField] Button playBtn;
    [SerializeField] Button stopBtn;
    [SerializeField]float[] samples = new float[64];

    GameObject musicLineContent;
    LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;

        playBtn.onClick.AddListener(delegate
        {
            audioSource.Play();
        });

        stopBtn.onClick.AddListener(delegate
        {
            audioSource.Stop();
        });

        musicLineContent = new GameObject("MusicLineContent");
        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = samples.Length;
        lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lr.material.color = Color.red;
        lr.startWidth = lr.endWidth = 0.015f;



    }

    // Update is called once per frame
    void Update()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        

        for (int i = 1; i < samples.Length; i++)
        {
            float n  = samples[i]*10*(i-1);
            float h = (float)(i-1) / samples.Length;
            float x = (2.0f+n) * Mathf.Cos(2 * h * pi);
            float y = (2.0f+n) * Mathf.Sin(2 * h * pi);
            Vector3 cp = new Vector3(x, y, 0);
            lr.SetPosition(i-1, cp);
        }
       
    }


    float RemapValue (float a ,float b ,float t)
    {
        float res = (t - b) / (b - a);
        return res;

    }
}
