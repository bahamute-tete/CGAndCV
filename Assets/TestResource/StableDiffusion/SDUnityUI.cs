using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SDUnityUI : MonoBehaviour
{

    [SerializeField] Button btn;
    [SerializeField] Image image;
    private SDWebUIAPI sdApi;
    private string response;
    // Start is called before the first frame update
    
    void Start()
    {
        sdApi = GetComponent<SDWebUIAPI>();
       

        btn.onClick.AddListener(delegate
        {
            StartCoroutine(getDatas());
            Debug.Log(response);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator getDatas()
    {
        response = sdApi.response;
        yield return  new WaitUntil(() => sdApi.response != null);

    }
}
