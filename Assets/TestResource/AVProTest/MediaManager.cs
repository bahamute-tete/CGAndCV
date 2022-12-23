using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using UnityEngine.UI;
using System;

public class MediaManager : MonoBehaviour
{
    [SerializeField] MediaPlayer player;
    [SerializeField] StreamList pathList;
    MediaPathType type = MediaPathType.AbsolutePathOrURL;
    MediaPath path;
    [SerializeField] GameObject Screen;


    [SerializeField] Button nextBtn, preBtn;
    [SerializeField] Button playBtn;

    int index = 0;

    bool isPause = false;

    Material meshMat;
   
    private void Awake()
    {
        player = GetComponent<MediaPlayer>();
        meshMat = Screen.GetComponent<MeshRenderer>().material;


    }

    private void HandleEvent(MediaPlayer arg0, MediaPlayerEvent.EventType arg1, ErrorCode arg2)
    {
   

        if (arg1 == MediaPlayerEvent.EventType.ReadyToPlay)
        {

            Texture2D firstFrame = player.ExtractFrame(null, 1);
            meshMat.SetTexture("_MainTex",firstFrame);

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        player.Events.AddListener(HandleEvent);

        nextBtn.onClick.AddListener(delegate {
            if (index<pathList.path.Count)
            index++;
            MediaPath mediaPath = new MediaPath(pathList.path[index], MediaPathType.AbsolutePathOrURL);
            player.OpenMedia(mediaPath, autoPlay: false);

        });

        preBtn.onClick.AddListener(delegate {
            if (index > 0)
                index--;

            MediaPath mediaPath = new MediaPath(pathList.path[index], MediaPathType.AbsolutePathOrURL);
            player.OpenMedia(mediaPath, autoPlay: false);
        });

        playBtn.onClick.AddListener(delegate {

            if (!isPause)
            {
                player.Pause();
                isPause = true;
            }
            else
            {
                player.Play();
                isPause = false;
            }
           
        });

       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
