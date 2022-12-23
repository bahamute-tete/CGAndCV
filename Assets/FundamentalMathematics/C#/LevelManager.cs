using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Button btn;
    
    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(delegate
        {
            StartCoroutine(LoadLevel());

        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator LoadLevel()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("MainUI", LoadSceneMode.Single);

        while (!async.isDone)
        {
            yield return null;
        }
    }
}
