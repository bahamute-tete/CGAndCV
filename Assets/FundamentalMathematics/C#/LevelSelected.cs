using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelected : MonoBehaviour
{
    [SerializeField] GameObject btnGRPS;
    [SerializeField] Button[] buttons;
    int n;
    private void Awake()
    {
        n = btnGRPS.transform.childCount;
        buttons = new Button[n];

        for (int i = 0; i < n; i++)
        {
            buttons[i] = btnGRPS.transform.GetChild(i).GetComponent<Button>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {

        foreach (var o in buttons)
        {
            o.onClick.AddListener(delegate
            {
                int index = o.transform.GetSiblingIndex();
                StartCoroutine(loadLevel(index + 2));
            });
        }


        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator loadLevel(int i)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(i);

        while (!async.isDone)
        {
            yield return null;
        }
    }
}
