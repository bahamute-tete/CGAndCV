using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class Caster : MonoBehaviour
{
    [SerializeField] Button btn;
    public delegate void ShowMessage();
    public event ShowMessage OnShow;

   
    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(delegate
        {
            OnShow();
            
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }


  
    
}
