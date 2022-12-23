using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
[System.Serializable]
public class EventTest : MonoBehaviour
{

    public Caster caster;

    Action showPos;

    public UnityEvent KeyPress;

    private void OnEnable()
    {
        caster.OnShow += show;
    }

    private void OnDisable()
    {
        caster.OnShow -= show;
    }
    // Start is called before the first frame update
    void Start()
    {
         showPos += delegate {

            Debug.Log(transform.position);
	     };
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            showPos.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            if (KeyPress!=null)
            KeyPress.Invoke();
        }
    }

    void show()
    {
        Debug.Log(transform.gameObject.name);
    }
}
