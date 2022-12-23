using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIButtonTest : MonoBehaviour
{

    [SerializeField]TextMeshProUGUI textMeshPro;
    Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayHint()
    {
        textMeshPro.text = $"ButtonName =={button.name}";
    }
}
